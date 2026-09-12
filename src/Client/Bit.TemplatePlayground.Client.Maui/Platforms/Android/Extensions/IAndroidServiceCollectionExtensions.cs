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
