using Java.Net;
using Android.OS;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Bit.TemplatePlayground.Client.Core.Components;

namespace Bit.TemplatePlayground.Client.Maui.Platforms.Android;

[IntentFilter([Intent.ActionView],
                        DataSchemes = ["https", "http"],
                        DataHosts = ["use-your-web-app-url-here.com"],
                        // the following app links will be opened in app instead of browser if the app is installed on Android device.
                        DataPaths = [Urls.HomePage],
                        DataPathPrefixes = [
                            "/en-US", "/en-GB", "/nl-NL", "/fa-IR", "sv-SE", "hi-IN", "zh-CN", "es-ES", "fr-FR", "ar-SA", "de-DE",
                            Urls.ConfirmPage, Urls.ForgotPasswordPage, Urls.SettingsPage, Urls.ResetPasswordPage, Urls.SignInPage,
                            Urls.SignUpPage, Urls.NotAuthorizedPage, Urls.NotFoundPage, Urls.TermsPage, Urls.AboutPage, Urls.Authorize, Urls.AboutPage,
                            Urls.AddOrEditProductPage, Urls.CategoriesPage, Urls.DashboardPage, Urls.ProductsPage,
                            ],
                        AutoVerify = true,
                        Categories = [Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable])]

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public partial class MainActivity : MauiAppCompatActivity
{

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // https://github.com/dotnet/maui/issues/24742
        Theme?.ApplyStyle(Resource.Style.OptOutEdgeToEdgeEnforcement, force: false);

        base.OnCreate(savedInstanceState);

        var url = Intent?.DataString; // Handling universal deep links handling when the app was closed.
        if (string.IsNullOrWhiteSpace(url) is false)
        {
            _ = Routes.OpenUniversalLink(new URL(url).File ?? Urls.HomePage);
        }

    }


    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        var action = intent!.Action; // Handling universal deep links handling when the is running.
        var url = intent.DataString;
        if (action is Intent.ActionView && string.IsNullOrWhiteSpace(url) is false)
        {
            _ = Routes.OpenUniversalLink(new URL(url).File ?? Urls.HomePage);
        }

    }

}
