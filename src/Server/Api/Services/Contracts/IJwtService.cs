using Bit.TemplatePlayground.Server.Api.Models.Identity;
using Bit.TemplatePlayground.Shared.Dtos.Identity;

namespace Bit.TemplatePlayground.Server.Api.Services.Contracts;

public interface IJwtService
{
    Task<SignInResponseDto> GenerateToken(User user);
}
