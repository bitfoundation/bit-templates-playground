using Bit.TemplatePlayground.Shared.Features.Attachments;
using Bit.TemplatePlayground.Shared.Features.Products;
using FluentStorage.Storage;
using Ganss.Xss;

namespace Bit.TemplatePlayground.Server.Api.Features.Products;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS),
    Authorize(Policy = AuthPolicies.TENANT_SELECTED),
    Authorize(Policy = AppFeatures.AdminPanel.ProductCatalog_Manage)]
public partial class ProductController : AppControllerBase, IProductController
{
    [AutoInject] private IStore blobStorage = default!;
    [AutoInject] private HtmlSanitizer htmlSanitizer = default!;

    [AutoInject] private IHubContext<AppHub> appHubContext = default!;
    [AutoInject] private ResponseCacheService responseCacheService = default!;

    [HttpGet, EnableQuery]
    public IQueryable<ProductDto> Get()
    {
        return DbContext.Products
            .Project();
    }

    [HttpGet]
    public async Task<PagedResponse<ProductDto>> GetProducts(ODataQueryOptions<ProductDto> odataQuery, CancellationToken cancellationToken)
    {
        var query = (IQueryable<ProductDto>)odataQuery.ApplyTo(Get(), ignoreQueryOptions: AllowedQueryOptions.Top | AllowedQueryOptions.Skip);

        var totalCount = await query.LongCountAsync(cancellationToken);

        query = query.SkipIf(odataQuery.Skip is not null, odataQuery.Skip?.Value)
                     .TakeIf(odataQuery.Top is not null, odataQuery.Top?.Value);

        return new PagedResponse<ProductDto>(await query.ToArrayAsync(cancellationToken), totalCount);
    }

    [HttpGet("{searchQuery}")]
    public async Task<PagedResponse<ProductDto>> SearchProducts(string searchQuery, ODataQueryOptions<ProductDto> odataQuery, CancellationToken cancellationToken)
    {
        var query = (IQueryable<ProductDto>)odataQuery.ApplyTo(DbContext.Products
            .Where(p => EF.Functions.Like(p.Name!, $"%{searchQuery}%"))
            .Project(), ignoreQueryOptions: AllowedQueryOptions.Top | AllowedQueryOptions.Skip);

        var totalCount = await query.LongCountAsync(cancellationToken);

        query = query.SkipIf(odataQuery.Skip is not null, odataQuery.Skip?.Value)
                     .TakeIf(odataQuery.Top is not null, odataQuery.Top?.Value);

        return new PagedResponse<ProductDto>(await query.ToArrayAsync(cancellationToken), totalCount);
    }

    [HttpGet("{id}")]
    public async Task<ProductDto> Get(Guid id, CancellationToken cancellationToken)
    {
        var dto = await Get().FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.ProductCouldNotBeFound)]);

        return dto;
    }

    [HttpPost]
    public async Task<ProductDto> Create(ProductDto dto, CancellationToken cancellationToken)
    {
        dto.DescriptionHTML = htmlSanitizer.Sanitize(dto.DescriptionHTML ?? string.Empty);

        var entityToAdd = dto.Map();

        entityToAdd.CreatedOn = TimeProvider.GetUtcNow();

        // The image is uploaded before the product row exists.
        entityToAdd.HasPrimaryImage = await DbContext.Attachments
            .AnyAsync(att => att.Id == entityToAdd.Id && att.Kind == AttachmentKind.ProductPrimaryImageMedium, cancellationToken);

        await DbContext.Products.AddAsync(entityToAdd, cancellationToken);

        await Validate(entityToAdd, cancellationToken);


        await DbContext.SaveChangesAsync(cancellationToken);

        await responseCacheService.PurgeProductCache(entityToAdd.ShortId, catalogChanged: true);

        await PublishDashboardDataChanged(cancellationToken);

        return await Get(entityToAdd.Id, cancellationToken);
    }

    [HttpPut]
    public async Task<ProductDto> Update(ProductDto dto, CancellationToken cancellationToken)
    {
        dto.DescriptionHTML = htmlSanitizer.Sanitize(dto.DescriptionHTML ?? string.Empty);

        var entityToUpdate = await DbContext.Products.FindAsync([dto.Id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.ProductCouldNotBeFound)]);

        dto.Patch(entityToUpdate);

        await Validate(entityToUpdate, cancellationToken);


        await DbContext.SaveChangesAsync(cancellationToken);

        await responseCacheService.PurgeProductCache(entityToUpdate.ShortId);

        await PublishDashboardDataChanged(cancellationToken);

        return await Get(entityToUpdate.Id, cancellationToken);
    }

    [HttpDelete("{id}/{version}")]
    public async Task Delete(Guid id, long version, CancellationToken cancellationToken)
    {
        var entityToDelete = await DbContext.Products.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.ProductCouldNotBeFound)]);

        entityToDelete.Version = version;

        var attachments = await DbContext.Attachments
            .Where(att => att.Id == id && (att.Kind == AttachmentKind.ProductPrimaryImageMedium || att.Kind == AttachmentKind.ProductPrimaryImageOriginal))
            .ToArrayAsync(cancellationToken);

        DbContext.Attachments.RemoveRange(attachments);
        DbContext.Remove(entityToDelete);

        await DbContext.SaveChangesAsync(cancellationToken);

        foreach (var attachment in attachments)
        {
            var filePath = attachment.Path;

            if (await blobStorage.ObjectExists(filePath, cancellationToken))
            {
                await blobStorage.DeleteSingleObject(filePath, cancellationToken);
            }
        }

        await responseCacheService.PurgeProductCache(entityToDelete.ShortId, catalogChanged: true);

        await PublishDashboardDataChanged(cancellationToken);
    }

    private async Task PublishDashboardDataChanged(CancellationToken cancellationToken)
    {
        // Check out AppHub's comments for more info.
        // In order to exclude current user session, gets its signalR connection id from database and use GroupExcept instead.
        // Only this tenant: "AuthenticatedClients" spans every tenant.
        await appHubContext.Clients.Group(AppHub.TenantGroupName(TenantProvider.GetCurrentTenantId())).Publish(SharedAppMessages.DASHBOARD_DATA_CHANGED, null, cancellationToken);
    }

    private async Task Validate(Product product, CancellationToken cancellationToken)
    {
        var entry = DbContext.Entry(product);
        // Remote validation example: Any errors thrown here will be displayed in the client's edit form component.
        // The `p.Id != product.Id` term matters on a case or accent insensitive collation: IsModified compares
        // ordinally, so renaming "EQB SUV" to "EQB Suv" reaches this query, and without it the row matches itself.
        if ((entry.State is EntityState.Added || entry.Property(c => c.Name).IsModified)
            && await DbContext.Products.AnyAsync(p => p.Id != product.Id && p.Name == product.Name, cancellationToken))
            throw new ResourceValidationException((nameof(ProductDto.Name), [Localizer[nameof(AppStrings.DuplicateProductName)]]));
    }
}

