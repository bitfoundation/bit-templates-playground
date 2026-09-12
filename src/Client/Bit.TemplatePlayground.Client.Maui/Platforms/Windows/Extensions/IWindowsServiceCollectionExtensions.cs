using Bit.TemplatePlayground.Client.Maui.Platforms.Windows.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class IWindowsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientMauiProjectWindowsServices(IConfiguration configuration)
        {
            // Services being registered here can get injected in Maui/windows.

            services.AddSingleton<IPushNotificationService, WindowsPushNotificationService>();

            return services;
        }
    }
}
