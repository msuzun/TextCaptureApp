namespace TextCaptureApp.Core.Models;

/// <summary>
/// Yakalanan görüntüyü temsil eder
/// </summary>
public class ImageCaptureResult : IDisposable
{
    /// <summary>
    /// Görüntü verisi (stream-based, memory efficient)
    /// </summary>
    public required Stream ImageStream { get; set; }

    /// <summary>
    /// Görüntü genişliği
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Görüntü yüksekliği
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Kaynak açıklaması (örn: 'ScreenRegion', 'File: c:\image.png')
    /// </summary>
    public string? SourceDescription { get; set; }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        
        ImageStream?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

