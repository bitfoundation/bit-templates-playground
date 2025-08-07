using Microsoft.AspNetCore.Components.WebAssembly.Services;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Dashboard;

public partial class DashboardPage
{
    [AutoInject] LazyAssemblyLoader lazyAssemblyLoader = default!;

    private bool isLoadingAssemblies = true;
    private Action? unsubscribe;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        unsubscribe = PubSubService.Subscribe(SharedPubSubMessages.DASHBOARD_DATA_CHANGED, async _ =>
        {
            NavigationManager.NavigateTo(PageUrls.Dashboard, replace: true);
        });
        try
        {
            if (AppPlatform.IsBrowser)
            {
                await lazyAssemblyLoader.LoadAssembliesAsync([
                    "System.Data.Common.wasm",
                    "Newtonsoft.Json.wasm",
                    "System.Private.Xml.wasm"]
                    );
            }
        }
        finally
        {
            isLoadingAssemblies = false;
        }
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing);

        unsubscribe?.Invoke();
    }
}
