using Java.Net;
using Android.OS;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Plugin.LocalNotification.Core.Models;
using Bit.TemplatePlayground.Client.Core.Components;

namespace Bit.TemplatePlayground.Client.Maui.Platforms.Android;

[IntentFilter([Intent.ActionView],
                        DataSchemes = ["https", "http"],
                        DataHosts = ["use-your-web-app-url-here.com"],
                        // the following app links will be opened in app instead of browser if the app is installed on Android device.
                        DataPaths = [PageUrls.Home],
                        DataPathPrefixes = [
                            "/en-US", "/en-GB", "/nl-NL", "/fa-IR", "sv-SE", "hi-IN", "zh-CN", "es-ES", "fr-FR", "ar-SA", "de-DE",
                            PageUrls.Confirm, PageUrls.ForgotPassword, PageUrls.Settings, PageUrls.ResetPassword, PageUrls.SignIn,
                            PageUrls.SignUp, PageUrls.NotAuthorized, PageUrls.NotFound, PageUrls.Terms, PageUrls.About,
                            PageUrls.Roles, PageUrls.Users,
                            PageUrls.AddOrEditProduct, PageUrls.Categories, PageUrls.Dashboard, PageUrls.Products,
                            PageUrls.SystemPrompts,
                            ],
                        AutoVerify = true,
                        Categories = [Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable])]

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

        var url = Intent?.DataString; // Handling universal deep links handling when the app was closed.
        if (string.IsNullOrWhiteSpace(url) is false)
        {
            _ = Routes.OpenUniversalLink(new URL(url).File ?? PageUrls.Home);
        }

        HandlePushNotificationTap(Intent); // Handling push notification taps when the app was closed.
        PushNotificationService.IsAvailable(default).ContinueWith(task =>
        {
            if (task.Result)
            {
                Services.AndroidPushNotificationService.Configure();
            }
        });
    }

    private static void HandlePushNotificationTap(Intent? intent)
    {
        if (intent is null)
            return;

        var dataString = intent.GetStringExtra(RequestConstants.ReturnRequest);
        string? pageUrl = null;
        if (string.IsNullOrEmpty(dataString) is false)
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

        pageUrl ??= intent?.Extras?.Get("pageUrl")?.ToString();
        if (string.IsNullOrEmpty(pageUrl) is false)
        {
            _ = Routes.OpenUniversalLink(pageUrl ?? PageUrls.Home); // The time that the notification received, the app was closed.
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        var action = intent!.Action; // Handling universal deep links handling when the is running.
        var url = intent.DataString;
        if (action is Intent.ActionView && string.IsNullOrWhiteSpace(url) is false)
        {
            _ = Routes.OpenUniversalLink(new URL(url).File ?? PageUrls.Home);
        }

        HandlePushNotificationTap(intent); // Handling push notification taps when the app is running.
    }

    public void OnSuccess(Java.Lang.Object? result)
    {
        PushNotificationService.Token = result!.ToString();
    }
}
