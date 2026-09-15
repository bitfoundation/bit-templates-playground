using Fido2NetLib;
using Bit.TemplatePlayground.Shared.Features.Statistics;
using Bit.TemplatePlayground.Server.Api.Features.Identity.OAuth.Services;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.Services;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(
  AllowTrailingCommas = true,
  PropertyNameCaseInsensitive = true,
  GenerationMode = JsonSourceGenerationMode.Default,
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(GoogleRecaptchaVerificationResponse))]
[JsonSerializable(typeof(CloudflarePurgeResponse))]
[JsonSerializable(typeof(AuthenticatorResponse))]
[JsonSerializable(typeof(ClientIdMetadataDocument))]
public partial class ServerJsonContext : JsonSerializerContext
{
}
