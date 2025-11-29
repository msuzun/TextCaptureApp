using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Internal;

/// <summary>
/// Internal strategy interface for text export implementations
/// </summary>
internal interface ITextExporter
{
    /// <summary>
    /// Metni belirtilen formatta export eder
    /// </summary>
    Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default);
}

