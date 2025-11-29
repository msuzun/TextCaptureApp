using System.Text;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Services;

/// <summary>
/// TXT formatında export servisi
/// </summary>
public class TxtExportService : ITextExportService
{
    public async Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await File.WriteAllTextAsync(options.OutputPath, text, Encoding.UTF8, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"TXT export başarısız: {ex.Message}", ex);
        }
    }

    public bool IsFormatSupported(TextExportFormat format)
    {
        return format == TextExportFormat.Txt;
    }
}
