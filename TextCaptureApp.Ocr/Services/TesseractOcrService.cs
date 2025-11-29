using System.Drawing;
using Tesseract;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Ocr.Services;

/// <summary>
/// Tesseract 5.x OCR tabanlı metin çıkarma servisi
/// Multi-language support ve optimized resource management
/// </summary>
public class TesseractOcrService : IOcrService, IDisposable
{
    private readonly string _tessDataPath;
    private readonly string _defaultLanguage;
    private readonly Dictionary<string, TesseractEngine> _engineCache;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// TesseractOcrService constructor
    /// </summary>
    /// <param name="tessDataPath">tessdata klasör yolu (traineddata dosyalarının bulunduğu yer)</param>
    /// <param name="defaultLanguage">Varsayılan dil(ler), örn: "tur+eng" (çoklu dil için + ile ayır)</param>
    public TesseractOcrService(string tessDataPath = "./tessdata", string defaultLanguage = "tur+eng")
    {
        if (string.IsNullOrWhiteSpace(tessDataPath))
            throw new ArgumentNullException(nameof(tessDataPath));

        _tessDataPath = tessDataPath;
        _defaultLanguage = defaultLanguage ?? "tur+eng";
        _engineCache = new Dictionary<string, TesseractEngine>();

        ValidateTessDataPath();
    }

    public async Task<OcrResult> ExtractTextAsync(
        ImageCaptureResult image, 
        CancellationToken cancellationToken = default)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (image.ImageStream == null)
            throw new ArgumentException("ImageStream cannot be null", nameof(image));

        try
        {
            // Stream'i başa al (multiple read durumunda)
            if (image.ImageStream.CanSeek)
                image.ImageStream.Position = 0;

            var engine = await GetOrCreateEngineAsync(_defaultLanguage, cancellationToken);

            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Stream → Bitmap → Pix conversion
                using var bitmap = new Bitmap(image.ImageStream);
                using var pix = ConvertBitmapToPix(bitmap);

                // OCR processing
                using var page = engine.Process(pix, PageSegMode.Auto);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                return new OcrResult
                {
                    Text = text.TrimEnd(), // Trailing whitespace temizle
                    Confidence = confidence,
                    Language = _defaultLanguage
                };
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"OCR işlemi başarısız oldu. Dil: {_defaultLanguage}, Hata: {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Bitmap'i Tesseract Pix formatına çevirir
    /// </summary>
    private Pix ConvertBitmapToPix(Bitmap bitmap)
    {
        using var tempMs = new MemoryStream();
        
        // PNG formatında serialize et (lossless)
        bitmap.Save(tempMs, System.Drawing.Imaging.ImageFormat.Png);
        tempMs.Position = 0;
        
        // Pix'e çevir
        var pixData = tempMs.ToArray();
        return Pix.LoadFromMemory(pixData);
    }

    /// <summary>
    /// Belirtilen dil için TesseractEngine'i getirir veya oluşturur
    /// Engine'ler cache'lenir (performance optimization)
    /// </summary>
    private async Task<TesseractEngine> GetOrCreateEngineAsync(
        string language, 
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_engineCache.TryGetValue(language, out var cachedEngine))
                return cachedEngine;
        }

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                // Double-check locking pattern
                if (_engineCache.TryGetValue(language, out var cachedEngine))
                    return cachedEngine;

                try
                {
                    var engine = new TesseractEngine(
                        _tessDataPath, 
                        language, 
                        EngineMode.Default);

                    // Tesseract configuration
                    engine.DefaultPageSegMode = PageSegMode.Auto;

                    _engineCache[language] = engine;
                    return engine;
                }
                catch (TesseractException ex)
                {
                    throw new InvalidOperationException(
                        $"Tesseract engine başlatılamadı.\n" +
                        $"Tessdata yolu: '{_tessDataPath}'\n" +
                        $"Dil: '{language}'\n" +
                        $"Gerekli dosya: '{language}.traineddata'\n" +
                        $"Lütfen tessdata klasörünü ve dil dosyalarını kontrol edin.",
                        ex);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// tessdata klasörünün varlığını doğrular
    /// </summary>
    private void ValidateTessDataPath()
    {
        if (!Directory.Exists(_tessDataPath))
        {
            throw new DirectoryNotFoundException(
                $"tessdata klasörü bulunamadı: '{_tessDataPath}'\n" +
                $"Lütfen Tesseract dil dosyalarını (*.traineddata) bu klasöre yerleştirin.\n" +
                $"İndirme: https://github.com/tesseract-ocr/tessdata");
        }
    }

    /// <summary>
    /// IDisposable implementation - Tüm TesseractEngine'leri temizler
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var engine in _engineCache.Values)
            {
                engine?.Dispose();
            }
            _engineCache.Clear();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
