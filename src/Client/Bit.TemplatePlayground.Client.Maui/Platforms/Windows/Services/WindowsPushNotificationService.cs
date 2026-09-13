// [mirror] not implemented push notification service of the windows targets - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Windows/Infrastructure/Services/WindowsPushNotificationService.cs

using Bit.TemplatePlayground.Shared.Features.PushNotification;

namespace Bit.TemplatePlayground.Client.Maui.Platforms.Windows.Services;

public partial class WindowsPushNotificationService : PushNotificationServiceBase
{
    public override Task<PushNotificationSubscriptionDto?> GetSubscription(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public override Task RequestPermission(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
