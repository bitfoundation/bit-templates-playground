using Bit.TemplatePlayground.Server.Api.Models.Identity;

namespace Bit.TemplatePlayground.Server.Api.Data.Configurations.Identity;

public partial class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.HasIndex(userClaim => new { userClaim.RoleId, userClaim.ClaimType });
    }
}
