// [mirror] blazor hybrid DI registrations, logging and OpenTelemetry setup - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Maui/MauiProgram.Services.cs

using System.Diagnostics.Metrics;
using Bit.TemplatePlayground.Client.Core.Infrastructure.Services.HttpMessageHandlers;
using Bit.TemplatePlayground.Client.Windows.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;

namespace Bit.TemplatePlayground.Client.Windows;

public static partial class Program
{
    extension(IServiceCollection services)
    {
        public void AddClientWindowsProjectServices(IConfiguration configuration)
        {
            // Services being registered here can get injected in windows project only.
            services.AddClientCoreProjectServices(configuration);

            services.AddScoped<IWebAuthnService, WindowsWebAuthnService>();
            services.AddScoped<IExternalNavigationService, WindowsExternalNavigationService>();
            services.AddScoped<ClientExceptionHandlerBase, WindowsExceptionHandler>();
            services.AddScoped<SharedExceptionHandler>(sp => sp.GetRequiredService<ClientExceptionHandlerBase>());

            services.AddScoped<IAppUpdateService, WindowsAppUpdateService>();
            services.AddScoped<IPermissionService, WindowsPermissionService>();
            services.AddScoped<IBitDeviceCoordinator, WindowsDeviceCoordinator>();

            services.AddScoped<HttpClient>(sp =>
            {
                var handlerFactory = sp.GetRequiredService<HttpMessageHandlersChainFactory>();
                var httpClient = new HttpClient(handlerFactory.Invoke())
                {
                    BaseAddress = new Uri(configuration.GetServerAddress(), UriKind.Absolute)
                };
                var origin = sp.GetRequiredService<ClientWindowsSettings>().WebAppUrl ?? httpClient.BaseAddress;
                httpClient.DefaultRequestHeaders.Add("X-Origin", origin.ToString());
                return httpClient;
            });

            services.AddSingleton(sp => configuration);
            services.AddSingleton<IStorageService, WindowsStorageService>();
            services.AddSingleton<ILocalHttpServer, WindowsLocalHttpServer>();

            ClientWindowsSettings settings = new();
            configuration.Bind(settings);
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<ClientWindowsSettings>>().Value);
            services.AddSingleton(ITelemetryContext.Current!);
            services.AddSingleton<IPushNotificationService, WindowsPushNotificationService>();

            services.AddWindowsFormsBlazorWebView();
            services.AddBlazorWebViewDeveloperTools();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ConfigureLoggers(configuration);
                loggingBuilder.AddEventSourceLogger();

                loggingBuilder.AddOpenTelemetry(options =>
                {
                    options.IncludeScopes = true;
                    options.IncludeFormattedMessage = true;
                    configuration.Bind("Logging:OpenTelemetry", options);
                });


                loggingBuilder.AddEventLog(options => configuration.Bind("Logging:EventLog", options));
            });

            var openTelemetry = services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddMeter(Meter.Current.Name);
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(ActivitySource.Current.Name);
                })
                .ConfigureResource(resource =>
                {
                    resource.AddAttributes([new("service.name", Application.ProductName!)]);
                });

            var useOtlpExporter = string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]) is false
                || string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"]) is false;

            if (useOtlpExporter)
            {
                openTelemetry.UseOtlpExporter();
            }


            services.AddOptions<ClientWindowsSettings>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}
