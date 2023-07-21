using Bit.AdminPanel.Server.Api.Models.Account;
using Bit.AdminPanel.Shared.Dtos.Account;

namespace Bit.AdminPanel.Server.Api.Services.Contracts;

public interface IJwtService
{
    Task<SignInResponseDto> GenerateToken(User user);
}
