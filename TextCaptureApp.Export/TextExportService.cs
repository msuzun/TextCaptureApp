using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

// Alias to resolve namespace conflicts
using PdfDocument = iTextSharp.text.Document;
using PdfParagraph = iTextSharp.text.Paragraph;
using PdfFont = iTextSharp.text.Font;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace TextCaptureApp.Export;

/// <summary>
/// Çoklu formatta (PDF, DOCX, TXT) metin export servisi
/// </summary>
public class TextExportService : ITextExportService
{
    public async Task<bool> ExportAsync(string text, ExportOptions options)
    {
        try
        {
            return options.Format switch
            {
                ExportFormat.Txt => await ExportToTxtAsync(text, options),
                ExportFormat.Pdf => await ExportToPdfAsync(text, options),
                ExportFormat.Docx => await ExportToDocxAsync(text, options),
                _ => throw new NotSupportedException($"Format {options.Format} desteklenmiyor.")
            };
        }
        catch
        {
            return false;
        }
    }

    public bool IsFormatSupported(ExportFormat format)
    {
        return format is ExportFormat.Txt or ExportFormat.Pdf or ExportFormat.Docx;
    }

    public bool ValidateFilePath(string filePath, ExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var extension = format switch
        {
            ExportFormat.Txt => ".txt",
            ExportFormat.Pdf => ".pdf",
            ExportFormat.Docx => ".docx",
            _ => string.Empty
        };

        return filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> ExportToTxtAsync(string text, ExportOptions options)
    {
        await File.WriteAllTextAsync(options.FilePath, text, Encoding.UTF8);
        return true;
    }

    private Task<bool> ExportToPdfAsync(string text, ExportOptions options)
    {
        return Task.Run(() =>
        {
            using var fs = new FileStream(options.FilePath, FileMode.Create);
            var document = new PdfDocument(iTextSharp.text.PageSize.A4, 50, 50, 50, 50);
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, fs);

            document.Open();

            // Metadata
            if (!string.IsNullOrWhiteSpace(options.Title))
                document.AddTitle(options.Title);
            if (!string.IsNullOrWhiteSpace(options.Author))
                document.AddAuthor(options.Author);

            // Font ayarı (Unicode desteği için)
            var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(
                iTextSharp.text.pdf.BaseFont.HELVETICA, 
                iTextSharp.text.pdf.BaseFont.CP1252, 
                iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
            var font = new PdfFont(baseFont, 12, PdfFont.NORMAL);

            // İçerik
            var paragraph = new PdfParagraph(text, font);
            document.Add(paragraph);

            if (options.IncludeTimestamp)
            {
                var timestamp = new PdfParagraph($"\n\nOluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", 
                    new PdfFont(baseFont, 8, PdfFont.ITALIC));
                document.Add(timestamp);
            }

            document.Close();
            return true;
        });
    }

    private Task<bool> ExportToDocxAsync(string text, ExportOptions options)
    {
        return Task.Run(() =>
        {
            using var wordDocument = WordprocessingDocument.Create(options.FilePath, WordprocessingDocumentType.Document);
            
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new WordDocument();
            var body = mainPart.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());

            // Paragraflar
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var para = body.AppendChild(new WordParagraph());
                var run = para.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(line));
            }

            // Timestamp
            if (options.IncludeTimestamp)
            {
                var timestampPara = body.AppendChild(new WordParagraph());
                var timestampRun = timestampPara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                timestampRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text($"\nOluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm:ss}"));
            }

            mainPart.Document.Save();
            return true;
        });
    }
}

