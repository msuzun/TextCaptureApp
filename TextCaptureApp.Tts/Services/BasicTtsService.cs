using System.Speech.Synthesis;
using TextCaptureApp.Core.Interfaces;

namespace TextCaptureApp.Tts.Services;

/// <summary>
/// Windows System.Speech.Synthesis tabanlı Text-to-Speech servisi
/// WAV formatında ses dosyası üretir.
/// 
/// NOT: İleride bu implementasyon Azure Cognitive Services Speech SDK veya
/// Google Cloud Text-to-Speech gibi cloud-based TTS servisleri ile değiştirilebilir.
/// Bu durumda MP3 formatına da çevrilebilir (NAudio veya FFmpeg kullanarak).
/// </summary>
public class BasicTtsService : ITtsService
{
    private const int DefaultSpeechRate = 0; // -10 to +10
    private const int DefaultVolume = 100;    // 0 to 100
    private const string DefaultVoiceName = ""; // Empty = system default

    private readonly int _speechRate;
    private readonly int _volume;
    private readonly string? _voiceName;

    /// <summary>
    /// BasicTtsService constructor
    /// </summary>
    /// <param name="speechRate">Konuşma hızı (-10 ile +10 arası, varsayılan: 0)</param>
    /// <param name="volume">Ses seviyesi (0 ile 100 arası, varsayılan: 100)</param>
    /// <param name="voiceName">Kullanılacak ses adı (null/empty = sistem varsayılanı)</param>
    public BasicTtsService(
        int speechRate = DefaultSpeechRate,
        int volume = DefaultVolume,
        string? voiceName = DefaultVoiceName)
    {
        _speechRate = Math.Clamp(speechRate, -10, 10);
        _volume = Math.Clamp(volume, 0, 100);
        _voiceName = string.IsNullOrWhiteSpace(voiceName) ? null : voiceName;
    }

    public Task GenerateAudioAsync(
        string text, 
        string outputPath, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("OutputPath cannot be null or empty", nameof(outputPath));

        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var synthesizer = new SpeechSynthesizer();

                // Speech rate ayarı
                synthesizer.Rate = _speechRate;

                // Volume ayarı
                synthesizer.Volume = _volume;

                // Ses seçimi (eğer belirtilmişse)
                if (_voiceName != null)
                {
                    try
                    {
                        synthesizer.SelectVoice(_voiceName);
                    }
                    catch (Exception ex)
                    {
                        // Ses bulunamazsa default kullan (log edilebilir)
                        System.Diagnostics.Debug.WriteLine(
                            $"Voice '{_voiceName}' not found, using default. Error: {ex.Message}");
                    }
                }

                // WAV dosyasına kaydet
                synthesizer.SetOutputToWaveFile(outputPath);

                // Metni sese dönüştür
                synthesizer.Speak(text);

                // Output'u temizle
                synthesizer.SetOutputToNull();

                // NOT: İleride burada WAV → MP3 dönüşümü yapılabilir:
                // - NAudio kullanarak WAV okuyup MP3'e encode edebiliriz
                // - veya Azure Cognitive Services gibi cloud TTS direkt MP3 üretebilir
                // - veya FFmpeg wrapper kullanarak dönüştürebiliriz
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"TTS başarısız oldu. Metin uzunluğu: {text.Length}, Çıktı: {outputPath}, Hata: {ex.Message}", 
                    ex);
            }
        }, cancellationToken);
    }
}

