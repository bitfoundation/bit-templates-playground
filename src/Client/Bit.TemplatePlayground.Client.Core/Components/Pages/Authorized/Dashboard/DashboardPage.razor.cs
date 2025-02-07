using Microsoft.AspNetCore.Components.WebAssembly.Services;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Authorized.Dashboard;

public partial class DashboardPage
{
    protected override string? Title => Localizer[nameof(AppStrings.Dashboard)];
    protected override string? Subtitle => Localizer[nameof(AppStrings.DashboardSubtitle)];

    [AutoInject] LazyAssemblyLoader lazyAssemblyLoader = default!;

    private bool isLoadingAssemblies = true;
    private Action? unsubscribe;

    protected override async Task OnInitAsync()
    {
        unsubscribe = PubSubService.Subscribe(SharedPubSubMessages.DASHBOARD_DATA_CHANGED, async _ =>
        {
            NavigationManager.NavigateTo(Urls.DashboardPage, replace: true);
        });
        try
        {
            if (AppPlatform.IsBrowser)
            {
                await lazyAssemblyLoader.LoadAssembliesAsync([
                    "System.Private.Xml.wasm", "System.Data.Common.wasm",
                    "Newtonsoft.Json.wasm"]
                    );
            }
        }
        finally
        {
            isLoadingAssemblies = false;
        }

        await base.OnInitAsync();
    }

    protected override ValueTask DisposeAsync(bool disposing)
    {
        unsubscribe?.Invoke();

        return base.DisposeAsync(disposing);
    }
}
