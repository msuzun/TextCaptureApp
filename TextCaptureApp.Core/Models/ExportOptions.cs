namespace TextCaptureApp.Core.Models;

/// <summary>
/// Export işlemi için seçenekleri tanımlar
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Export edilecek dosya yolu
    /// </summary>
    public required string OutputPath { get; set; }

    /// <summary>
    /// Export formatı
    /// </summary>
    public TextExportFormat Format { get; set; }
}

/// <summary>
/// Metin export formatları
/// </summary>
public enum TextExportFormat
{
    Txt,
    Pdf,
    Docx
}

