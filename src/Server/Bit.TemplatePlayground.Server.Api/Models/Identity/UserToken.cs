namespace Bit.TemplatePlayground.Server.Api.Models.Identity;

public class UserToken : IdentityUserToken<Guid>
{
    public User? User { get; set; }
}
