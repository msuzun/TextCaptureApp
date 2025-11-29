using System.Drawing;
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
    private readonly string _defaultLanguage;
    private TesseractEngine? _engine;
    private bool _disposed;

    public TesseractOcrService(string tessDataPath = "./tessdata", string defaultLanguage = "eng")
    {
        _tessDataPath = tessDataPath;
        _defaultLanguage = defaultLanguage;
    }

    public async Task<OcrResult> ExtractTextAsync(ImageCaptureResult image, CancellationToken cancellationToken = default)
    {
        await EnsureEngineInitializedAsync(cancellationToken);

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = new Bitmap(image.ImageStream);
            
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
                Language = _defaultLanguage
            };
        }, cancellationToken);
    }

    private async Task EnsureEngineInitializedAsync(CancellationToken cancellationToken)
    {
        if (_engine != null) return;

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _engine = new TesseractEngine(_tessDataPath, _defaultLanguage, EngineMode.Default);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Tesseract engine başlatılamadı. tessdata klasörünün '{_tessDataPath}' yolunda ve '{_defaultLanguage}.traineddata' dosyasının mevcut olduğundan emin olun.", 
                    ex);
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _engine?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
