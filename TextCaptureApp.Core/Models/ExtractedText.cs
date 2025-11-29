namespace TextCaptureApp.Core.Models;

/// <summary>
/// OCR ile çıkarılan metni temsil eder
/// </summary>
public class ExtractedText
{
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Language { get; set; } = "eng";
    public DateTime ExtractedAt { get; set; }
    public CapturedImage? SourceImage { get; set; }
}

