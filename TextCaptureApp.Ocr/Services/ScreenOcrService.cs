using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;
using Size = OpenCvSharp.Size;

namespace TextCaptureApp.Ocr.Services;

/// <summary>
/// Ekran görüntüleri (monitör fotoğrafları) için özel OCR servisi
/// Renk tersliği (inverted colors) ve Moiré pattern sorunlarını çözer
/// </summary>
public class ScreenOcrService : IOcrService, IDisposable
{
    private readonly string _tessDataPath;
    private readonly string _defaultLanguage;
    private readonly Dictionary<string, TesseractEngine> _engineCache;
    private readonly object _lock = new();
    private bool _disposed;

    public ScreenOcrService(string tessDataPath = "./tessdata", string defaultLanguage = "eng+tur")
    {
        _tessDataPath = tessDataPath;
        _defaultLanguage = defaultLanguage;
        _engineCache = new Dictionary<string, TesseractEngine>();
    }

    public async Task<OcrResult> ExtractTextAsync(ImageCaptureResult image, CancellationToken cancellationToken = default)
    {
        if (image?.ImageStream == null) 
            throw new ArgumentNullException(nameof(image));

        if (image.ImageStream.CanSeek) 
            image.ImageStream.Position = 0;

        var engine = GetOrCreateEngine(_defaultLanguage);

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Stream'den OpenCV Mat formatına
            using var memoryStream = new MemoryStream();
            image.ImageStream.CopyTo(memoryStream);
            var imageBytes = memoryStream.ToArray();
            using var srcMat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

            if (srcMat.Empty())
            {
                throw new InvalidOperationException("Görüntü yüklenemedi veya boş.");
            }

            // Görüntü çok büyükse biraz küçült (Monitör fotoları bazen 4000px olur, gürültü artar)
            // Ancak çok küçükse büyüt. İdeal genişlik ~1500-2000px arasıdır.
            if (srcMat.Width > 2500)
                Cv2.Resize(srcMat, srcMat, new Size(srcMat.Width / 2, srcMat.Height / 2));
            else if (srcMat.Width < 800)
                Cv2.Resize(srcMat, srcMat, new Size(srcMat.Width * 2, srcMat.Height * 2), 0, 0, InterpolationFlags.Lanczos4);

            // 1. Temel Gürültü Temizleme (Grayscale öncesi)
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);

            // Bilateral Filter: Monitör piksellerini (ızgarayı) yok ederken metin kenarlarını korur.
            // Bu adım "sdeprasasiyriny..." gibi halüsinasyonları engeller.
            var denoisedMat = new Mat();
            Cv2.BilateralFilter(grayMat, denoisedMat, 15, 80, 80);

            // 2. Thresholding (Siyah-Beyaz yapma) - Otsu metodu genelde en temizidir
            var binaryMat = new Mat();
            Cv2.Threshold(denoisedMat, binaryMat, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            // 3. İKİLİ STRATEJİ: Normal ve Inverted (Ters) Görüntü
            // Seçili metin (Mavi üzerine Beyaz) normalde okunmaz.
            // Ters çevrilince (Sarı/Beyaz üzerine Siyah) olur ve okunur.
            var invertedMat = new Mat();
            Cv2.BitwiseNot(binaryMat, invertedMat); // Renkleri ters çevir

            // Listeye her iki versiyonu da ekle
            var candidates = new List<Mat> { binaryMat, invertedMat };
            
            OcrResult? bestResult = null;

            foreach (var matToProcess in candidates)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Kenar boşluğu ekle (Tesseract kenara yapışık yazıyı sevmez)
                    var borderedMat = new Mat();
                    Cv2.CopyMakeBorder(matToProcess, borderedMat, 50, 50, 50, 50, BorderTypes.Constant, Scalar.White);

                    using var pix = ConvertMatToPix(borderedMat);
                    
                    // Auto modu sayfa yapısını çözmeye çalışır.
                    using var page = engine.Process(pix, PageSegMode.Auto);
                    
                    var text = page.GetText()?.Trim();
                    var meanConfidence = page.GetMeanConfidence();

                    // Basit bir filtre: Metin çok kısaysa veya güven düşükse atla
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 3)
                    {
                        // En iyi sonucu sakla
                        if (bestResult == null || 
                            (bestResult.Confidence == null || meanConfidence > bestResult.Confidence.Value))
                        {
                            bestResult = new OcrResult
                            {
                                Text = text,
                                Confidence = meanConfidence,
                                Language = _defaultLanguage
                            };
                        }
                    }

                    borderedMat.Dispose();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch 
                { 
                    continue; 
                }
            }
            
            // Temizlik
            grayMat.Dispose();
            denoisedMat.Dispose();
            binaryMat.Dispose();
            invertedMat.Dispose();

            return bestResult ?? new OcrResult { Text = "", Confidence = 0, Language = _defaultLanguage };
        }, cancellationToken);
    }

    private Pix ConvertMatToPix(Mat mat)
    {
        using var ms = mat.ToMemoryStream(".png");
        ms.Position = 0;
        return Pix.LoadFromMemory(ms.ToArray());
    }

    private TesseractEngine GetOrCreateEngine(string language)
    {
        lock (_lock)
        {
            if (_engineCache.TryGetValue(language, out var engine)) 
                return engine;
            
            var newEngine = new TesseractEngine(_tessDataPath, language, EngineMode.LstmOnly); // LSTM Only daha başarılı
            
            // --- EKRAN OKUMA İÇİN ÖZEL AYARLAR ---
            
            // Çöp karakterleri azaltmak için
            newEngine.SetVariable("debug_file", "/dev/null");
            
            // Eğer sadece o URL'yi okumak istiyorsan ve diğer gürültüleri atmak istiyorsan:
            // newEngine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.:/-*");
            
            _engineCache[language] = newEngine;
            return newEngine;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var engine in _engineCache.Values) 
                engine?.Dispose();

            _engineCache.Clear();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

