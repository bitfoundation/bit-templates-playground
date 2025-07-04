using Bit.TemplatePlayground.Shared.Dtos.Dashboard;
using Bit.TemplatePlayground.Shared.Dtos.Products;
using Bit.TemplatePlayground.Shared.Dtos.Categories;
using Bit.TemplatePlayground.Shared.Dtos.Chatbot;
using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Shared.Dtos.Statistics;
using Bit.TemplatePlayground.Shared.Dtos.Diagnostic;

namespace Bit.TemplatePlayground.Shared.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(SendNotificationToRoleDto))]
[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(List<CategoryDto>))]
[JsonSerializable(typeof(PagedResult<CategoryDto>))]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSerializable(typeof(PagedResult<ProductDto>))]
[JsonSerializable(typeof(List<ProductsCountPerCategoryResponseDto>))]
[JsonSerializable(typeof(OverallAnalyticsStatsDataResponseDto))]
[JsonSerializable(typeof(List<ProductPercentagePerCategoryResponseDto>))]
[JsonSerializable(typeof(VerifyWebAuthnAndSignInRequestDto))]
[JsonSerializable(typeof(WebAuthnAssertionOptionsRequestDto))]

[JsonSerializable(typeof(DiagnosticLogDto[]))]
[JsonSerializable(typeof(StartChatbotRequest))]
[JsonSerializable(typeof(SystemPromptDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
