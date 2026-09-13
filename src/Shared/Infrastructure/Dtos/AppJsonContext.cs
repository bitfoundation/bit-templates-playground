using Bit.TemplatePlayground.Shared.Features.Categories;
using Bit.TemplatePlayground.Shared.Features.Chatbot;
using Bit.TemplatePlayground.Shared.Features.Dashboard;
using Bit.TemplatePlayground.Shared.Features.Diagnostic;
using Bit.TemplatePlayground.Shared.Features.Identity.OAuth.Dtos;
using Bit.TemplatePlayground.Shared.Features.Products;
using Bit.TemplatePlayground.Shared.Features.PushNotification;
using Bit.TemplatePlayground.Shared.Features.Statistics;
using Bit.TemplatePlayground.Shared.Features.Tenants.Dtos;
using Bit.TemplatePlayground.Shared.Infrastructure.Dtos.SignalR;

namespace Bit.TemplatePlayground.Shared.Infrastructure.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(


  AllowTrailingCommas = true,
  PropertyNameCaseInsensitive = true,
  GenerationMode = JsonSourceGenerationMode.Default,
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
[JsonSerializable(typeof(OAuthAuthorizeRequestDto))]
[JsonSerializable(typeof(OAuthConsentDto))]
[JsonSerializable(typeof(OAuthApprovalDto))]
[JsonSerializable(typeof(OAuthClientDto))]
[JsonSerializable(typeof(List<OAuthClientDto>))]
[JsonSerializable(typeof(RevokeOAuthClientRequestDto))]
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

[JsonSerializable(typeof(TenantDto))]
[JsonSerializable(typeof(List<TenantDto>))]
[JsonSerializable(typeof(PagedResponse<TenantDto>))]
[JsonSerializable(typeof(InviteUserToTenantRequestDto))]
[JsonSerializable(typeof(DiagnosticLogDto[]))]
[JsonSerializable(typeof(StartChatRequest))]
[JsonSerializable(typeof(AiChatMessage))]
[JsonSerializable(typeof(AssistantReply))]
[JsonSerializable(typeof(AssistantTurn))]
[JsonSerializable(typeof(List<SystemPromptDto>))]
[JsonSerializable(typeof(BackgroundJobProgressDto))]
[JsonSerializable(typeof(SynthesizeSpeechRequestDto))]
[JsonSerializable(typeof(TranscribeSpeechResponseDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
