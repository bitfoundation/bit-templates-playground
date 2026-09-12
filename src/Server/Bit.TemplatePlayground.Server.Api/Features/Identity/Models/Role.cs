using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Identity.Models;

public partial class Role : IdentityRole<Guid>
{
    public List<UserRole> Users { get; set; } = [];
    public List<RoleClaim> Claims { get; set; } = [];

    /// <summary>
    /// Null means the role is a global role (like g-admin and demo), otherwise the role belongs to a tenant (like each tenant's t-admin role).
    /// </summary>
    public Guid? TenantId { get; set; }

    public Tenant? Tenant { get; set; }
}

