using Bit.TemplatePlayground.Shared.Dtos.PushNotification;

namespace Bit.TemplatePlayground.Client.Windows.Services;

public partial class WindowsPushNotificationService : PushNotificationServiceBase
{
    public override Task<PushNotificationSubscriptionDto?> GetSubscription(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public override Task RequestPermission(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
