using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Ekran görüntüsü yakalama servisini tanımlar
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>
    /// Tüm ekranın görüntüsünü yakalar
    /// </summary>
    Task<CapturedImage> CaptureFullScreenAsync();

    /// <summary>
    /// Belirli bir alanın görüntüsünü yakalar
    /// </summary>
    Task<CapturedImage> CaptureRegionAsync(int x, int y, int width, int height);

    /// <summary>
    /// Kullanıcının seçtiği alanı yakalar (interaktif)
    /// </summary>
    Task<CapturedImage> CaptureSelectedRegionAsync();
}

