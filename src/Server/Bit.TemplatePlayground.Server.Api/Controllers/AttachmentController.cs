using ImageMagick;
using FluentStorage.Blobs;
using Microsoft.AspNetCore.SignalR;
using Bit.TemplatePlayground.Server.Api.SignalR;
using Bit.TemplatePlayground.Shared.Controllers;
using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Server.Api.Models.Identity;

namespace Bit.TemplatePlayground.Server.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public partial class AttachmentController : AppControllerBase, IAttachmentController
{
    [AutoInject] private IBlobStorage blobStorage = default!;
    [AutoInject] private UserManager<User> userManager = default!;
    [AutoInject] private IHubContext<AppHub> appHubContext = default!;

    [HttpPost]
    [RequestSizeLimit(11 * 1024 * 1024 /*11MB*/)]
    public async Task UploadProfileImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            throw new BadRequestException();

        var userId = User.GetUserId();

        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new ResourceNotFoundException();

        var destFileName = $"{userId}_{file.FileName}";

        if (user.ProfileImageName is not null)
        {
            var oldFilePath = $"{AppSettings.UserProfileImagesDir}{user.ProfileImageName}";

            if (await blobStorage.ExistsAsync(oldFilePath, cancellationToken))
            {
                await blobStorage.DeleteAsync(oldFilePath, cancellationToken);
            }
        }

        destFileName = destFileName.Replace(Path.GetExtension(destFileName), "_256.webp");
        var resizedFilePath = $"{AppSettings.UserProfileImagesDir}{destFileName}";

        try
        {
            using MagickImage sourceImage = new(file.OpenReadStream());

            MagickGeometry resizedImageSize = new(256, 256);

            sourceImage.Resize(resizedImageSize);

            await blobStorage.WriteAsync(resizedFilePath, sourceImage.ToByteArray(MagickFormat.WebP), cancellationToken: cancellationToken);

            user.ProfileImageName = destFileName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ResourceValidationException(result.Errors.Select(err => new LocalizedString(err.Code, err.Description)).ToArray());
        }
        catch
        {
            await blobStorage.DeleteAsync(resizedFilePath, cancellationToken);

            throw;
        }

        await PublishUserProfileUpdated(user.Map(), cancellationToken);
    }

    [HttpDelete]
    public async Task RemoveProfileImage(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user?.ProfileImageName is null)
            throw new ResourceNotFoundException();

        var filePath = $"{AppSettings.UserProfileImagesDir}{user.ProfileImageName}";

        if (await blobStorage.ExistsAsync(filePath, cancellationToken) is false)
            throw new ResourceNotFoundException(Localizer[nameof(AppStrings.UserImageCouldNotBeFound)]);

        user.ProfileImageName = null;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ResourceValidationException(result.Errors.Select(err => new LocalizedString(err.Code, err.Description)).ToArray());

        await blobStorage.DeleteAsync(filePath, cancellationToken);

        await PublishUserProfileUpdated(user.Map(), cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("{userId}")]
    [AppResponseCache(MaxAge = 3600 * 24 * 7, UserAgnostic = true)]
    public async Task<IActionResult> GetProfileImage(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user?.ProfileImageName is null)
            throw new ResourceNotFoundException();

        var filePath = $"{AppSettings.UserProfileImagesDir}{user.ProfileImageName}";

        if (await blobStorage.ExistsAsync(filePath, cancellationToken) is false)
            return new EmptyResult();

        return File(await blobStorage.OpenReadAsync(filePath, cancellationToken), "image/webp", enableRangeProcessing: true);
    }

    private async Task PublishUserProfileUpdated(UserDto user, CancellationToken cancellationToken)
    {
        // Notify other sessions of the user that user's info has been updated, so they'll update their UI.
        var currentUserSessionId = User.GetSessionId();
        var userSessionIdsExceptCurrentUserSessionId = await DbContext.UserSessions
            .Where(us => us.UserId == user.Id && us.Id != currentUserSessionId && us.SignalRConnectionId != null)
        .Select(us => us.SignalRConnectionId!)
            .ToArrayAsync(cancellationToken);
        await appHubContext.Clients.Clients(userSessionIdsExceptCurrentUserSessionId).SendAsync(SignalREvents.PUBLISH_MESSAGE, SharedPubSubMessages.PROFILE_UPDATED, user, cancellationToken);
    }

}
