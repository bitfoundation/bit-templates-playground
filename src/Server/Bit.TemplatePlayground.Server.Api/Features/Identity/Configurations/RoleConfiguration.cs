using Bit.TemplatePlayground.Server.Api.Features.Identity.Models;
using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Identity.Configurations;

public partial class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(role => role.Name).HasMaxLength(50);

        builder.HasMany(role => role.Users)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);

        builder.HasMany(role => role.Claims)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);

        // The base IdentityDbContext adds a global unique index (RoleNameIndex) on NormalizedName that conflicts
        // with having a t-admin role per tenant, so its uniqueness gets replaced by the following filtered unique indexes:
        // 1. The role name must be unique within the tenant (When TenantId is not null).
        // 2. The role name must be unique among the global roles (When TenantId is null).
        builder.HasIndex(role => role.NormalizedName).IsUnique(false);
        builder.HasUniqueIndexOnNullable(role => new { role.Name, role.TenantId }, role => role.TenantId);

        builder.HasIndex(role => role.Name)
            .HasFilter($"[{nameof(Role.TenantId)}] IS NULL")
            .IsUnique();

        // The default store tenant's admin role.
        builder.HasData(new Role
        {
            Id = Guid.Parse("7ff71671-a1d6-5f97-abb9-d87d7b47d6e9"),
            Name = AppRoles.TenantAdmin,
            NormalizedName = AppRoles.TenantAdmin.ToUpperInvariant(),
            TenantId = TenantConfiguration.FallbackTenantId,
            ConcurrencyStamp = "7ff71671-a1d6-5f97-abb9-d87d7b47d6e9"
        });
        builder.HasData(new Role
        {
            Id = Guid.Parse("8ff71671-a1d6-5f97-abb9-d87d7b47d6e7"),
            Name = AppRoles.GlobalAdmin,
            NormalizedName = AppRoles.GlobalAdmin.ToUpperInvariant(),
            ConcurrencyStamp = "8ff71671-a1d6-5f97-abb9-d87d7b47d6e7"
        });

        builder.HasData(new Role
        {
            Id = Guid.Parse("9ff71672-a1d5-4f97-abb7-d87d6b47d5e8"),
            Name = AppRoles.Demo,
            NormalizedName = AppRoles.Demo.ToUpperInvariant(),
            ConcurrencyStamp = "9ff71672-a1d5-4f97-abb7-d87d6b47d5e8",
            TenantId = TenantConfiguration.FallbackTenantId
        });
    }
}
