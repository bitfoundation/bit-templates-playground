using Bit.TemplatePlayground.Server.Api.Models.Identity;

namespace Bit.TemplatePlayground.Server.Api.Data.Configurations.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(role => role.Name).HasMaxLength(50);
    }
}

