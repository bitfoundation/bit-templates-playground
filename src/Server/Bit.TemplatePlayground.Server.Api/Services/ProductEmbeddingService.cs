using Bit.TemplatePlayground.Server.Api.Models.Products;

namespace Bit.TemplatePlayground.Server.Api.Services;

/// <summary>
/// Approaches to implement text search:
/// 1- Simple string matching (e.g., `Contains` method).
/// 2- Full-text search using database capabilities (e.g., PostgreSQL's full-text search).
/// 3- Vector-based search using embeddings (e.g., using OpenAI's embeddings).
/// This service implements vector-based search using embeddings that has the following advantages:
///     - More accurate search results based on semantic meaning rather than just similarity matching.
///     - Multi-language support, as embeddings can capture the meaning of words across different languages.
/// And has the following disadvantages:
///     - Requires additional processing to generate embeddings for the text.
///     - Require more storage space for embeddings compared to simple text search.
/// The simple full-text search would be enough for product search case, but we have implemented the vector-based search to demonstrate how to use embeddings in the project.
/// </summary>
public partial class ProductEmbeddingService
{
    private const float SIMILARITY_THRESHOLD = 0.85f;

    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IWebHostEnvironment env = default!;
    [AutoInject] private IServiceProvider serviceProvider = default!;

    public async Task<IQueryable<Product>> GetProductsBySearchQuery(string searchQuery, CancellationToken cancellationToken)
    {
        // The RAG has been implemented for PostgreSQL / SQL Server only. Check out https://github.com/bitfoundation/bitplatform/blob/develop/src/Templates/Bit.TemplatePlayground/Bit.Bit.TemplatePlayground/src/Server/Bit.TemplatePlayground.Server.Api/Services/ProductEmbeddingService.cs
        return dbContext.Products.Where(p => p.Name!.Contains(searchQuery) || p.Category!.Name!.Contains(searchQuery));
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
