namespace TextCaptureApp.Core.Models;

/// <summary>
/// Yakalanan ekran görüntüsünü temsil eder
/// </summary>
public class ImageCaptureResult
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CapturedAt { get; set; }
    public string Format { get; set; } = "PNG";
}

