// [mirror] per platform DI registrations of the maui project - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/Android/Extensions/IAndroidServiceCollectionExtensions.cs
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/iOS/Extensions/IIosServiceCollectionExtensions.cs
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/Windows/Extensions/IWindowsServiceCollectionExtensions.cs

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
