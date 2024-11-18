
namespace Microsoft.Extensions.DependencyInjection;

public static partial class IIosServiceCollectionExtensions
{
    public static IServiceCollection AddClientMauiProjectIosServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Services registered in this class can be injected in iOS.


        return services;
    }
}
