using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Kullanıcıya ekran bölgesi seçtiren UI component'i için contract
/// UI katmanında implement edilir (örn: WPF overlay window)
/// </summary>
public interface IRegionSelector
{
    /// <summary>
    /// Kullanıcıya ekran bölgesi seçtirir (örn: overlay window ile)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Seçilen bölge veya iptal durumunda IsCancelled=true</returns>
    Task<RegionSelectionResult> SelectRegionAsync(CancellationToken cancellationToken = default);
}

