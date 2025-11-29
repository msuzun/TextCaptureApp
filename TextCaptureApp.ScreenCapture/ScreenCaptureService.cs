using System.Drawing;
using System.Drawing.Imaging;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.ScreenCapture;

/// <summary>
/// Windows ekran görüntüsü yakalama servisi
/// </summary>
public class ScreenCaptureService : IScreenCaptureService
{
    public Task<CapturedImage> CaptureFullScreenAsync()
    {
        return Task.Run(() =>
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            return CaptureRegion(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        });
    }

    public Task<CapturedImage> CaptureRegionAsync(int x, int y, int width, int height)
    {
        return Task.Run(() => CaptureRegion(x, y, width, height));
    }

    public Task<CapturedImage> CaptureSelectedRegionAsync()
    {
        // Bu metod UI'da bir selection window açarak implement edilecek
        // Şimdilik placeholder olarak tüm ekranı yakalayalım
        return CaptureFullScreenAsync();
    }

    private CapturedImage CaptureRegion(int x, int y, int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        
        graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);

        return new CapturedImage
        {
            ImageData = ms.ToArray(),
            Width = width,
            Height = height,
            CapturedAt = DateTime.Now,
            Format = "PNG"
        };
    }
}

