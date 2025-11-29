using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IOcrService _ocrService;
    private readonly ITextExportService _textExportService;
    private readonly ITtsService _ttsService;

    private ImageCaptureResult? _currentImage;
    private string _extractedText = string.Empty;
    private CancellationTokenSource? _cts;

    public MainWindow(
        IScreenCaptureService screenCaptureService,
        IOcrService ocrService,
        ITextExportService textExportService,
        ITtsService ttsService)
    {
        _screenCaptureService = screenCaptureService;
        _ocrService = ocrService;
        _textExportService = textExportService;
        _ttsService = ttsService;

        InitializeComponent();
    }

    private async void BtnCaptureScreen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            WindowState = WindowState.Minimized;
            await Task.Delay(300);

            _cts = new CancellationTokenSource();
            _currentImage?.Dispose();
            _currentImage = await _screenCaptureService.CaptureRegionAsync(_cts.Token);

            if (_currentImage != null)
            {
                DisplayCapturedImage();
                BtnExtractText.IsEnabled = true;
            }

            WindowState = WindowState.Normal;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ekran görüntüsü alınamadı: {ex.Message}", "Hata", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async void BtnCaptureRegion_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Görüntü Dosyası Seç"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _cts = new CancellationTokenSource();
                _currentImage?.Dispose();
                _currentImage = await _screenCaptureService.CaptureFromFileAsync(dialog.FileName, _cts.Token);

                if (_currentImage != null)
                {
                    DisplayCapturedImage();
                    BtnExtractText.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya yüklenemedi: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private async void BtnExtractText_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;

        try
        {
            BtnExtractText.IsEnabled = false;
            BtnExtractText.Content = "⏳ Processing...";

            _cts = new CancellationTokenSource();
            
            // Stream'i başa sar (multiple reads için)
            _currentImage.ImageStream.Position = 0;
            
            var result = await _ocrService.ExtractTextAsync(_currentImage, _cts.Token);

            _extractedText = result.Text;
            TxtExtracted.Text = _extractedText;
            TxtConfidence.Text = result.Confidence.HasValue 
                ? $"Confidence: {result.Confidence.Value:P1}" 
                : string.Empty;

            EnableExportButtons();
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("OCR işlemi iptal edildi.", "İptal",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Metin çıkarılamadı: {ex.Message}", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnExtractText.IsEnabled = true;
            BtnExtractText.Content = "🔍 Extract Text (OCR)";
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async void BtnExportTxt_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(TextExportFormat.Txt, "Text Files (*.txt)|*.txt");
    }

    private async void BtnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(TextExportFormat.Pdf, "PDF Files (*.pdf)|*.pdf");
    }

    private async void BtnExportDocx_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(TextExportFormat.Docx, "Word Documents (*.docx)|*.docx");
    }

    private async void BtnGenerateTts_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_extractedText)) return;

        var dialog = new SaveFileDialog
        {
            Filter = "WAV Audio Files (*.wav)|*.wav",
            DefaultExt = ".wav",
            FileName = $"speech_{DateTime.Now:yyyyMMdd_HHmmss}.wav"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _cts = new CancellationTokenSource();
                await _ttsService.GenerateAudioAsync(_extractedText, dialog.FileName, _cts.Token);

                MessageBox.Show("Ses dosyası başarıyla oluşturuldu!", "Başarılı",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("TTS işlemi iptal edildi.", "İptal",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"TTS hatası: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private async Task ExportText(TextExportFormat format, string filter)
    {
        if (string.IsNullOrWhiteSpace(_extractedText)) return;

        var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = format.ToString().ToLower(),
            FileName = $"exported_text_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var options = new ExportOptions
                {
                    Format = format,
                    OutputPath = dialog.FileName
                };

                _cts = new CancellationTokenSource();
                await _textExportService.ExportAsync(_extractedText, options, _cts.Token);

                MessageBox.Show("Dosya başarıyla kaydedildi!", "Başarılı",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Export işlemi iptal edildi.", "İptal",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export hatası: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private void DisplayCapturedImage()
    {
        if (_currentImage == null) return;

        _currentImage.ImageStream.Position = 0;
        
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = _currentImage.ImageStream;
        bitmap.EndInit();

        ImgCaptured.Source = bitmap;
    }

    private void EnableExportButtons()
    {
        BtnExportTxt.IsEnabled = true;
        BtnExportPdf.IsEnabled = true;
        BtnExportDocx.IsEnabled = true;
        BtnGenerateTts.IsEnabled = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _currentImage?.Dispose();
        base.OnClosed(e);
    }
}
