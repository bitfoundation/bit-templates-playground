using System.Diagnostics.Metrics;
using Bit.TemplatePlayground.Server.Api.Features.Identity;
using Bit.TemplatePlayground.Server.Api.Features.Identity.Models;
using Bit.TemplatePlayground.Server.Api.Infrastructure.Services;
using Bit.TemplatePlayground.Server.Api.Infrastructure.SignalR;
using Bit.TemplatePlayground.Shared.Features.Attachments;
using FluentStorage.Storage;
using ImageMagick;
using Microsoft.AspNetCore.SignalR;

namespace Bit.TemplatePlayground.Server.Api.Features.Attachments;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public partial class AttachmentController : AppControllerBase, IAttachmentController
{
    [AutoInject] private IStore blobStorage = default!;
    [AutoInject] private UserManager<User> userManager = default!;

    [AutoInject] private IServiceProvider serviceProvider = default!;
    [AutoInject] private ILogger<AttachmentController> logger = default!;

    [AutoInject] private IHubContext<AppHub> appHubContext = default!;

    [AutoInject] private ResponseCacheService responseCacheService = default!;

    [AutoInject] private IConfiguration configuration = default!;

    // For open telemetry metrics
    private static readonly Histogram<double> updateResizeDurationHistogram = Meter.Current.CreateHistogram<double>("attachment.resize_duration", "ms", "Elapsed time to resize and persist an uploaded image");

    [HttpPost]
    [RequestSizeLimit(11 * 1024 * 1024 /*11MB*/)]
    public async Task<IActionResult> UploadUserProfilePicture(IFormFile? file, CancellationToken cancellationToken)
    {
        return await UploadAttachment(
             User.GetUserId(),
             [AttachmentKind.UserProfileImageSmall, AttachmentKind.UserProfileImageOriginal],
             file,
             cancellationToken);
    }

