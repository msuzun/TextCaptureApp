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
    public Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var fs = new FileStream(options.OutputPath, FileMode.Create);
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, fs);

                document.Open();

                // Font ayarı
                var baseFont = BaseFont.CreateFont(
                    BaseFont.HELVETICA, 
                    BaseFont.CP1252, 
                    BaseFont.NOT_EMBEDDED);
                var font = new Font(baseFont, 12, Font.NORMAL);

                // İçerik
                var paragraph = new Paragraph(text, font);
                document.Add(paragraph);

                document.Close();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"PDF export başarısız: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    public bool IsFormatSupported(TextExportFormat format)
    {
        return format == TextExportFormat.Pdf;
    }
}
