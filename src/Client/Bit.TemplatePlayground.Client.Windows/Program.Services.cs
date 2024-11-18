using System.Net.Http;
using Microsoft.Extensions.Logging;
using Bit.TemplatePlayground.Client.Windows.Services;

namespace Bit.TemplatePlayground.Client.Windows;

public static partial class Program
{
    public static void AddClientWindowsProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Services being registered here can get injected in windows project only.
        services.AddClientCoreProjectServices(configuration);

        services.AddScoped<IExceptionHandler, WindowsExceptionHandler>();
        services.AddScoped<IBitDeviceCoordinator, WindowsDeviceCoordinator>();
        services.AddScoped(sp =>
        {
            var handler = sp.GetRequiredService<HttpMessageHandler>();
            HttpClient httpClient = new(handler)
            {
                BaseAddress = new Uri(configuration.GetServerAddress(), UriKind.Absolute)
            };
            return httpClient;
        });

        services.AddSingleton(sp => configuration);
        services.AddSingleton<IStorageService, WindowsStorageService>();
        services.AddSingleton<ILocalHttpServer, WindowsLocalHttpServer>();
        services.AddSingleton(sp => configuration.Get<ClientWindowsSettings>()!);
        services.AddSingleton(ITelemetryContext.Current!);

        services.AddWpfBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ConfigureLoggers();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            loggingBuilder.AddEventSourceLogger();

            loggingBuilder.AddEventLog();
        });

        services.AddOptions<ClientWindowsSettings>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
