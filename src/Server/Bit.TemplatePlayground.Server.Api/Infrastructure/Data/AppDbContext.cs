using Bit.TemplatePlayground.Server.Api.Features.Products;
using Bit.TemplatePlayground.Server.Api.Features.Categories;
using Bit.TemplatePlayground.Server.Api.Features.Identity.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Bit.TemplatePlayground.Server.Api.Features.PushNotification;
using Hangfire.EntityFrameworkCore;
using Bit.TemplatePlayground.Server.Api.Features.Attachments;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.Data;

public partial class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options), IDataProtectionKeyContext
{
    public DbSet<UserSession> UserSessions { get; set; } = default!;

    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<PushNotificationSubscription> PushNotificationSubscriptions { get; set; } = default!;

    public DbSet<WebAuthnCredential> WebAuthnCredential { get; set; } = default!;

    public DbSet<SystemPrompt> SystemPrompts { get; set; } = default!;

    public DbSet<Attachment> Attachments { get; set; } = default!;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.OnHangfireModelCreating("jobs");



        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureIdentityTableNames(modelBuilder);

        ConfigureConcurrencyToken(modelBuilder);

    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            OnSavingChanges();

#pragma warning disable NonAsyncEFCoreMethodsUsageAnalyzer
            return base.SaveChanges(acceptAllChangesOnSuccess);
#pragma warning restore NonAsyncEFCoreMethodsUsageAnalyzer
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(nameof(AppStrings.UpdateConcurrencyException), exception);
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            OnSavingChanges();

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(nameof(AppStrings.UpdateConcurrencyException), exception);
        }
    }

    private void OnSavingChanges()
    {
        ChangeTracker.DetectChanges();

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                entry.CurrentValues["UpdatedAt"] = DateTimeOffset.UtcNow;
        }

        foreach (var entityEntry in ChangeTracker.Entries().Where(e => e.State is EntityState.Modified or EntityState.Deleted))
        {
            // https://github.com/dotnet/efcore/issues/35443
            if (entityEntry.Properties.Any(p => p.Metadata.Name == "Version") && entityEntry.CurrentValues["Version"] is long currentVersion)
                entityEntry.OriginalValues["Version"] = currentVersion;
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
            // SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses. Convert the values to a supported type:
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();



        configurationBuilder.Properties<decimal>().HavePrecision(18, 3);
        configurationBuilder.Properties<decimal?>().HavePrecision(18, 3);

        base.ConfigureConventions(configurationBuilder);
    }

    private void ConfigureIdentityTableNames(ModelBuilder builder)
    {
        builder.Entity<User>()
            .ToTable("Users");

        builder.Entity<Role>()
            .ToTable("Roles");

        builder.Entity<UserRole>()
            .ToTable("UserRoles");

        builder.Entity<RoleClaim>()
            .ToTable("RoleClaims");

        builder.Entity<UserClaim>()
            .ToTable("UserClaims");

        builder.Entity<UserLogin>()
            .ToTable("UserLogins");

        builder.Entity<UserToken>()
            .ToTable("UserTokens");
    }

    private void ConfigureConcurrencyToken(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {

            foreach (var property in entityType.GetProperties()
                .Where(p => p.Name is "Version" && p.PropertyInfo?.PropertyType == typeof(long)))
            {
                var builder = new PropertyBuilder(property);
                builder.IsConcurrencyToken();
            }
        }
    }


}
