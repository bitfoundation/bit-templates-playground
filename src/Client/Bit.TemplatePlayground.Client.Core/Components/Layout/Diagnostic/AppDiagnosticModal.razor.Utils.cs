using System.Runtime.CompilerServices;
using System.Text;
using Bit.TemplatePlayground.Shared.Features.Identity;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bit.TemplatePlayground.Client.Core.Components.Layout.Diagnostic;

public partial class AppDiagnosticModal
{
    [AutoInject] private Cookie cookie = default!;
    [AutoInject] private AuthManager authManager = default!;
    [AutoInject] private IStorageService storageService = default!;
    [AutoInject] private IUserController userController = default!;
    [AutoInject] private IAppUpdateService appUpdateService = default!;
    [AutoInject] private ILogger<AppDiagnosticModal> logger = default!;

    private static async Task ThrowTestException()
    {
        await Task.Delay(250);

        showKnownException = !showKnownException;

        throw showKnownException
            ? new InvalidOperationException("Something critical happened.").WithData("TestData", 1)
            : new DomainLogicException("Something bad happened.").WithData("TestData", 2);
    }

    private async Task CallDiagnosticApi()
    {
        string? signalRConnectionId = null;
        string? pushNotificationSubscriptionDeviceId = null;

        try
        {
            signalRConnectionId = hubConnection.State == HubConnectionState.Connected ? hubConnection.ConnectionId : null;
        }
        catch (Exception exp)
        {
            logger.LogWarning(exp, "Failed to get SignalR ConnectionId for diagnostic.");
        }

        try
        {
            pushNotificationSubscriptionDeviceId = (await pushNotificationService.GetSubscription(CurrentCancellationToken))!.DeviceId;
        }
        catch (Exception exp)
        {
            logger.LogWarning(exp, "Failed to get Push Notification Subscription DeviceId for diagnostic.");
        }

        var serverResult = await diagnosticController.PerformDiagnostic(signalRConnectionId, pushNotificationSubscriptionDeviceId, CurrentCancellationToken);

        StringBuilder resultBuilder = new(serverResult);
        try
        {
            resultBuilder.AppendLine();

            resultBuilder.AppendLine($"IsDynamicCodeCompiled: {RuntimeFeature.IsDynamicCodeCompiled}");
            resultBuilder.AppendLine($"IsDynamicCodeSupported: {RuntimeFeature.IsDynamicCodeSupported}");
            resultBuilder.AppendLine($"Is Aot: {new StackTrace(false).GetFrame(0)?.GetMethod() is null}"); // No 100% Guaranteed way to detect AOT.

            resultBuilder.AppendLine();

            resultBuilder.AppendLine($"Env version: {Environment.Version}");
            resultBuilder.AppendLine($"64 bit process: {Environment.Is64BitProcess}");
            resultBuilder.AppendLine($"Privilaged process: {Environment.IsPrivilegedProcess}");

            resultBuilder.AppendLine();

            if (GC.GetConfigurationVariables().TryGetValue("ServerGC", out var serverGC))
                resultBuilder.AppendLine($"ServerGC: {serverGC}");

            if (GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrentGC))
                resultBuilder.AppendLine($"ConcurrentGC: {concurrentGC}");
        }
        catch (Exception exp)
        {
            resultBuilder.AppendLine($"{Environment.NewLine}Error while getting diagnostic data: {exp.Message}");
        }

        await messageBoxService.Show("Diagnostic Result", resultBuilder.ToString());
    }

    private async Task OpenDevTools()
    {
        await JSRuntime.InvokeVoidAsync("App.openDevTools");
    }

    private async Task CallGC()
    {
        SnackBarService.Show("Memory Before GC", GetMemoryUsage());

        await Task.Run(() =>
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        });

        SnackBarService.Show("Memory After GC", GetMemoryUsage());
    }

    private string GetMemoryUsage()
    {
        long memory = Environment.WorkingSet;
        return $"{memory / (1024.0 * 1024.0):F2} MB";
    }

    private async Task ClearAppFiles()
    {

        try
        {
            await authManager.SignOut(default);
        }
        catch (Exception exp)
        {
            logger.LogWarning(exp, "Failed to sign out during ClearAppStorage.");
        }

        await storageService.Clear(); // Blazor Hybrid stores key/value pairs outside webview's storage.

        await JSRuntime.ClearWebStorages();


        if (AppPlatform.IsBlazorHybrid is false)
        {
            await JSRuntime.InvokeVoidAsync("BitBswup.forceRefresh"); // Clears cache storages and uninstalls service-worker.
        }
        else
        {
            NavigationManager.Refresh(forceReload: true);
        }
    }

    private async Task UpdateApp()
    {
        await appUpdateService.ForceUpdate();
    }
}
