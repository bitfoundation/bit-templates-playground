using Bit.TemplatePlayground.Server.Api.Features.Categories;
using Bit.TemplatePlayground.Server.Api.Features.Tenants;

namespace Bit.TemplatePlayground.Server.Api.Features.Products;

public partial class Product
    : ITenantAware
{
    public Guid Id { get; set; }

    /// <summary>
    /// The product's ShortId is used to create a more human-friendly URL.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ShortId { get; set; }
        = Environment.TickCount; // Using a database sequence for this is recommended.

    [Required, MaxLength(64)]
    public string? Name { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(4096)]
    public string? DescriptionHTML { get; set; }

    [MaxLength(4096)]
    public string? DescriptionText { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public Guid CategoryId { get; set; }

    public long Version { get; set; }

    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }

    public Guid TenantId { get; set; }

    public bool HasPrimaryImage { get; set; } = false;

    public string? PrimaryImageAltText { get; set; }

}
