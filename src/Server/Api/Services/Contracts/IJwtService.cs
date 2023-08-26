using Bit.AdminPanel.Server.Api.Models.Identity;
using Bit.AdminPanel.Shared.Dtos.Identity;

namespace Bit.AdminPanel.Server.Api.Services.Contracts;

public interface IJwtService
{
    Task<SignInResponseDto> GenerateToken(User user);
}
