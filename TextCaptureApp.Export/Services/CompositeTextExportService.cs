using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;
using TextCaptureApp.Export.Internal;

namespace TextCaptureApp.Export.Services;

/// <summary>
/// Composite pattern implementation of ITextExportService
/// Delegates to internal strategy-based exporters
/// </summary>
public class CompositeTextExportService : ITextExportService
{
    private readonly Dictionary<TextExportFormat, ITextExporter> _exporters;

    public CompositeTextExportService()
    {
        // Initialize exporters dictionary
        _exporters = new Dictionary<TextExportFormat, ITextExporter>
        {
            { TextExportFormat.Txt, new TxtTextExporter() },
            { TextExportFormat.Pdf, new PdfTextExporter() },
            { TextExportFormat.Docx, new DocxTextExporter() }
        };
    }

    public async Task ExportAsync(
        string text, 
        ExportOptions options, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty", nameof(text));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.OutputPath))
            throw new ArgumentException("OutputPath cannot be empty", nameof(options));

        // Format'a göre uygun exporter'ı seç ve çalıştır
        if (!_exporters.TryGetValue(options.Format, out var exporter))
        {
            throw new NotSupportedException(
                $"Export format '{options.Format}' desteklenmiyor. " +
                $"Desteklenen formatlar: {string.Join(", ", _exporters.Keys)}");
        }

        await exporter.ExportAsync(text, options, cancellationToken);
    }

    public bool IsFormatSupported(TextExportFormat format)
    {
        return _exporters.ContainsKey(format);
    }
}

