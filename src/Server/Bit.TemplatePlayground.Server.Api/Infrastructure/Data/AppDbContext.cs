using System.Reflection;
using Bit.TemplatePlayground.Server.Api.Features.Attachments;
using Bit.TemplatePlayground.Server.Api.Features.Categories;
using Bit.TemplatePlayground.Server.Api.Features.Identity.OAuth.Models;
using Bit.TemplatePlayground.Server.Api.Features.Products;
using Bit.TemplatePlayground.Server.Api.Features.PushNotification;
using Bit.TemplatePlayground.Server.Api.Features.Tenants;
using Hangfire.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.Data;

public partial class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options), IDataProtectionKeyContext
{
    public DbSet<UserSession> UserSessions { get; set; } = default!;

    public DbSet<Tenant> Tenants { get; set; } = default!;
    public DbSet<TenantUser> TenantUsers { get; set; } = default!;

    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<PushNotificationSubscription> PushNotificationSubscriptions { get; set; } = default!;

    public DbSet<WebAuthnCredential> WebAuthnCredential { get; set; } = default!;

    public DbSet<SystemPrompt> SystemPrompts { get; set; } = default!;

    public DbSet<Attachment> Attachments { get; set; } = default!;

    /// <summary>The only state the OAuth authorization flow keeps.</summary>
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; } = default!;

    /// <summary>The OAuth half of a <see cref="UserSession"/>, for the few sessions an external application holds.</summary>
    public DbSet<OAuthGrant> OAuthGrants { get; set; } = default!;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.OnHangfireModelCreating("jobs");



        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureIdentityTableNames(modelBuilder);

        ConfigureTenantAwareEntities(modelBuilder);

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

        foreach (var entry in ChangeTracker.Entries<ITenantAware>().Where(e => e.State is EntityState.Added && e.Entity.TenantId == default))
        {
            entry.Entity.TenantId = CurrentTenantId;
        }

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                entry.CurrentValues["UpdatedAt"] = this.GetService<TimeProvider>().GetUtcNow();
        }

        foreach (var entityEntry in ChangeTracker.Entries().Where(e => e.State is EntityState.Modified or EntityState.Deleted))
        {
            var versionProperty = entityEntry.Properties.FirstOrDefault(p => p.Metadata.Name == "Version");

            if (versionProperty is null || entityEntry.CurrentValues["Version"] is not long currentVersion)
                continue;

            // https://github.com/dotnet/efcore/issues/35443
            // The row is matched on the client supplied Version rather than on the value that was read from the
            // database, so a PUT carrying a stale Version is rejected with a ConflictException.
            entityEntry.OriginalValues["Version"] = currentVersion;

            // SQL Server (rowversion) and PostgreSQL (xmin) move the stored value themselves. Where they do not,
            // nothing else in the app ever writes Version, so the WHERE clause above would match forever and every
            // concurrent edit would be accepted. Advance it here so the token actually changes.
            if (entityEntry.State is EntityState.Modified && versionProperty.Metadata.ValueGenerated is ValueGenerated.Never)
                entityEntry.CurrentValues["Version"] = currentVersion + 1;
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

    private TenantProvider tenantProvider => field ??= this.GetService<TenantProvider>();
    private Guid CurrentTenantId => tenantProvider.GetCurrentTenantId();

    /// <summary>
    /// While reads are protected by the following row level security global query filters, INSERTs/Creates get their
    /// TenantId stamped by <see cref="OnSavingChanges"/> when it is still default, resolved through TenantProvider.
    /// A controller only assigns it explicitly when the row belongs to a tenant other than the current one
    /// (See TenantController.Create as an example).
    /// </summary>
    private void ConfigureTenantAwareEntities(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(et => typeof(ITenantAware).IsAssignableFrom(et.ClrType)))
        {
            typeof(AppDbContext)
                .GetMethod(nameof(ConfigureTenantAwareEntity), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private void ConfigureTenantAwareEntity<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantAware
    {
        // Referencing CurrentTenantId (an AppDbContext's instance property) makes EF Core evaluate the filter per context instance.
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

        modelBuilder.Entity<TEntity>()
            .HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
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
