namespace Bit.TemplatePlayground.Shared.Controllers;

[Route("api/[controller]/[action]/"), AuthorizedApi]
public interface IAttachmentController : IAppController
{
    [HttpDelete]
    Task DeleteUserProfilePicture(CancellationToken cancellationToken);

    [HttpDelete("{productId}")]
    Task DeleteProductPrimaryImage(Guid productId, CancellationToken cancellationToken);
}
