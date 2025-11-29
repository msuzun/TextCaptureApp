using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Ocr.Services;
using TextCaptureApp.ScreenCapture.Services;
using TextCaptureApp.Export.Services;
using TextCaptureApp.Tts.Services;
using TextCaptureApp.UI.Services;
using TextCaptureApp.UI.ViewModels;

namespace TextCaptureApp.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // appsettings.json dosyasını yükle
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Configuration sections
        var ocrConfig = configuration.GetSection("Ocr");
        var ttsConfig = configuration.GetSection("Tts");

        // Register UI services
        services.AddSingleton<IRegionSelector, WpfRegionSelector>();

        // Register core services with configuration
        services.AddSingleton<IScreenCaptureService>(sp =>
        {
            var regionSelector = sp.GetService<IRegionSelector>();
            return new ScreenCaptureService(regionSelector);
        });

        // OCR Service with configuration - Ekran görüntüleri için özel servis
        services.AddSingleton<IOcrService>(sp =>
        {
            var tessDataPath = ocrConfig["TessDataPath"] ?? "./tessdata";
            var defaultLanguage = ocrConfig["DefaultLanguage"] ?? "eng+tur";
            return new ScreenOcrService(tessDataPath, defaultLanguage);
        });

        // TTS Service with configuration
        services.AddSingleton<ITtsService>(sp =>
        {
            var speechRate = int.TryParse(ttsConfig["SpeechRate"], out var rate) ? rate : 0;
            var volume = int.TryParse(ttsConfig["Volume"], out var vol) ? vol : 100;
            var voiceName = ttsConfig["VoiceName"] ?? "";
            return new BasicTtsService(speechRate, volume, voiceName);
        });

        // Register composite export service
        services.AddSingleton<ITextExportService, CompositeTextExportService>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Resolve ViewModel and MainWindow from DI
        var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
