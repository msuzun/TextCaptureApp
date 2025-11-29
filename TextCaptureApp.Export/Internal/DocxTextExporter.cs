using Xceed.Words.NET;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Export.Internal;

/// <summary>
/// DOCX exporter using DocX library
/// </summary>
internal class DocxTextExporter : ITextExporter
{
    public Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // DocX document oluştur
                using var document = DocX.Create(options.OutputPath);

                // Metni paragraf olarak ekle
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                
                foreach (var line in lines)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Her satır bir paragraf
                    var paragraph = document.InsertParagraph(line);
                    paragraph.FontSize(12);
                    paragraph.Font("Calibri");
                }

                // Kaydet
                document.Save();
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
}

