namespace Bit.AdminPanel.Shared.Services.Contracts;

public interface IAuthTokenProvider
{
    Task<string?> GetAcccessTokenAsync();
}
