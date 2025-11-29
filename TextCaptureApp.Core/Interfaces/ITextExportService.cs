using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Metin export servisini tanımlar (PDF, DOCX, TXT)
/// </summary>
public interface ITextExportService
{
    /// <summary>
    /// Metni belirtilen formatta export eder
    /// </summary>
    Task<bool> ExportAsync(string text, ExportOptions options);

    /// <summary>
    /// Export formatının desteklenip desteklenmediğini kontrol eder
    /// </summary>
    bool IsFormatSupported(ExportFormat format);

    /// <summary>
    /// Dosya kaydetmek için geçerli bir path olup olmadığını doğrular
    /// </summary>
    bool ValidateFilePath(string filePath, ExportFormat format);
}

