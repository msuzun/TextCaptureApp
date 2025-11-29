using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextCaptureApp.Core.Interfaces;
using TextCaptureApp.Core.Models;
using TextCaptureApp.Ocr.Services;
using TextCaptureApp.ScreenCapture.Services;
using TextCaptureApp.Export.Services;
using TextCaptureApp.Tts.Services;

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
        // Register core services
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IOcrService>(sp => new TesseractOcrService("./tessdata"));
        services.AddSingleton<ITtsService, TtsService>();

        // Register export services (Strategy pattern)
        services.AddSingleton<ITextExportService, TxtExportService>();
        services.AddSingleton<ITextExportService, PdfExportService>();
        services.AddSingleton<ITextExportService, DocxExportService>();

        // Register export service factory/resolver
        services.AddSingleton<ExportServiceResolver>(sp =>
        {
            var exportServices = sp.GetServices<ITextExportService>();
            return format => exportServices.FirstOrDefault(s => s.IsFormatSupported(format))
                ?? throw new NotSupportedException($"Export format {format} is not supported");
        });

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

/// <summary>
/// Export servislerini format'a göre çözümleyen delegate
/// </summary>
public delegate ITextExportService ExportServiceResolver(ExportFormat format);

