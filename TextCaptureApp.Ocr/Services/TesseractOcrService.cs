using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Ocr.Services;

/// <summary>
/// Tesseract OCR tabanlı metin çıkarma servisi
/// </summary>
public class TesseractOcrService : IOcrService, IDisposable
{
    private readonly string _tessDataPath;
    private TesseractEngine? _engine;
    private bool _disposed;

    public TesseractOcrService(string tessDataPath = "./tessdata")
    {
        _tessDataPath = tessDataPath;
    }

    public async Task<OcrResult> ExtractTextAsync(ImageCaptureResult image, string language = "eng")
    {
        await EnsureEngineInitializedAsync(language);

        return await Task.Run(() =>
        {
            using var ms = new MemoryStream(image.ImageData);
            using var bitmap = new Bitmap(ms);
            
            // Save bitmap temporarily and load as Pix
            using var tempMs = new MemoryStream();
            bitmap.Save(tempMs, System.Drawing.Imaging.ImageFormat.Png);
            tempMs.Position = 0;
            
            using var pix = Pix.LoadFromMemory(tempMs.ToArray());
            using var page = _engine!.Process(pix);

            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            return new OcrResult
            {
                Text = text,
                Confidence = confidence,
                Language = language,
                ExtractedAt = DateTime.Now,
                SourceImage = image
            };
        });
    }

    public Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        // Tesseract desteklenen diller: eng, tur, deu, fra, spa vb.
        // tessdata klasöründe bulunan .traineddata dosyalarına göre
        var languages = new[] { "eng", "tur", "deu", "fra", "spa", "ita" };
        return Task.FromResult<IEnumerable<string>>(languages);
    }

    public Task<bool> IsReadyAsync()
    {
        try
        {
            // tessdata klasörünün var olup olmadığını kontrol et
            return Task.FromResult(Directory.Exists(_tessDataPath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private async Task EnsureEngineInitializedAsync(string language)
    {
        if (_engine != null) return;

        await Task.Run(() =>
        {
            try
            {
                _engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Tesseract engine başlatılamadı. tessdata klasörünün '{_tessDataPath}' yolunda ve '{language}.traineddata' dosyasının mevcut olduğundan emin olun.", 
                    ex);
            }
        });
    }

    public void Dispose()
    {
        if (_disposed) return;

        _engine?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

