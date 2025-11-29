using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Ekran görüntüsü yakalama ve dosya yükleme servisini tanımlar
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>
    /// Kullanıcının seçtiği ekran bölgesini yakalar
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yakalanan görüntü veya kullanıcı iptal ettiyse null</returns>
    Task<ImageCaptureResult?> CaptureRegionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosyadan görüntü yükler
    /// </summary>
    /// <param name="filePath">Görüntü dosya yolu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yüklenen görüntü veya hata durumunda null</returns>
    Task<ImageCaptureResult?> CaptureFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}

