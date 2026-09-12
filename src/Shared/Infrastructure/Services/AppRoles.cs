namespace Bit.TemplatePlayground.Shared.Infrastructure.Services;

public class AppRoles
{
    /// <summary>
    /// Has all features <see cref="AppFeatures"/> automatically assigned (See IAuthTokenProvider.ReadClaims and AppJwtSecureDataFormat.Unprotect).
    /// </summary>
    public const string GlobalAdmin = "g-admin";

    /// <summary>
    /// Each tenant has its own role named t-admin (Scoped by Role's TenantId).
    /// Has the features returned by <see cref="AppFeatures.GetTenantAdminFeatures"/> automatically assigned (See IAuthTokenProvider.ReadClaims and AppJwtSecureDataFormat.Unprotect).
    /// </summary>
    public const string TenantAdmin = "t-admin";


    public const string Demo = "demo";

    public static bool IsBuiltInRole(string name)
    {
        return name is GlobalAdmin
            or TenantAdmin
            ;
    }
}
