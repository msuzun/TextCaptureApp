using TextCaptureApp.Core.Models;

namespace TextCaptureApp.Core.Interfaces;

/// <summary>
/// Text-to-Speech servisini tanımlar
/// </summary>
public interface ITtsService
{
    /// <summary>
    /// Metni sese dönüştürür ve dosyaya kaydeder
    /// </summary>
    Task<bool> ConvertTextToSpeechAsync(string text, TtsOptions options);

    /// <summary>
    /// Kullanılabilir sesleri döndürür
    /// </summary>
    Task<IEnumerable<string>> GetAvailableVoicesAsync();

    /// <summary>
    /// TTS servisinin hazır olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsReadyAsync();
}

