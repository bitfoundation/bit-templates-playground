
namespace Microsoft.Extensions.DependencyInjection;

public static partial class IAndroidServiceCollectionExtensions
{
    public static IServiceCollection AddClientMauiProjectAndroidServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Services being registered here can get injected in Maui/Android.


        return services;
    }
}
