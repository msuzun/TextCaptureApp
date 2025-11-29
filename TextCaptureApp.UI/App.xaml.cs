using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;
using TextCaptureApp.Ocr.Services;
using TextCaptureApp.ScreenCapture.Services;
using TextCaptureApp.Export.Services;
using TextCaptureApp.Tts.Services;
using TextCaptureApp.UI.Services;

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
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register UI services
        services.AddSingleton<IRegionSelector, WpfRegionSelector>();

        // Register core services
        services.AddSingleton<IScreenCaptureService>(sp =>
        {
            var regionSelector = sp.GetService<IRegionSelector>();
            return new ScreenCaptureService(regionSelector);
        });
        services.AddSingleton<IOcrService>(sp => 
            new TesseractOcrService(tessDataPath: "./tessdata", defaultLanguage: "tur+eng"));
        services.AddSingleton<ITtsService, BasicTtsService>();

        // Register composite export service
        services.AddSingleton<ITextExportService, CompositeTextExportService>();

        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

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

