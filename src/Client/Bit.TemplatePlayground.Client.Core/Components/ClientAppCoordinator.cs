using Microsoft.AspNetCore.Components.Routing;

namespace Bit.TemplatePlayground.Client.Core.Components;

/// <summary>
/// Manages the initialization and coordination of core services and settings within the client application.
/// This includes authentication state handling, telemetry setup, culture configuration, and optional
/// services such as SignalR connections, push notifications, and application insights.
/// </summary>
public partial class ClientAppCoordinator : AppComponentBase
{
    [AutoInject] private Navigator navigator = default!;
    [AutoInject] private IJSRuntime jsRuntime = default!;
    [AutoInject] private IStorageService storageService = default!;
    [AutoInject] private AuthenticationManager authManager = default!;
    [AutoInject] private ILogger<Navigator> navigatorLogger = default!;
    [AutoInject] private CultureInfoManager cultureInfoManager = default!;
    [AutoInject] private ILogger<AuthenticationManager> authLogger = default!;
    [AutoInject] private IBitDeviceCoordinator bitDeviceCoordinator = default!;

    protected override async Task OnInitAsync()
    {
        if (AppPlatform.IsBlazorHybrid)
        {
            if (CultureInfoManager.MultilingualEnabled)
            {
                cultureInfoManager.SetCurrentCulture(new Uri(NavigationManager.Uri).GetCulture() ??  // 1- Culture query string OR Route data request culture
                                                     await storageService.GetItem("Culture") ?? // 2- User settings
                                                     CultureInfo.CurrentUICulture.Name); // 3- OS settings
            }

            await SetupBodyClasses();
        }

        if (InPrerenderSession is false)
        {
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;
            AuthenticationManager.AuthenticationStateChanged += AuthenticationStateChanged;

            TelemetryContext.UserAgent = await navigator.GetUserAgent();
            TelemetryContext.TimeZone = await jsRuntime.GetTimeZone();
            TelemetryContext.Culture = CultureInfo.CurrentCulture.Name;
            TelemetryContext.PageUrl = NavigationManager.Uri;
            if (AppPlatform.IsBlazorHybrid is false)
            {
                TelemetryContext.OS = await jsRuntime.GetBrowserPlatform();
            }


            AuthenticationStateChanged(AuthenticationManager.GetAuthenticationStateAsync());
        }

        await base.OnInitAsync();
    }

    private void NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        TelemetryContext.PageUrl = e.Location;
        navigatorLogger.LogInformation("Navigation's location changed to {Location}", e.Location);
    }

    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            var user = (await task).User;
            var isAuthenticated = user.IsAuthenticated();
            TelemetryContext.UserId = isAuthenticated ? user.GetUserId() : null;
            TelemetryContext.UserSessionId = isAuthenticated ? user.GetSessionId() : null;


            var data = TelemetryContext.ToDictionary();
            using var scope = authLogger.BeginScope(data);
            {
                authLogger.LogInformation("Authentication state changed.");
            }


        }
        catch (Exception exp)
        {
            ExceptionHandler.Handle(exp);
        }
    }


    private async Task SetupBodyClasses()
    {
        var cssClasses = new List<string> { };

        if (AppPlatform.IsWindows)
        {
            cssClasses.Add("bit-windows");
        }
        else if (AppPlatform.IsMacOS)
        {
            cssClasses.Add("bit-macos");
        }
        else if (AppPlatform.IsIOS)
        {
            cssClasses.Add("bit-ios");
        }
        else if (AppPlatform.IsAndroid)
        {
            cssClasses.Add("bit-android");
        }

        var cssVariables = new Dictionary<string, string>
        {
        };

        await jsRuntime.ApplyBodyElementClasses(cssClasses, cssVariables);
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
        AuthenticationManager.AuthenticationStateChanged -= AuthenticationStateChanged;


        await base.DisposeAsync(disposing);
    }
}
