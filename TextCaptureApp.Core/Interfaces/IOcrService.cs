using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// OCR (Optical Character Recognition) servisini tanımlar
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Görüntüden metin çıkarır
    /// </summary>
    Task<OcrResult> ExtractTextAsync(ImageCaptureResult image, string language = "eng");

    /// <summary>
    /// Desteklenen dilleri döndürür
    /// </summary>
    Task<IEnumerable<string>> GetSupportedLanguagesAsync();

    /// <summary>
    /// OCR engine'inin hazır olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsReadyAsync();
}

