namespace Bit.TemplatePlayground.Server.Api.Features.Identity.Models;

public class UserRole : IdentityUserRole<Guid>
{
    public User? User { get; set; }

    public Role? Role { get; set; }
}
