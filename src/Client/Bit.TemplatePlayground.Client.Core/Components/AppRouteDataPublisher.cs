namespace Bit.TemplatePlayground.Client.Core.Components;

public partial class AppRouteDataPublisher : AppComponentBase
{
    [Parameter] public RouteData? RouteData { get; set; }

    private RouteData? lastPublishedRouteData;
    protected override async Task OnParamsSetAsync()
    {
        await base.OnParamsSetAsync();

        // RouteData is a fresh instance per navigation; skip redundant publishes on plain re-renders.
        if (ReferenceEquals(lastPublishedRouteData, RouteData)) return;
        lastPublishedRouteData = RouteData;

        PubSubService.Publish(ClientAppMessages.ROUTE_DATA_UPDATED, RouteData);
    }
}
