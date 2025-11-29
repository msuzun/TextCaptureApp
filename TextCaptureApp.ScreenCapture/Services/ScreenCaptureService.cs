using System.Drawing;
using System.Drawing.Imaging;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.ScreenCapture.Services;

/// <summary>
/// Windows ekran görüntüsü yakalama ve dosya yükleme servisi
/// </summary>
public class ScreenCaptureService : IScreenCaptureService
{
    public async Task<ImageCaptureResult?> CaptureRegionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.Run(() =>
            {
                // Tüm ekranı yakala (kullanıcı seçimi için UI gerekli, şimdilik full screen)
                var bounds = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
                return CaptureRegion(bounds.X, bounds.Y, bounds.Width, bounds.Height, "ScreenRegion");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task<ImageCaptureResult?> CaptureFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(filePath))
                    return null;

                using var bitmap = new Bitmap(filePath);
                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                return new ImageCaptureResult
                {
                    ImageStream = ms,
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    SourceDescription = $"File: {filePath}"
                };
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    private ImageCaptureResult CaptureRegion(int x, int y, int width, int height, string source)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        
        graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height));

        var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        ms.Position = 0;

        return new ImageCaptureResult
        {
            ImageStream = ms,
            Width = width,
            Height = height,
            SourceDescription = source
        };
    }
}
