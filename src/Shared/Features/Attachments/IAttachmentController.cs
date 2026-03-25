namespace Bit.TemplatePlayground.Shared.Features.Attachments;

[Route("api/v1/[controller]/[action]/"), AuthorizedApi]
public interface IAttachmentController : IAppController
{
    [HttpDelete]
    Task DeleteUserProfilePicture(CancellationToken cancellationToken);

    [HttpDelete("{productId}")]
    Task DeleteProductPrimaryImage(Guid productId, CancellationToken cancellationToken);
}
