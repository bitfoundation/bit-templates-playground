using Bit.TemplatePlayground.Client.Maui.Platforms.MacCatalyst.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class IMacServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientMauiProjectMacCatalystServices(IConfiguration configuration)
        {
            // Services being registered here can get injected in Maui/macOS.

            services.AddSingleton<IPushNotificationService, MacCatalystPushNotificationService>();

            return services;
        }
    }
}
