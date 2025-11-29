using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Ocr;
using TextCaptureApp.ScreenCapture;
using TextCaptureApp.Export;
using TextCaptureApp.Tts;

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
        // Register all services with their interfaces
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<ITextExtractionService>(sp => new TextExtractionService("./tessdata"));
        services.AddSingleton<ITextExportService, TextExportService>();
        services.AddSingleton<ITtsService, TtsService>();

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

