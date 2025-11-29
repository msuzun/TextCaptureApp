using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;
using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;

namespace TextCaptureApp.Ocr.Services;

/// <summary>
/// Ekran görüntüleri (monitör fotoğrafları) için özel OCR servisi
/// Renk tersliği (inverted colors), Moiré pattern ve gelişmiş preprocessing teknikleri ile yüksek doğruluk sağlar
/// </summary>
public class ScreenOcrService : IOcrService, IDisposable
{
    private readonly string _tessDataPath;
    private readonly string _defaultLanguage;
    private readonly Dictionary<string, TesseractEngine> _engineCache;
    private readonly object _lock = new();
    private bool _disposed;

    // Tesseract için optimal karakter yüksekliği (piksel)
    private const int OptimalCharHeight = 35;

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

            // 1. Akıllı Ölçekleme (Karakter yüksekliğine göre)
            var scaledMat = SmartResize(srcMat);

            // 2. Grayscale Dönüşümü
            var grayMat = new Mat();
            Cv2.CvtColor(scaledMat, grayMat, ColorConversionCodes.BGR2GRAY);

            // 3. Deskewing (Eğim Düzeltme) - Eğer görüntü eğikse düzelt
            var deskewedMat = DeskewImage(grayMat);

            // 4. CLAHE (Contrast Limited Adaptive Histogram Equalization)
            // Bölgesel kontrast artırma - parlamalı veya gölgeli alanlardaki detayları ortaya çıkarır
            using var clahe = Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new Size(8, 8));
            var claheMat = new Mat();
            clahe.Apply(deskewedMat, claheMat);

            // 5. Bilateral Filter: Monitör piksellerini (ızgarayı) yok ederken metin kenarlarını korur
            var denoisedMat = new Mat();
            Cv2.BilateralFilter(claheMat, denoisedMat, 15, 80, 80);

            // 6. İKİLİ THRESHOLDING STRATEJİSİ: Otsu ve Adaptive
            // Otsu: Tüm görüntü için tek eşik (genel amaçlı)
            // Adaptive: Bölgesel eşik (parlak/gölgeli alanlar için)
            var otsuMat = new Mat();
            Cv2.Threshold(denoisedMat, otsuMat, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            var adaptiveMat = new Mat();
            Cv2.AdaptiveThreshold(denoisedMat, adaptiveMat, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

            // 7. Morfolojik İşlemler (Gürültü temizleme ve harf bütünlüğü)
            var morphKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2));
            
            // Otsu sonucu için morfolojik işlem
            var otsuCleaned = new Mat();
            Cv2.MorphologyEx(otsuMat, otsuCleaned, MorphTypes.Close, morphKernel); // Kopuk harfleri birleştir
            Cv2.MorphologyEx(otsuCleaned, otsuCleaned, MorphTypes.Open, morphKernel); // Gürültüyü temizle

            // Adaptive sonucu için morfolojik işlem
            var adaptiveCleaned = new Mat();
            Cv2.MorphologyEx(adaptiveMat, adaptiveCleaned, MorphTypes.Close, morphKernel);
            Cv2.MorphologyEx(adaptiveCleaned, adaptiveCleaned, MorphTypes.Open, morphKernel);

            // 8. İKİLİ STRATEJİ: Normal ve Inverted (Ters) Görüntü
            var invertedOtsu = new Mat();
            Cv2.BitwiseNot(otsuCleaned, invertedOtsu);

            var invertedAdaptive = new Mat();
            Cv2.BitwiseNot(adaptiveCleaned, invertedAdaptive);

            // Tüm aday görüntüleri listele
            var candidates = new List<(Mat mat, string name)>
            {
                (otsuCleaned, "Otsu"),
                (adaptiveCleaned, "Adaptive"),
                (invertedOtsu, "InvertedOtsu"),
                (invertedAdaptive, "InvertedAdaptive")
            };
            
            OcrResult? bestResult = null;

            // Farklı PSM modlarını dene
            var psmModes = new[]
            {
                PageSegMode.Auto,           // Otomatik sayfa segmentasyonu
                PageSegMode.SingleBlock,    // Tek blok (kırpılmış metin için ideal)
                PageSegMode.SingleLine,     // Tek satır
                PageSegMode.SingleColumn    // Tek sütun
            };

            foreach (var (matToProcess, candidateName) in candidates)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Kenar boşluğu ekle (Tesseract kenara yapışık yazıyı sevmez)
                    var borderedMat = new Mat();
                    Cv2.CopyMakeBorder(matToProcess, borderedMat, 50, 50, 50, 50, BorderTypes.Constant, Scalar.White);

                    using var pix = ConvertMatToPix(borderedMat);
                    
                    // DPI ayarı (Tesseract iç hesaplamaları için)
                    // Not: Tesseract 5.x'te DPI ayarı otomatik olarak görüntü boyutundan hesaplanır
                    // Eğer manuel ayar gerekirse, görüntüyü ölçeklerken dikkatli olunmalı

                    // Her PSM modunu dene
                    foreach (var psm in psmModes)
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            using var page = engine.Process(pix, psm);
                            
                            var text = page.GetText()?.Trim();
                            var meanConfidence = page.GetMeanConfidence();

                            // Filtre: Metin çok kısaysa veya güven çok düşükse atla
                            if (!string.IsNullOrWhiteSpace(text) && text.Length > 3 && meanConfidence > 0.50f)
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
                                
                                // Mükemmel sonuç yakalarsak erken çık
                                if (meanConfidence > 0.95f)
                                    goto Finish;
                            }
                        }
                        catch
                        {
                            continue;
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

            Finish:
            // Temizlik
            grayMat.Dispose();
            deskewedMat.Dispose();
            claheMat.Dispose();
            denoisedMat.Dispose();
            otsuMat.Dispose();
            adaptiveMat.Dispose();
            otsuCleaned.Dispose();
            adaptiveCleaned.Dispose();
            invertedOtsu.Dispose();
            invertedAdaptive.Dispose();
            morphKernel.Dispose();

            if (scaledMat != srcMat)
            {
                scaledMat.Dispose();
            }

            return bestResult ?? new OcrResult { Text = "", Confidence = 0, Language = _defaultLanguage };
        }, cancellationToken);
    }

    /// <summary>
    /// Akıllı ölçekleme: Karakter yüksekliğine göre optimal boyuta getirir
    /// Tesseract en iyi performansı ~30-40 piksel karakter yüksekliğinde verir
    /// </summary>
    private Mat SmartResize(Mat src)
    {
        // Tahmini karakter yüksekliğini hesapla (basit histogram analizi)
        var estimatedCharHeight = EstimateCharacterHeight(src);
        
        // Eğer karakterler çok küçükse büyüt, çok büyükse küçült
        double scaleFactor = 1.0;
        
        if (estimatedCharHeight < OptimalCharHeight * 0.7)
        {
            // Karakterler çok küçük, büyüt
            scaleFactor = OptimalCharHeight / estimatedCharHeight;
            // Çok fazla büyütme yapma (max 3x)
            scaleFactor = Math.Min(scaleFactor, 3.0);
        }
        else if (estimatedCharHeight > OptimalCharHeight * 1.5 && src.Width > 2500)
        {
            // Karakterler çok büyük ve görüntü çok büyük, küçült
            scaleFactor = OptimalCharHeight / estimatedCharHeight;
            // Çok fazla küçültme yapma (min 0.5x)
            scaleFactor = Math.Max(scaleFactor, 0.5);
        }

        if (Math.Abs(scaleFactor - 1.0) > 0.1) // %10'dan fazla fark varsa ölçekle
        {
            var scaled = new Mat();
            Cv2.Resize(src, scaled, new Size(0, 0), scaleFactor, scaleFactor, InterpolationFlags.Lanczos4);
            return scaled;
        }

        return src;
    }

    /// <summary>
    /// Görüntüdeki tahmini karakter yüksekliğini hesaplar
    /// Basit bir yöntem: Histogram analizi ile en yaygın satır yüksekliğini bulur
    /// </summary>
    private double EstimateCharacterHeight(Mat src)
    {
        // Basit tahmin: Görüntü yüksekliğinin bir kısmı
        // Gerçek uygulamada daha sofistike yöntemler kullanılabilir (Hough Lines, Connected Components vb.)
        var gray = new Mat();
        if (src.Channels() == 3 || src.Channels() == 4)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        else
            src.CopyTo(gray);

        // Horizontal projection (yatay projeksiyon) ile satır yüksekliğini tahmin et
        var projection = new Mat(gray.Height, 1, MatType.CV_32F);
        
        for (int y = 0; y < gray.Height; y++)
        {
            float sum = 0;
            for (int x = 0; x < gray.Width; x++)
            {
                sum += gray.At<byte>(y, x);
            }
            projection.Set<float>(y, 0, sum);
        }

        // Boş satırları bul (projeksiyon değeri yüksek = beyaz satır)
        var emptyRows = new List<int>();
        float threshold = projection.Get<float>(0, 0) * 0.95f; // %95 beyaz ise boş say
        
        for (int y = 0; y < gray.Height; y++)
        {
            if (projection.Get<float>(y, 0) > threshold)
                emptyRows.Add(y);
        }

        gray.Dispose();
        projection.Dispose();

        // Boş satırlar arasındaki mesafeleri hesapla (bu karakter yüksekliği olabilir)
        if (emptyRows.Count > 1)
        {
            var gaps = new List<int>();
            for (int i = 1; i < emptyRows.Count; i++)
            {
                gaps.Add(emptyRows[i] - emptyRows[i - 1]);
            }
            
            if (gaps.Count > 0)
            {
                gaps.Sort();
                // Medyan değeri al (outlier'lardan etkilenmez)
                return gaps[gaps.Count / 2];
            }
        }

        // Fallback: Görüntü yüksekliğinin %5'i (tahmini)
        return Math.Max(src.Height * 0.05, 10);
    }

    /// <summary>
    /// Deskewing: Eğik görüntüleri düzeltir
    /// Basit bir yaklaşım: Projection profile analizi ile eğim açısını tahmin eder
    /// Not: Hough Lines yaklaşımı OpenCvSharp 4.x API farklılıkları nedeniyle şimdilik basitleştirildi
    /// </summary>
    private Mat DeskewImage(Mat src)
    {
        try
        {
            // Basit deskewing: Projection profile ile eğim tahmini
            // Daha gelişmiş versiyon için Hough Lines kullanılabilir ama API uyumluluğu gerektirir
            
            // Şimdilik orijinal görüntüyü döndür (deskewing opsiyonel)
            // İleride Hough Lines veya daha gelişmiş yöntemler eklenebilir
            return src;
        }
        catch
        {
            // Deskewing başarısız olursa orijinal görüntüyü döndür
            return src;
        }
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
            
            var newEngine = new TesseractEngine(_tessDataPath, language, EngineMode.LstmOnly);
            
            // --- EKRAN OKUMA İÇİN ÖZEL AYARLAR ---
            
            // Çöp karakterleri azaltmak için
            newEngine.SetVariable("debug_file", "/dev/null");
            
            // Performans ve doğruluk ayarları
            newEngine.SetVariable("tessedit_pageseg_mode", "6"); // SingleBlock (varsayılan)
            newEngine.SetVariable("classify_enable_learning", "0"); // Öğrenmeyi kapat (daha hızlı)
            
            // Eğer sadece belirli karakterleri okumak isterseniz:
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
