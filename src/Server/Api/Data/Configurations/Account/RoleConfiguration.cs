using Bit.AdminPanel.Server.Api.Models.Account;

namespace Bit.AdminPanel.Server.Api.Data.Configurations.Account;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(role => role.Name).HasMaxLength(50);
    }
}

