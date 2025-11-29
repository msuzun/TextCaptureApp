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
    private readonly IRegionSelector? _regionSelector;

    /// <summary>
    /// ScreenCaptureService constructor
    /// </summary>
    /// <param name="regionSelector">Optional: UI katmanından region seçimi için (örn: WPF overlay window)</param>
    public ScreenCaptureService(IRegionSelector? regionSelector = null)
    {
        _regionSelector = regionSelector;
    }

    public async Task<ImageCaptureResult?> CaptureRegionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RegionSelectionResult? region = null;

            // Eğer region selector mevcutsa, kullanıcıya seçim yaptır
            if (_regionSelector != null)
            {
                region = await _regionSelector.SelectRegionAsync(cancellationToken);
                
                if (region.IsCancelled)
                    return null;
            }

            return await Task.Run(() =>
            {
                if (region != null && region.Width > 0 && region.Height > 0)
                {
                    // Kullanıcının seçtiği bölgeyi yakala
                    return CaptureRegion(region.X, region.Y, region.Width, region.Height, 
                        $"ScreenRegion ({region.Width}x{region.Height})");
                }
                else
                {
                    // Fallback: Region selector yoksa veya geçersiz region ise tüm ekranı yakala
                    var bounds = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
                    return CaptureRegion(bounds.X, bounds.Y, bounds.Width, bounds.Height, 
                        "FullScreen");
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            // Log edilebilir
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

    /// <summary>
    /// Belirtilen ekran bölgesini yakalar
    /// </summary>
    private ImageCaptureResult CaptureRegion(int x, int y, int width, int height, string sourceDescription)
    {
        // Bitmap ve Graphics oluştur
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Ekrandan kopyala
        graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

        // MemoryStream'e kaydet (PNG formatında)
        var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        ms.Position = 0;

        return new ImageCaptureResult
        {
            ImageStream = ms,
            Width = width,
            Height = height,
            SourceDescription = sourceDescription
        };
    }
}
