using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Services;

/// <summary>
/// DOCX formatında export servisi
/// </summary>
public class DocxExportService : ITextExportService
{
    public Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var wordDocument = WordprocessingDocument.Create(options.OutputPath, WordprocessingDocumentType.Document);
                
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Paragraflar
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var para = body.AppendChild(new Paragraph());
                    var run = para.AppendChild(new Run());
                    run.AppendChild(new Text(line));
                }

                mainPart.Document.Save();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"DOCX export başarısız: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    public bool IsFormatSupported(TextExportFormat format)
    {
        return format == TextExportFormat.Docx;
    }
}
