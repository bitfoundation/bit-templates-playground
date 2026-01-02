using Bit.TemplatePlayground.Shared.Dtos.Dashboard;
using Bit.TemplatePlayground.Shared.Dtos.Products;
using Bit.TemplatePlayground.Shared.Dtos.Categories;
using Bit.TemplatePlayground.Shared.Dtos.PushNotification;
using Bit.TemplatePlayground.Shared.Dtos.Chatbot;
using Bit.TemplatePlayground.Shared.Dtos.SignalR;
using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Shared.Dtos.Statistics;
using Bit.TemplatePlayground.Shared.Dtos.Diagnostic;

namespace Bit.TemplatePlayground.Shared.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(



  PropertyNameCaseInsensitive = true,
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
  DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
  UseStringEnumConverter = true,
  WriteIndented = false,
  GenerationMode = JsonSourceGenerationMode.Metadata,
  AllowTrailingCommas = true,
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
)]


[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(SendNotificationToRoleDto))]
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
[JsonSerializable(typeof(VerifyWebAuthnAndSignInRequestDto))]
[JsonSerializable(typeof(WebAuthnAssertionOptionsRequestDto))]

[JsonSerializable(typeof(DiagnosticLogDto[]))]
[JsonSerializable(typeof(StartChatRequest))]
[JsonSerializable(typeof(List<SystemPromptDto>))]
[JsonSerializable(typeof(BackgroundJobProgressDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
