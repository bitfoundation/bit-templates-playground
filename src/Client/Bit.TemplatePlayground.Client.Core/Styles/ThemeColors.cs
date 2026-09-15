namespace Bit.TemplatePlayground.Client.Core.Styles;

// [mirror] These must equal Bit.BlazorUI's --bit-clr-bg-pri for the active theme, because they paint the native
// chrome around the WebView (MAUI status bar and page background, Windows caption, the in-app browser toolbar) and
// the WebView's own background before any CSS has loaded. If they drift, the native surface and the page meet in a
// visible seam. The same two colors paint the web app's status bar (the theme-color meta tags of the host pages, which
// Scripts/theme.ts keeps up to date) and, for an installed PWA before those tags apply, manifest.json's theme_color -
// a manifest cannot be theme aware, so it carries the light color. Keep in sync with:
// Styles/app.scss, src/Client/Bit.TemplatePlayground.Client.Web/wwwroot/index.html, src/Client/Bit.TemplatePlayground.Client.Maui/wwwroot/index.html,
// src/Server/Bit.TemplatePlayground.Server.Web/Components/App.razor, src/Client/Bit.TemplatePlayground.Client.Web/wwwroot/manifest.json.
public partial class ThemeColors
{
    public static readonly string PrimaryDarkBgColor = "#1A1A1A";
    public static readonly string PrimaryLightBgColor = "#FFFFFF";
}
