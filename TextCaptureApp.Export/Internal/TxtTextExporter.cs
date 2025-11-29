using System.Text;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Internal;

/// <summary>
/// Plain text (.txt) exporter
/// </summary>
internal class TxtTextExporter : ITextExporter
{
    public async Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            // UTF-8 encoding ile kaydet
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
}

