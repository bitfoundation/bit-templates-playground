using Hangfire;
using Bit.TemplatePlayground.Server.Web;
using Bit.TemplatePlayground.Tests.Services;
using Bit.TemplatePlayground.Server.Api.Services;
using Bit.TemplatePlayground.Client.Core.Services.HttpMessageHandlers;

namespace Microsoft.AspNetCore.Builder;

public static partial class WebApplicationBuilderExtensions
{
    public static void AddTestProjectServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHangfire(configuration => configuration.UseColouredConsoleLogProvider());

        builder.AddServerWebProjectServices();

        // Register test-specific services for all tests here

        services.AddTransient<HttpClient>(sp =>
        {
            var handlerFactory = sp.GetRequiredService<HttpMessageHandlersChainFactory>();
            return new HttpClient(handlerFactory.Invoke())
            {
                BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>().GetServerAddress(), UriKind.Absolute)
            };
        });
    }
}
