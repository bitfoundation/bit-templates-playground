namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Settings;

public partial class SettingsPage
{
    [Parameter] public string? Section { get; set; }

    private bool isLoading;
}
