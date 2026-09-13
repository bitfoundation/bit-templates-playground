using Bit.TemplatePlayground.Shared.Features.Tenants.Dtos;

namespace Bit.TemplatePlayground.Server.Api.Features.Tenants;

/// <summary>
/// More info at src/Server/Bit.TemplatePlayground.Server.Api/Features/Mappers.md
/// </summary>
[Mapper]
public static partial class TenantsMapper
{
    public static partial IQueryable<TenantDto> Project(this IQueryable<Tenant> query);

    public static partial TenantDto Map(this Tenant source);
    public static partial Tenant Map(this TenantDto source);
    public static partial void Patch(this TenantDto source, Tenant destination);
}
