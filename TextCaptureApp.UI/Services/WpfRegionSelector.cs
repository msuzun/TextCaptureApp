using System.Windows;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.UI.Services;

/// <summary>
/// WPF overlay window kullanarak region selection
/// </summary>
public class WpfRegionSelector : IRegionSelector
{
    public Task<RegionSelectionResult> SelectRegionAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<RegionSelectionResult>();

        // UI thread'de çalıştır
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var window = new RegionSelectorWindow();
                
                // Cancellation token ile window'u kapat
                cancellationToken.Register(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (window.IsLoaded)
                            window.Close();
                    });
                });

                var result = window.ShowDialog();

                if (result == true && window.Result != null)
                {
                    tcs.SetResult(window.Result);
                }
                else
                {
                    tcs.SetResult(new RegionSelectionResult { IsCancelled = true });
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}

