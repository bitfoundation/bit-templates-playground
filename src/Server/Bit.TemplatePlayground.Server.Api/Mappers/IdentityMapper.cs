using Riok.Mapperly.Abstractions;
using Bit.TemplatePlayground.Server.Api.Models.Identity;
using Bit.TemplatePlayground.Shared.Dtos.Identity;

namespace Bit.TemplatePlayground.Server.Api.Mappers;

/// <summary>
/// More info at Server/Mappers/README.md
/// </summary>
[Mapper]
public static partial class IdentityMapper
{
    public static partial UserDto Map(this User source);
    public static partial void Patch(this EditUserDto source, User destination);
    public static partial UserSessionDto Map(this UserSession source);
}
