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
    /// <param name="image">İşlenecek görüntü</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>OCR sonucu</returns>
    Task<OcrResult> ExtractTextAsync(ImageCaptureResult image, CancellationToken cancellationToken = default);
}

