namespace TextCaptureApp.Core.Models;

/// <summary>
/// Export işlemi için seçenekleri tanımlar
/// </summary>
public class ExportOptions
{
    public ExportFormat Format { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IncludeTimestamp { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

public enum ExportFormat
{
    Txt,
    Pdf,
    Docx
}

