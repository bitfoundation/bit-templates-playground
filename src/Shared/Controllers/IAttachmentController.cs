using Bit.TemplatePlayground.Shared.Dtos.Products;

namespace Bit.TemplatePlayground.Shared.Controllers;

[Route("api/[controller]/[action]/"), AuthorizedApi]
public interface IAttachmentController : IAppController
{
    [HttpDelete]
    Task RemoveProfileImage(CancellationToken cancellationToken);

    [HttpDelete("{id}")]
    Task<ProductDto> RemoveProductImage(Guid id, CancellationToken cancellationToken);
}
