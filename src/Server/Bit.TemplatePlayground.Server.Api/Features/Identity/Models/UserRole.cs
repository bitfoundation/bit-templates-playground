using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Identity.Models;

public class UserRole : IdentityUserRole<Guid>
{
    public User? User { get; set; }

    public Role? Role { get; set; }

    /// <summary>
    /// Follows the <see cref="Role.TenantId"/> of the assigned role. Null means a global role assignment.
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }

    public Guid? TenantId { get; set; }
}
