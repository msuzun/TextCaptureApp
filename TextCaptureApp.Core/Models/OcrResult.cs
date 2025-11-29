namespace TextCaptureApp.Core.Models;

/// <summary>
/// OCR ile çıkarılan metin sonucunu temsil eder
/// </summary>
public class OcrResult
{
    /// <summary>
    /// Çıkarılan metin
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// OCR güven skoru (0.0 - 1.0 arası, opsiyonel)
    /// </summary>
    public double? Confidence { get; set; }

    /// <summary>
    /// Tespit edilen veya kullanılan dil kodu (örn: 'eng', 'tur')
    /// </summary>
    public string? Language { get; set; }
}

