using Bit.TemplatePlayground.Server.Api.Models.Products;

namespace Bit.TemplatePlayground.Server.Api.Services;

/// <summary>
/// This class stores vectorized products and provides methods to query/manage them.
/// </summary>
public partial class ProductEmbeddingService
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IWebHostEnvironment env = default!;
    [AutoInject] private IServiceProvider serviceProvider = default!;

    public async Task<IQueryable<Product>> GetProductsBySearchQuery(string searchQuery, CancellationToken cancellationToken)
    {
        // The RAG has been implemented for PostgreSQL only. Checkout https://github.com/bitfoundation/bitplatform/blob/develop/src/Templates/Bit.TemplatePlayground/Bit.Bit.TemplatePlayground/src/Server/Bit.TemplatePlayground.Server.Api/Services/ProductEmbeddingService.cs
        return dbContext.Products.OrderBy(_ => EF.Functions.Random()).Take(15);
    }

    public async Task Embed(Product product, CancellationToken cancellationToken)
    {
        return;
    }

    private async Task<ReadOnlyMemory<float>?> EmbedText(string input, CancellationToken cancellationToken)
    {
        return null;
    }
}
