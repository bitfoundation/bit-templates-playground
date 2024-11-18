using Bit.TemplatePlayground.Shared.Dtos.Statistics;

namespace Bit.TemplatePlayground.Shared.Controllers.Statistics;

[Route("api/[controller]/[action]/")]
public interface IStatisticsController : IAppController
{
    [HttpGet("{packageId}")]
    Task<NugetStatsDto> GetNugetStats(string packageId, CancellationToken cancellationToken);

    [HttpGet, Route("https://api.github.com/repos/bitfoundation/bitplatform")]
    Task<GitHubStats> GetGitHubStats(CancellationToken cancellationToken) => default!;
}
