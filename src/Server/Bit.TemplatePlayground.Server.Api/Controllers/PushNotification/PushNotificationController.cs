using Bit.TemplatePlayground.Server.Api.Services;
using Bit.TemplatePlayground.Shared.Dtos.PushNotification;
using Bit.TemplatePlayground.Shared.Controllers.PushNotification;

namespace Bit.TemplatePlayground.Server.Api.Controllers.PushNotification;

[Route("api/[controller]/[action]")]
[ApiController, AllowAnonymous]
public partial class PushNotificationController : AppControllerBase, IPushNotificationController
{
    [AutoInject] PushNotificationService pushNotificationService = default!;

    [HttpPost]
    public async Task Subscribe([Required] PushNotificationSubscriptionDto subscription, CancellationToken cancellationToken)
    {
        HttpContext.ThrowIfContainsExpiredAccessToken();

        await pushNotificationService.Subscribe(subscription, cancellationToken);
    }
}
