using Bit.TemplatePlayground.Shared.Dtos.PushNotification;

namespace Bit.TemplatePlayground.Shared.Controllers.PushNotification;

[Route("api/[controller]/[action]/")]
public interface IPushNotificationController : IAppController
{
    [HttpPost]
    Task Subscribe([Required] PushNotificationSubscriptionDto subscription, CancellationToken cancellationToken);
}
