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
    private readonly ExportServiceResolver _exportServiceResolver;
    private readonly ITtsService _ttsService;

    private ImageCaptureResult? _currentImage;
    private string _extractedText = string.Empty;

    public MainWindow(
        IScreenCaptureService screenCaptureService,
        IOcrService ocrService,
        ExportServiceResolver exportServiceResolver,
        ITtsService ttsService)
    {
        _screenCaptureService = screenCaptureService;
        _ocrService = ocrService;
        _exportServiceResolver = exportServiceResolver;
        _ttsService = ttsService;

        InitializeComponent();
        CheckServicesReady();
    }

    private async void CheckServicesReady()
    {
        var ocrReady = await _ocrService.IsReadyAsync();
        var ttsReady = await _ttsService.IsReadyAsync();

        if (!ocrReady)
        {
            MessageBox.Show("OCR servisi hazır değil. 'tessdata' klasörünü ve dil dosyalarını kontrol edin.",
                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        if (!ttsReady)
        {
            MessageBox.Show("TTS servisi hazır değil. Sisteminizde TTS voice'ları kurulu olmayabilir.",
                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnCaptureScreen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Minimize window for clean capture
            WindowState = WindowState.Minimized;
            await Task.Delay(300);

            _currentImage = await _screenCaptureService.CaptureFullScreenAsync();
            DisplayCapturedImage();

            WindowState = WindowState.Normal;
            BtnExtractText.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ekran görüntüsü alınamadı: {ex.Message}", "Hata", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnCaptureRegion_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            WindowState = WindowState.Minimized;
            await Task.Delay(300);

            _currentImage = await _screenCaptureService.CaptureSelectedRegionAsync();
            DisplayCapturedImage();

            WindowState = WindowState.Normal;
            BtnExtractText.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bölge yakalanamadı: {ex.Message}", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnExtractText_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;

        try
        {
            BtnExtractText.IsEnabled = false;
            BtnExtractText.Content = "⏳ Processing...";

            var languageCode = GetSelectedLanguageCode();
            var result = await _ocrService.ExtractTextAsync(_currentImage, languageCode);

            _extractedText = result.Text;
            TxtExtracted.Text = _extractedText;
            TxtConfidence.Text = $"Confidence: {result.Confidence:P1}";

            EnableExportButtons();
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
        }
    }

    private async void BtnExportTxt_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(ExportFormat.Txt, "Text Files (*.txt)|*.txt");
    }

    private async void BtnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(ExportFormat.Pdf, "PDF Files (*.pdf)|*.pdf");
    }

    private async void BtnExportDocx_Click(object sender, RoutedEventArgs e)
    {
        await ExportText(ExportFormat.Docx, "Word Documents (*.docx)|*.docx");
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
                var options = new TtsOptions
                {
                    OutputPath = dialog.FileName,
                    Speed = 0,
                    Volume = 100,
                    Format = AudioFormat.Wav
                };

                var success = await _ttsService.ConvertTextToSpeechAsync(_extractedText, options);

                if (success)
                {
                    MessageBox.Show("Ses dosyası başarıyla oluşturuldu!", "Başarılı",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ses dosyası oluşturulamadı.", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"TTS hatası: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task ExportText(ExportFormat format, string filter)
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
                    FilePath = dialog.FileName,
                    IncludeTimestamp = true,
                    Title = "OCR Extracted Text",
                    Author = "Text Capture App"
                };

                // Strategy pattern: format'a göre doğru export servisini seç
                var exportService = _exportServiceResolver(format);
                var success = await exportService.ExportAsync(_extractedText, options);

                if (success)
                {
                    MessageBox.Show("Dosya başarıyla kaydedildi!", "Başarılı",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Dosya kaydedilemedi.", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export hatası: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DisplayCapturedImage()
    {
        if (_currentImage == null) return;

        var bitmap = new BitmapImage();
        using (var ms = new MemoryStream(_currentImage.ImageData))
        {
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
        }

        ImgCaptured.Source = bitmap;
    }

    private void EnableExportButtons()
    {
        BtnExportTxt.IsEnabled = true;
        BtnExportPdf.IsEnabled = true;
        BtnExportDocx.IsEnabled = true;
        BtnGenerateTts.IsEnabled = true;
    }

    private string GetSelectedLanguageCode()
    {
        return CmbLanguage.SelectedIndex switch
        {
            0 => "eng",
            1 => "tur",
            2 => "deu",
            3 => "fra",
            4 => "spa",
            _ => "eng"
        };
    }
}