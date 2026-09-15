// [mirror] per platform DI registrations of the maui project - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/iOS/Extensions/IIosServiceCollectionExtensions.cs
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/MacCatalyst/Extensions/IMacServiceCollectionExtensions.cs
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/Windows/Extensions/IWindowsServiceCollectionExtensions.cs

using Bit.TemplatePlayground.Client.Maui.Platforms.Android.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class IAndroidServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientMauiProjectAndroidServices(IConfiguration configuration)
        {
            // Services being registered here can get injected in Maui/Android.

            services.AddSingleton<IPushNotificationService, AndroidPushNotificationService>();

            return services;
        }
    }
}
