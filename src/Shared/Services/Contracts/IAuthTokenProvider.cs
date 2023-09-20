namespace Bit.TemplatePlayground.Shared.Services.Contracts;

public interface IAuthTokenProvider
{
    Task<string?> GetAccessTokenAsync();
}
