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
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(configuration.GetServerAddress(), UriKind.Absolute)
            };
            if (sp.GetRequiredService<ClientWindowsSettings>().WebAppUrl is Uri origin)
            {
                httpClient.DefaultRequestHeaders.Add("X-Origin", origin.ToString());
            }
            return httpClient;
        });

        services.AddSingleton(sp => configuration);
        services.AddSingleton<IStorageService, WindowsStorageService>();
        services.AddSingleton<ILocalHttpServer, WindowsLocalHttpServer>();
        ClientWindowsSettings settings = new();
        configuration.Bind(settings);
        services.AddSingleton(sp =>
        {
            return settings;
        });
        services.AddSingleton(ITelemetryContext.Current!);

        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ConfigureLoggers(configuration);
            loggingBuilder.AddEventSourceLogger();

            loggingBuilder.AddEventLog(options => configuration.GetRequiredSection("Logging:EventLog").Bind(options));
        });

        services.AddOptions<ClientWindowsSettings>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
