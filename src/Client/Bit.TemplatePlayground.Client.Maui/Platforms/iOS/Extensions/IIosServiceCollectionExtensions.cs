using Bit.TemplatePlayground.Client.Maui.Platforms.iOS.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class IIosServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientMauiProjectIosServices(IConfiguration configuration)
        {
            // Services registered in this class can be injected in iOS.

            services.AddSingleton<IPushNotificationService, iOSPushNotificationService>();

            return services;
        }
    }
}
