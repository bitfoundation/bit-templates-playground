using Bit.TemplatePlayground.Shared.Dtos.Dashboard;

namespace Bit.TemplatePlayground.Shared.Controllers.Dashboard;

[Route("api/[controller]/[action]/"), AuthorizedApi]
public interface IDashboardController : IAppController
{
    [HttpGet]
    Task<OverallAnalyticsStatsDataResponseDto> GetOverallAnalyticsStatsData(CancellationToken cancellationToken);

    [HttpGet]
    Task<List<ProductsCountPerCategoryResponseDto>> GetProductsCountPerCategoryStats(CancellationToken cancellationToken) => default!;

    [HttpGet]
    Task<List<ProductPercentagePerCategoryResponseDto>> GetProductsPercentagePerCategoryStats(CancellationToken cancellationToken);
}
