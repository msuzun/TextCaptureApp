using System.Text;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Services;

/// <summary>
/// TXT formatında export servisi
/// </summary>
public class TxtExportService : ITextExportService
{
    public async Task<bool> ExportAsync(string text, ExportOptions options)
    {
        try
        {
            await File.WriteAllTextAsync(options.FilePath, text, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsFormatSupported(ExportFormat format)
    {
        return format == ExportFormat.Txt;
    }

    public bool ValidateFilePath(string filePath, ExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
    }
}

