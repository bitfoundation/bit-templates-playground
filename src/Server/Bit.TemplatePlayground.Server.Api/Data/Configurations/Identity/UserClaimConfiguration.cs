using Bit.TemplatePlayground.Server.Api.Models.Identity;

namespace Bit.TemplatePlayground.Server.Api.Data.Configurations.Identity;

public partial class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.HasIndex(userClaim => new { userClaim.UserId, userClaim.ClaimType, userClaim.ClaimValue });
    }
}
