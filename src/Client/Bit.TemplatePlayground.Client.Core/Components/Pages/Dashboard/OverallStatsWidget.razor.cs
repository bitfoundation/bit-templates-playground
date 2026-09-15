using Bit.TemplatePlayground.Shared.Features.Dashboard;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Dashboard;

public partial class OverallStatsWidget
{
    [AutoInject] IDashboardController dashboardController = default!;

    private bool isLoading;
    private OverallAnalyticsStatsDataResponseDto dto = new();
    private Action? unsubscribe;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        // Instead of reloading the whole app, refresh only this widget's data when the dashboard changes.
        unsubscribe = PubSubService.Subscribe(SharedAppMessages.DASHBOARD_DATA_CHANGED, async _ => await InvokeAsync(GetData));

        await GetData();
    }

    private async Task GetData()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            dto = await dashboardController.GetOverallAnalyticsStatsData(CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing);

        unsubscribe?.Invoke();
    }
}
