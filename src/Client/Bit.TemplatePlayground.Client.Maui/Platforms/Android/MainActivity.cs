using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Bit.TemplatePlayground.Client.Core.Components;
using Java.Net;
using Plugin.LocalNotification.Core.Models;

namespace Bit.TemplatePlayground.Client.Maui.Platforms.Android;

[IntentFilter([Intent.ActionView],
                        DataSchemes = ["https", "http"],
                        DataHosts = ["use-your-web-app-url-here.com"],
                        // the following app links will be opened in app instead of browser if the app is installed on Android device.
                        DataPaths = [PageUrls.Home],
                        DataPathPrefixes = [
                            "/en-US", "/en-GB", "/nl-NL", "/fa-IR", "/sv-SE", "/hi-IN", "/zh-CN", "/es-ES", "/fr-FR", "/ar-SA", "/de-DE",
                            "/en-us", "/en-gb", "/nl-nl", "/fa-ir", "/sv-se", "/hi-in", "/zh-cn", "/es-es", "/fr-fr", "/ar-sa", "/de-de",
                            PageUrls.Confirm, PageUrls.ForgotPassword, PageUrls.Settings, PageUrls.ResetPassword, PageUrls.SignIn,
                            PageUrls.SignUp, PageUrls.NotAuthorized, PageUrls.NotFound, PageUrls.Terms, PageUrls.PrivacyPolicy, PageUrls.About,
                            PageUrls.Roles, PageUrls.Users, PageUrls.OAuthClients, PageUrls.OAuthConsent,
                            PageUrls.ManageMyTenants, PageUrls.ManageAllTenants,
                            PageUrls.AddOrEditProduct, PageUrls.Categories, PageUrls.Dashboard, PageUrls.Products,
                            PageUrls.SystemPrompts,
                            ],
                        AutoVerify = true,
                        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable])]

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public partial class MainActivity : MauiAppCompatActivity
    , IOnSuccessListener
{
    private IPushNotificationService PushNotificationService => IPlatformApplication.Current!.Services.GetRequiredService<IPushNotificationService>();

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // https://github.com/dotnet/maui/issues/24742
        Theme?.ApplyStyle(Resource.Style.OptOutEdgeToEdgeEnforcement, force: false);

        base.OnCreate(savedInstanceState);

        OpenDeepLink(Intent); // Handling universal deep links handling when the app was closed.

        HandlePushNotificationTap(Intent); // Handling push notification taps when the app was closed.
        PushNotificationService.IsAvailable(default).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                MauiProgram.LogException(task.Exception, reportedBy: nameof(IPushNotificationService.IsAvailable));
                return;
            }

            if (task.Result)
            {
                Services.AndroidPushNotificationService.Configure();
            }
        }, TaskScheduler.Default);
    }

    /// <summary>
    /// The activity is exported (it is the launcher activity), so any app on the device can send it an explicit
    /// intent whose data is not an http(s) url at all. <c>new URL("myscheme://x")</c> throws MalformedURLException,
    /// and an unhandled throw inside <c>OnCreate</c> kills the process on launch, so the parse is guarded here.
    /// <c>Routes.OpenUniversalLink</c> validates the resulting path before navigating to it.
    /// </summary>
    private static void OpenDeepLink(Intent? intent)
    {
        var url = intent?.DataString;
        if (string.IsNullOrWhiteSpace(url))
            return;

        string? path;
        try
        {
            path = new URL(url).File;
        }
        catch (Exception exp)
        {
            MauiProgram.LogException(exp, reportedBy: nameof(OpenDeepLink));
            return;
        }

        _ = Routes.OpenUniversalLink(string.IsNullOrWhiteSpace(path) ? PageUrls.Home : path);
    }

    private static void HandlePushNotificationTap(Intent? intent)
    {
        if (intent is null)
            return;

        var dataString = intent.GetStringExtra(RequestConstants.ReturnRequest);
        string? pageUrl = null;
        if (string.IsNullOrWhiteSpace(dataString) is false)
        {
            try
            {
                var request = JsonSerializer.Deserialize<NotificationRequest>(dataString, options: new()
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                });
                if (request?.ReturningData is not null)
                {
                    var returningData = JsonSerializer.Deserialize<Dictionary<string, object>>(request.ReturningData);
                    if (returningData?.ContainsKey("pageUrl") is true)
                    {
                        pageUrl = returningData["pageUrl"]?.ToString(); // The time that the notification received, the app was open. (See PushNotificationFirebaseMessagingService's OnMessageReceived)
                    }
                }
            }
            catch (JsonException exp)
            {
                MauiProgram.LogException(exp, reportedBy: nameof(HandlePushNotificationTap));
            }
        }

        pageUrl ??= intent?.Extras?.Get("pageUrl")?.ToString();
        if (string.IsNullOrWhiteSpace(pageUrl) is false)
        {
            _ = Routes.OpenUniversalLink(pageUrl ?? PageUrls.Home); // The time that the notification received, the app was closed.
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent!.Action is Intent.ActionView) // Handling universal deep links handling when the is running.
        {
            OpenDeepLink(intent);
        }

        HandlePushNotificationTap(intent); // Handling push notification taps when the app is running.
    }

    public void OnSuccess(Java.Lang.Object? result)
    {
        PushNotificationService.Token = result!.ToString();
    }
}
