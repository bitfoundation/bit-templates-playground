using Microsoft.AspNetCore.OutputCaching;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.Services;

/// <summary>
/// 
/// The default Bit.TemplatePlayground project template includes:
/// 1. `Static` file caching on browsers and CDN edge servers.
/// 2. Caching JSON and dynamic files responses on CDN edge servers and ASP.NET Core's Output Cache by using `AppResponseCache` attribute in controllers like `StatisticsController`, `AttachmentController` and minimal apis.
/// 3. Caching pre-rendered HTML results of Blazor pages on CDN edge servers and ASP.NET Core's Output by using `AppResponseCache` attribute in pages like HomePage.razor
/// 
/// - Note: For successful cache purging, the request URL must exactly match the URL passed to <see cref="PurgeCache(string[])"/>.  
/// </summary>
public partial class ResponseCacheService
{
    [AutoInject] private HttpClient httpClient = default!;
    [AutoInject] private IOutputCacheStore outputCacheStore = default!;
    [AutoInject] private ServerApiSettings serverApiSettings = default!;
    [AutoInject] private IHttpContextAccessor httpContextAccessor = default!;

    public async Task PurgeCache(params string[] relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            await outputCacheStore.EvictByTagAsync(relativePath, default);
        }
        await PurgeCloudflareCache(relativePaths);
    }

    public async Task PurgeProductCache(int shortId)
    {
        await PurgeCache("/", $"/product/{shortId}", $"/api/v1/ProductView/Get/{shortId}");
    }

    private async Task PurgeCloudflareCache(string[] relativePaths)
    {
        if (serverApiSettings?.Cloudflare?.Configured is not true)
            return;

        var zoneId = serverApiSettings.Cloudflare.ZoneId;
        var apiToken = serverApiSettings.Cloudflare.ApiToken;

        var files = serverApiSettings.Cloudflare.AdditionalDomains
            .Union([httpContextAccessor.HttpContext!.Request.GetBaseUrl(), httpContextAccessor.HttpContext!.Request.GetWebAppUrl()])
            .SelectMany(baseUri => relativePaths.Select(path => new Uri(baseUri, path)))
            .Distinct()
            .ToArray();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{zoneId}/purge_cache");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        request.Content = JsonContent.Create(new { files });
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
