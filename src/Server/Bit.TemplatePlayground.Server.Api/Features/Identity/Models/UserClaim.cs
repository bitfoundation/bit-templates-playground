using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Identity.Models;

public class UserClaim : IdentityUserClaim<Guid>
{
    public User? User { get; set; }

    /// <summary>
    /// Null means the claim is assigned to the user globally, otherwise the claim only applies while the user is signed into that tenant.
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }

    public Guid? TenantId { get; set; }
}