    [HttpPost("{productId}")]
    [RequestSizeLimit(11 * 1024 * 1024 /*11MB*/)]
    [Authorize(Policy = AppFeatures.AdminPanel.ProductCatalog_Manage)]
    [Authorize(Policy = AuthPolicies.TENANT_SELECTED)]
    public async Task<IActionResult> UploadProductPrimaryImage(Guid productId, IFormFile? file, CancellationToken cancellationToken)
    {
        await EnsureProductIsInCurrentTenant(productId, cancellationToken);

        return await UploadAttachment(
            productId,
            [AttachmentKind.ProductPrimaryImageMedium, AttachmentKind.ProductPrimaryImageOriginal],
            file,
            cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("{attachmentId}/{kind}")]
    [AppResponseCache(MaxAge = 3600 * 24 * 7, UserAgnostic = true)]
    public async Task<IActionResult> GetAttachment(Guid attachmentId, AttachmentKind kind, CancellationToken cancellationToken = default)
    {
        // If the backend is hosted behind a CDN (which is recommended for production), the GetAttachment method's returned stream will be cached on CDN edge servers.
        // Alternatively, you can generate URLs that allow clients to download files directly from the file storage, further reducing the load on the backend.
        // If security is a concern, you can generate short-lived signed URLs for the file storage. These signed URLs can be validated either at the CDN edge or on the file storage server, ensuring that only authorized users can access the files.

        var filePath = GetFilePath(attachmentId, kind);

        if (await blobStorage.ObjectExists(filePath, cancellationToken) is false)
            throw new ResourceNotFoundException().WithData("Reason", "The attachment does not exist.");

        var mimeType = kind switch
        {
            _ => "image/webp" // Currently, all attachment types are images.
        };

        return File(await blobStorage.OpenRead(filePath, cancellationToken), mimeType, enableRangeProcessing: true);
    }

    [HttpDelete]
    public async Task DeleteUserProfilePicture(CancellationToken cancellationToken)
    {
        await DeleteAttachment(User.GetUserId(), [AttachmentKind.UserProfileImageSmall, AttachmentKind.UserProfileImageOriginal], cancellationToken);
    }

    [HttpDelete("{productId}"), Authorize(Policy = AppFeatures.AdminPanel.ProductCatalog_Manage)]
    public async Task DeleteProductPrimaryImage(Guid productId, CancellationToken cancellationToken)
    {
        await EnsureProductIsInCurrentTenant(productId, cancellationToken);

        await DeleteAttachment(productId, [AttachmentKind.ProductPrimaryImageMedium, AttachmentKind.ProductPrimaryImageOriginal], cancellationToken);
    }

    /// <summary>
    /// Attachments aren't tenant-aware, so before creating/updating/deleting a product's images, the product must belong
    /// to the current tenant. Products that are being added aren't in the database yet, so they get a pass here.
    /// </summary>
    private async Task EnsureProductIsInCurrentTenant(Guid productId, CancellationToken cancellationToken)
    {
        var productTenantId = await DbContext.Products
            .IgnoreQueryFilters()
            .Where(p => p.Id == productId)
            .Select(p => (Guid?)p.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (productTenantId is not null && productTenantId != TenantProvider.GetCurrentTenantId())
            throw new ResourceNotFoundException().WithData("Reason", "The product belongs to another tenant.");
    }

    private async Task PublishUserProfileUpdated(User user, CancellationToken cancellationToken)
    {
        // Notify other sessions of the user that user's info has been updated, so they'll update their UI.
        var currentUserSessionId = User.GetSessionId();
        var userSessionIdsExceptCurrentUserSessionId = await DbContext.UserSessions
            .Where(us => us.UserId == user.Id && us.Id != currentUserSessionId && us.SignalRConnectionId != null)
            .Select(us => us.SignalRConnectionId!)
            .ToArrayAsync(cancellationToken);
        await appHubContext.Clients.Clients(userSessionIdsExceptCurrentUserSessionId).Publish(SharedAppMessages.PROFILE_UPDATED, user.Map(), cancellationToken);
    }

    private async Task DeleteAttachment(Guid attachmentId, AttachmentKind[] kinds, CancellationToken cancellationToken)
    {
        var attachments = await DbContext.Attachments.Where(p => p.Id == attachmentId && kinds.Contains(p.Kind)).ToArrayAsync(cancellationToken);

        foreach (var attachment in attachments)
        {
            var filePath = attachment.Path;

            if (await blobStorage.ObjectExists(filePath, cancellationToken) is false)
                throw new ResourceNotFoundException(Localizer[nameof(AppStrings.ImageCouldNotBeFound)]);

            await blobStorage.DeleteObject(filePath, cancellationToken);

            if (attachment.Kind is AttachmentKind.ProductPrimaryImageOriginal)
            {
                var product = await DbContext.Products.FindAsync([attachment.Id], cancellationToken);
                if (product is not null) // else means product is being added to the database.
                {
                    product.HasPrimaryImage = false;
                    product.PrimaryImageAltText = null;
                    await DbContext.SaveChangesAsync(cancellationToken);
                    await responseCacheService.PurgeProductCache(product.ShortId);
                }
            }

            if (attachment.Kind is AttachmentKind.UserProfileImageOriginal)
            {
                var user = await userManager.FindByIdAsync(User.GetUserId().ToString());
                user!.HasProfilePicture = false;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    throw new ResourceValidationException(result.Errors.Select(err => new LocalizedString(err.Code, err.Description)).ToArray());

                await PublishUserProfileUpdated(user, cancellationToken);
            }

            DbContext.Attachments.Remove(attachment);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<IActionResult> UploadAttachment(Guid attachmentId, AttachmentKind[] kinds, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            throw new BadRequestException().WithData("Reason", "No file provided.");

        string? altText = null; // For future use, e.g., AI-generated alt text.

        await DbContext.Attachments.Where(att => att.Id == attachmentId).ExecuteDeleteAsync(cancellationToken);

        foreach (var kind in kinds)
        {
            var attachment = new Attachment
            {
                Id = attachmentId,
                Kind = kind,
                Path = GetFilePath(attachmentId, kind, file.FileName),
            };

            if (await blobStorage.ObjectExists(attachment.Path, cancellationToken))
            {
                await blobStorage.DeleteObject(attachment.Path, cancellationToken);
            }

            (bool NeedsResize, uint Width, uint Height) imageResizeContext = kind switch
            {
                AttachmentKind.UserProfileImageSmall => (true, 256, 256),
                AttachmentKind.ProductPrimaryImageMedium => (true, 512, 512),
                _ => (false, 0, 0)
            };

            byte[]? imageBytes = null;

            if (imageResizeContext.NeedsResize)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                using MagickImage sourceImage = new(file.OpenReadStream());

                if (sourceImage.Width < imageResizeContext.Width || sourceImage.Height < imageResizeContext.Height)
                    return BadRequest(Localizer[nameof(AppStrings.ImageTooSmall), imageResizeContext.Width, imageResizeContext.Height, sourceImage.Width, sourceImage.Height].ToString());

                sourceImage.Resize(new MagickGeometry(imageResizeContext.Width, imageResizeContext.Height));

                await blobStorage.SetBytes(attachment.Path, imageBytes = sourceImage.ToByteArray(MagickFormat.WebP), cancellationToken: cancellationToken);

                updateResizeDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("kind", kind.ToString()));
            }
            else
            {
                await blobStorage.SetObject(attachment.Path, file.OpenReadStream(), cancellationToken: cancellationToken);
            }

            await DbContext.Attachments.AddAsync(attachment, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);

            if (attachment.Kind is AttachmentKind.ProductPrimaryImageMedium)
            {
                if (serviceProvider.GetKeyedService<Microsoft.Agents.AI.AIAgent>("AnalyzeProductImageAgent") is Microsoft.Agents.AI.AIAgent analyzeProductImageAgent)
                {
                    ChatOptions chatOptions = new()
                    {
                        ResponseFormat = ChatResponseFormat.Json,
                        AdditionalProperties = new()
                        {
                            ["response_format"] = new { type = "json_object" }
                        }
                    };

                    configuration.GetRequiredSection("AI:ChatOptions").Bind(chatOptions);

                    var response = await analyzeProductImageAgent.RunAsync<AIImageReviewResponse>(
                        messages: [
                            new ChatMessage(ChatRole.User,
                                "Analyze this product image for our car catalog. Is this a valid car product image that meets our quality and content standards?")
                            {
                                Contents = [new DataContent(imageBytes, "image/webp")]
                            }
                        ],
                        cancellationToken: cancellationToken,
                        options: new Microsoft.Agents.AI.ChatClientAgentRunOptions(chatOptions));

                    if (response.Result.IsCar is false)
                    {
                        logger.LogWarning(
                            "Image validation failed - Not a car product. Confidence: {Confidence}, Reasoning: {Reasoning}",
                            response.Result.Confidence,
                            response.Result.Reasoning);
                        return BadRequest(Localizer[nameof(AppStrings.ImageNotCarError)].ToString());
                    }

                    if (response.Result.Confidence < 0.85)
                    {
                        logger.LogWarning(
                            "Image analysis low confidence ({Confidence}). Reasoning: {Reasoning}. Alt text: {AltText}",
                            response.Result.Confidence,
                            response.Result.Reasoning,
                            response.Result.Alt);
                    }

                    altText = response.Result.Alt;
                }
            }

            if (kind is AttachmentKind.UserProfileImageSmall)
            {
                var user = await userManager.FindByIdAsync(User.GetUserId().ToString());
                user!.HasProfilePicture = true;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    throw new ResourceValidationException(result.Errors.Select(err => new LocalizedString(err.Code, err.Description)).ToArray());

                await PublishUserProfileUpdated(user, cancellationToken);
            }
        }

        return Ok(altText);
    }

    private string GetFilePath(Guid attachmentId, AttachmentKind kind, string? fileName = null)
    {
        var filePath = kind switch
        {
            AttachmentKind.ProductPrimaryImageMedium => $"{AppSettings.ProductImagesDir}{attachmentId}_{kind}.webp",
            AttachmentKind.ProductPrimaryImageOriginal => $"{AppSettings.ProductImagesDir}{attachmentId}_{kind}{Path.GetExtension(fileName)}",
            AttachmentKind.UserProfileImageSmall => $"{AppSettings.UserProfileImagesDir}{attachmentId}_{kind}.webp",
            AttachmentKind.UserProfileImageOriginal => $"{AppSettings.UserProfileImagesDir}{attachmentId}_{kind}{Path.GetExtension(fileName)}",
            _ => throw new NotImplementedException()
        };

        filePath = Environment.ExpandEnvironmentVariables(filePath);

        return filePath;
    }

    public record AIImageReviewResponse(bool IsCar, double Confidence, string? Alt, string? Reasoning);
}
