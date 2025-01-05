using Bit.TemplatePlayground.Server.Api.Services;
using Bit.TemplatePlayground.Shared.Dtos.Statistics;
using Bit.TemplatePlayground.Shared.Controllers.Statistics;

namespace Bit.TemplatePlayground.Server.Api.Controllers.Statistics;

[ApiController, Route("api/[controller]/[action]")]
public partial class StatisticsController : AppControllerBase, IStatisticsController
{
    [AutoInject] private NugetStatisticsHttpClient nugetHttpClient = default!;

    [AllowAnonymous]
    [HttpGet("{packageId}")]
    [ResponseCache(Duration = 1 * 24 * 3600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "*" })]
    public async Task<NugetStatsDto> GetNugetStats(string packageId, CancellationToken cancellationToken)
    {
        return await nugetHttpClient.GetPackageStats(packageId, cancellationToken);
    }
}
