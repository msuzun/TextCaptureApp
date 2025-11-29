using iTextSharp.text;
using iTextSharp.text.pdf;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Services;

/// <summary>
/// PDF formatında export servisi
/// </summary>
public class PdfExportService : ITextExportService
{
    public Task<bool> ExportAsync(string text, ExportOptions options)
    {
        return Task.Run(() =>
        {
            try
            {
                using var fs = new FileStream(options.FilePath, FileMode.Create);
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, fs);

                document.Open();

                // Metadata
                if (!string.IsNullOrWhiteSpace(options.Title))
                    document.AddTitle(options.Title);
                if (!string.IsNullOrWhiteSpace(options.Author))
                    document.AddAuthor(options.Author);

                // Font ayarı (Unicode desteği için)
                var baseFont = BaseFont.CreateFont(
                    BaseFont.HELVETICA, 
                    BaseFont.CP1252, 
                    BaseFont.NOT_EMBEDDED);
                var font = new Font(baseFont, 12, Font.NORMAL);

                // İçerik
                var paragraph = new Paragraph(text, font);
                document.Add(paragraph);

                if (options.IncludeTimestamp)
                {
                    var timestamp = new Paragraph($"\n\nOluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", 
                        new Font(baseFont, 8, Font.ITALIC));
                    document.Add(timestamp);
                }

                document.Close();
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
        return format == ExportFormat.Pdf;
    }

    public bool ValidateFilePath(string filePath, ExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }
}

