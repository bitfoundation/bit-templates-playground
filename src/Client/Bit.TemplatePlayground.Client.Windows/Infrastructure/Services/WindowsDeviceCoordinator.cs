// [mirror] IBitDeviceCoordinator - applying the theme to native chrome - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Maui/Infrastructure/Services/MauiDeviceCoordinator.cs

using Bit.TemplatePlayground.Client.Core.Styles;

namespace Bit.TemplatePlayground.Client.Windows.Infrastructure.Services;

public partial class WindowsDeviceCoordinator : IBitDeviceCoordinator
{
    public async Task ApplyTheme(bool isDark)
    {
        Application.SetColorMode(isDark ? SystemColorMode.Dark : SystemColorMode.Classic);
        Application.OpenForms[0]!.FormCaptionBackColor = ColorTranslator.FromHtml(isDark ? ThemeColors.PrimaryDarkBgColor : ThemeColors.PrimaryLightBgColor);
    }
}
