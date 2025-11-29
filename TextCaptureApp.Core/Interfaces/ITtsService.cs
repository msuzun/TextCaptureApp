namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Text-to-Speech servisini tanımlar
/// </summary>
public interface ITtsService
{
    /// <summary>
    /// Metni sese dönüştürür ve belirtilen dosyaya kaydeder
    /// </summary>
    /// <param name="text">Sese dönüştürülecek metin</param>
    /// <param name="outputPath">Ses dosyası kayıt yolu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <exception cref="InvalidOperationException">TTS başarısız olursa</exception>
    Task GenerateAudioAsync(string text, string outputPath, CancellationToken cancellationToken = default);
}

