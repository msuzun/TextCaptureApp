using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.UI.ViewModels;

/// <summary>
/// MainWindow ViewModel - MVVM pattern implementation
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IOcrService _ocrService;
    private readonly ITextExportService _textExportService;
    private readonly ITtsService _ttsService;

    private ImageSource? _currentImage;
    private ImageCaptureResult? _currentImageResult;
    private string? _extractedText;
    private bool _isBusy;
    private string? _statusMessage;
    private CancellationTokenSource? _cts;

    public MainWindowViewModel(
        IScreenCaptureService screenCaptureService,
        IOcrService ocrService,
        ITextExportService textExportService,
        ITtsService ttsService)
    {
        _screenCaptureService = screenCaptureService;
        _ocrService = ocrService;
        _textExportService = textExportService;
        _ttsService = ttsService;

        // Initialize commands
        OpenImageCommand = new RelayCommand(async () => await OpenImageAsync());
        CaptureScreenRegionCommand = new RelayCommand(async () => await CaptureScreenRegionAsync());
        RunOcrCommand = new RelayCommand(async () => await RunOcrAsync(), () => CurrentImage != null && !IsBusy);
        ExportTxtCommand = new RelayCommand(async () => await ExportTextAsync(TextExportFormat.Txt), () => !string.IsNullOrWhiteSpace(ExtractedText) && !IsBusy);
        ExportPdfCommand = new RelayCommand(async () => await ExportTextAsync(TextExportFormat.Pdf), () => !string.IsNullOrWhiteSpace(ExtractedText) && !IsBusy);
        ExportDocxCommand = new RelayCommand(async () => await ExportTextAsync(TextExportFormat.Docx), () => !string.IsNullOrWhiteSpace(ExtractedText) && !IsBusy);
        GenerateAudioCommand = new RelayCommand(async () => await GenerateAudioAsync(), () => !string.IsNullOrWhiteSpace(ExtractedText) && !IsBusy);
        CopyTextCommand = new RelayCommand(() => CopyTextToClipboard(), () => !string.IsNullOrWhiteSpace(ExtractedText));

        StatusMessage = "Ready";
    }

    #region Properties

    public ImageSource? CurrentImage
    {
        get => _currentImage;
        set
        {
            _currentImage = value;
            OnPropertyChanged();
            RunOcrCommand.RaiseCanExecuteChanged();
        }
    }

    public string? ExtractedText
    {
        get => _extractedText;
        set
        {
            _extractedText = value;
            OnPropertyChanged();
            ExportTxtCommand.RaiseCanExecuteChanged();
            ExportPdfCommand.RaiseCanExecuteChanged();
            ExportDocxCommand.RaiseCanExecuteChanged();
            GenerateAudioCommand.RaiseCanExecuteChanged();
            CopyTextCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            RunOcrCommand.RaiseCanExecuteChanged();
            ExportTxtCommand.RaiseCanExecuteChanged();
            ExportPdfCommand.RaiseCanExecuteChanged();
            ExportDocxCommand.RaiseCanExecuteChanged();
            GenerateAudioCommand.RaiseCanExecuteChanged();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Commands

    public RelayCommand OpenImageCommand { get; }
    public RelayCommand CaptureScreenRegionCommand { get; }
    public RelayCommand RunOcrCommand { get; }
    public RelayCommand ExportTxtCommand { get; }
    public RelayCommand ExportPdfCommand { get; }
    public RelayCommand ExportDocxCommand { get; }
    public RelayCommand GenerateAudioCommand { get; }
    public RelayCommand CopyTextCommand { get; }

    #endregion

    #region Command Implementations

    private async Task OpenImageAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Select Image File"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading image...";

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                var imageResult = await _screenCaptureService.CaptureFromFileAsync(dialog.FileName, _cts.Token);

                if (imageResult != null)
                {
                    await LoadImageAsync(imageResult);
                    StatusMessage = "Image loaded successfully";
                }
                else
                {
                    StatusMessage = "Failed to load image";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to load image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private async Task CaptureScreenRegionAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Preparing screen capture...";

            // Minimize window for clean capture
            Application.Current.MainWindow!.WindowState = WindowState.Minimized;
            await Task.Delay(300);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var imageResult = await _screenCaptureService.CaptureRegionAsync(_cts.Token);

            Application.Current.MainWindow.WindowState = WindowState.Normal;

            if (imageResult != null)
            {
                await LoadImageAsync(imageResult);
                StatusMessage = "Screen captured successfully";
            }
            else
            {
                StatusMessage = "Screen capture cancelled";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Failed to capture screen: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task RunOcrAsync()
    {
        if (CurrentImage == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Running OCR...";

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            // Use stored ImageCaptureResult
            if (_currentImageResult != null)
            {
                _currentImageResult.ImageStream.Position = 0;

                var ocrResult = await _ocrService.ExtractTextAsync(_currentImageResult, _cts.Token);

                ExtractedText = ocrResult.Text;
                StatusMessage = ocrResult.Confidence.HasValue
                    ? $"OCR completed (Confidence: {ocrResult.Confidence.Value:P1})"
                    : "OCR completed";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "OCR cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"OCR failed: {ex.Message}";
            MessageBox.Show($"OCR failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task ExportTextAsync(TextExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(ExtractedText)) return;

        var filter = format switch
        {
            TextExportFormat.Txt => "Text Files (*.txt)|*.txt",
            TextExportFormat.Pdf => "PDF Files (*.pdf)|*.pdf",
            TextExportFormat.Docx => "Word Documents (*.docx)|*.docx",
            _ => "All Files (*.*)|*.*"
        };

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
                IsBusy = true;
                StatusMessage = $"Exporting to {format}...";

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                var options = new ExportOptions
                {
                    Format = format,
                    OutputPath = dialog.FileName
                };

                await _textExportService.ExportAsync(ExtractedText, options, _cts.Token);

                StatusMessage = $"Exported to {format} successfully";
                MessageBox.Show("File saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Export cancelled";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export failed: {ex.Message}";
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private async Task GenerateAudioAsync()
    {
        if (string.IsNullOrWhiteSpace(ExtractedText)) return;

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
                IsBusy = true;
                StatusMessage = "Generating audio...";

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                await _ttsService.GenerateAudioAsync(ExtractedText, dialog.FileName, _cts.Token);

                StatusMessage = "Audio generated successfully";
                MessageBox.Show("Audio file created successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Audio generation cancelled";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Audio generation failed: {ex.Message}";
                MessageBox.Show($"Audio generation failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private void CopyTextToClipboard()
    {
        if (!string.IsNullOrWhiteSpace(ExtractedText))
        {
            Clipboard.SetText(ExtractedText);
            StatusMessage = "Text copied to clipboard";
        }
    }

    #endregion

    #region Helper Methods

    private async Task LoadImageAsync(ImageCaptureResult imageResult)
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                imageResult.ImageStream.Position = 0;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = imageResult.ImageStream;
                bitmap.EndInit();
                bitmap.Freeze();

                // Store both image and result
                _currentImageResult = imageResult;
                CurrentImage = bitmap;
            });
        });
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _currentImageResult?.Dispose();
    }

    #endregion
}

