using Bit.TemplatePlayground.Server.Api.Models.Identity;

namespace Bit.TemplatePlayground.Server.Api.Services.Identity;

public partial class AppUserConfirmation : IUserConfirmation<User>
{
    public async Task<bool> IsConfirmedAsync(UserManager<User> manager, User user)
    {
        return user.EmailConfirmed || user.PhoneNumberConfirmed;
    }
}
