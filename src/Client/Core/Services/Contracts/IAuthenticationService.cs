using Bit.TemplatePlayground.Shared.Dtos.Identity;

namespace Bit.TemplatePlayground.Client.Core.Services.Contracts;

public interface IAuthenticationService
{
    Task SignIn(SignInRequestDto dto);

    Task SignOut();
}
