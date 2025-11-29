using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Metin export servisini tanımlar (PDF, DOCX, TXT)
/// </summary>
public interface ITextExportService
{
    /// <summary>
    /// Metni belirtilen formatta export eder
    /// </summary>
    /// <param name="text">Export edilecek metin</param>
    /// <param name="options">Export seçenekleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <exception cref="InvalidOperationException">Export başarısız olursa</exception>
    Task ExportAsync(string text, ExportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export formatının bu servis tarafından desteklenip desteklenmediğini kontrol eder
    /// </summary>
    bool IsFormatSupported(TextExportFormat format);
}

