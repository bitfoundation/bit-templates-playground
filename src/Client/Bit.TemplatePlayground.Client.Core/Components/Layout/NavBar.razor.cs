namespace Bit.TemplatePlayground.Client.Core.Components.Layout;

public partial class NavBar
{
    /// <summary>
    /// Renders the bar inline in the desktop header (Snapchat's consumer nav) instead of pinned to the bottom of small screens.
    /// </summary>
    [Parameter] public bool InHeader { get; set; }


    /// <summary>
    /// The culture-prefixed form of a page url (e.g. /en-US/categories), so the automatic selection of the navbar
    /// still lights the current item up when the culture segment is part of the address.
    /// </summary>
    private static IEnumerable<string> CultureUrls(string url)
    {
        if (CultureInfoManager.InvariantGlobalization) return [];

        var path = url == PageUrls.Home ? string.Empty : url;

        return [$"/{CultureInfo.CurrentUICulture.Name}{path}"];
    }
}
