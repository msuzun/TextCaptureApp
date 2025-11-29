namespace TextCaptureApp.Core.Models;

/// <summary>
/// OCR ile çıkarılan metin sonucunu temsil eder
/// </summary>
public class OcrResult
{
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Language { get; set; } = "eng";
    public DateTime ExtractedAt { get; set; }
    public ImageCaptureResult? SourceImage { get; set; }
}

