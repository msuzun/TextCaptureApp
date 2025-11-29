namespace TextCaptureApp.Core.Models;

/// <summary>
/// Text-to-Speech dönüşüm seçeneklerini tanımlar
/// </summary>
public class TtsOptions
{
    public string OutputPath { get; set; } = string.Empty;
    public string Voice { get; set; } = "default";
    public int Speed { get; set; } = 0; // -10 to +10
    public int Volume { get; set; } = 100; // 0 to 100
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
}

public enum AudioFormat
{
    Mp3,
    Wav
}

