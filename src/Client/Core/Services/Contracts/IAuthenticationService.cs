using Bit.AdminPanel.Shared.Dtos.Identity;

namespace Bit.AdminPanel.Client.Core.Services.Contracts;

public interface IAuthenticationService
{
    Task SignIn(SignInRequestDto dto);

    Task SignOut();
}
