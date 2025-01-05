using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]


namespace Bit.TemplatePlayground.Client.Maui.Platforms.Android;

[Application(
#if Development
    UsesCleartextTraffic = true,
#endif
    AllowBackup = true,
    SupportsRtl = true
)]
public partial class MainApplication(IntPtr handle, JniHandleOwnership ownership) 
    : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram
        .CreateMauiApp();
}
