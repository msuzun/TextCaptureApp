namespace TextCaptureApp.Core.Models;

/// <summary>
/// Kullanıcının seçtiği ekran bölgesini temsil eder
/// </summary>
public class RegionSelectionResult
{
    /// <summary>
    /// X koordinatı
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y koordinatı
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Genişlik
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Yükseklik
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Kullanıcı seçimi iptal etti mi?
    /// </summary>
    public bool IsCancelled { get; set; }
}

