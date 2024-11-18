using Bit.TemplatePlayground.Shared.Dtos.Categories;
using Bit.TemplatePlayground.Shared.Dtos.Dashboard;
using Bit.TemplatePlayground.Shared.Dtos.Products;
using Bit.TemplatePlayground.Shared.Dtos.Statistics;

namespace Bit.TemplatePlayground.Shared.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(RestErrorInfo))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(List<ProductsCountPerCategoryResponseDto>))]
[JsonSerializable(typeof(OverallAnalyticsStatsDataResponseDto))]
[JsonSerializable(typeof(List<ProductPercentagePerCategoryResponseDto>))]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(PagedResult<ProductDto>))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(PagedResult<CategoryDto>))]
[JsonSerializable(typeof(List<CategoryDto>))]
public partial class AppJsonContext : JsonSerializerContext
{
}
