namespace Bit.TemplatePlayground.Client.Windows.Services;

public partial class WindowsDeviceCoordinator : IBitDeviceCoordinator
{
    public async Task ApplyTheme(bool isDark)
    {
        App.Current.ThemeMode = isDark ? System.Windows.ThemeMode.Dark : System.Windows.ThemeMode.Light;
    }
}
