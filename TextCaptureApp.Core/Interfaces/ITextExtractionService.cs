using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// OCR tabanlı metin çıkarma servisini tanımlar
/// </summary>
public interface ITextExtractionService
{
    /// <summary>
    /// Görüntüden metin çıkarır
    /// </summary>
    Task<ExtractedText> ExtractTextAsync(CapturedImage image, string language = "eng");

    /// <summary>
    /// Desteklenen dilleri döndürür
    /// </summary>
    Task<IEnumerable<string>> GetSupportedLanguagesAsync();

    /// <summary>
    /// OCR engine'inin hazır olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsReadyAsync();
}

