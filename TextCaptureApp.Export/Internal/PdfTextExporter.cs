using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Internal;

/// <summary>
/// PDF exporter using QuestPDF
/// </summary>
internal class PdfTextExporter : ITextExporter
{
    public Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // QuestPDF license (Community/Commercial)
                QuestPDF.Settings.License = LicenseType.Community;

                // PDF document oluştur
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Sayfa ayarları
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        // İçerik
                        page.Content().Column(column =>
                        {
                            column.Spacing(5);

                            // Plain text paragraflar
                            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            foreach (var line in lines)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                column.Item().Text(line);
                            }
                        });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf(options.OutputPath);
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
}

