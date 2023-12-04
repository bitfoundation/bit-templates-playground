using Bit.TemplatePlayground.Server.Models.Products;

namespace Bit.TemplatePlayground.Server.Models.Categories;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string? Name { get; set; }

    public string? Color { get; set; }

    public IList<Product>? Products { get; set; }
}
