using Bit.TemplatePlayground.Server.Api.Features.Products;
using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Categories;

public partial class Category
    : ITenantAware
{
    public Guid Id { get; set; }

    [Required, MaxLength(64)]
    public string? Name { get; set; }

    [MaxLength(16)]
    public string? Color { get; set; }

    public long Version { get; set; }

    public Guid TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }

    public IList<Product> Products { get; set; } = [];
}
