using Bit.TemplatePlayground.Shared.Features.Dashboard;
using Bit.TemplatePlayground.Shared.Features.Products;
using Bit.TemplatePlayground.Shared.Features.Categories;
using Bit.TemplatePlayground.Shared.Features.Chatbot;
using Bit.TemplatePlayground.Shared.Infrastructure.Dtos.SignalR;
using Bit.TemplatePlayground.Shared.Features.Statistics;
using Bit.TemplatePlayground.Shared.Features.Diagnostic;

namespace Bit.TemplatePlayground.Shared.Infrastructure.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(


  AllowTrailingCommas = true,
  PropertyNameCaseInsensitive = true,
  GenerationMode = JsonSourceGenerationMode.Default,
  DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase

)]


[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(PushNotificationSubscriptionDto))]
[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(List<CategoryDto>))]
[JsonSerializable(typeof(PagedResponse<CategoryDto>))]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSerializable(typeof(PagedResponse<ProductDto>))]
[JsonSerializable(typeof(List<ProductsCountPerCategoryResponseDto>))]
[JsonSerializable(typeof(OverallAnalyticsStatsDataResponseDto))]
[JsonSerializable(typeof(List<ProductPercentagePerCategoryResponseDto>))]

[JsonSerializable(typeof(DiagnosticLogDto[]))]
[JsonSerializable(typeof(StartChatRequest))]
[JsonSerializable(typeof(List<SystemPromptDto>))]
[JsonSerializable(typeof(BackgroundJobProgressDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
