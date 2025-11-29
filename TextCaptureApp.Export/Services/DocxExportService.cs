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
    public Task<bool> ExportAsync(string text, ExportOptions options)
    {
        return Task.Run(() =>
        {
            try
            {
                using var wordDocument = WordprocessingDocument.Create(options.FilePath, WordprocessingDocumentType.Document);
                
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Paragraflar
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    var para = body.AppendChild(new Paragraph());
                    var run = para.AppendChild(new Run());
                    run.AppendChild(new Text(line));
                }

                // Timestamp
                if (options.IncludeTimestamp)
                {
                    var timestampPara = body.AppendChild(new Paragraph());
                    var timestampRun = timestampPara.AppendChild(new Run());
                    timestampRun.AppendChild(new Text($"\nOluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm:ss}"));
                }

                mainPart.Document.Save();
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    public bool IsFormatSupported(ExportFormat format)
    {
        return format == ExportFormat.Docx;
    }

    public bool ValidateFilePath(string filePath, ExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);
    }
}

