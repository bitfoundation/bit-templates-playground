using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;

namespace Bit.TemplatePlayground.Client.Maui;

public partial class MainPage
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HeadOutlet))]
    public MainPage(ClientMauiSettings clientMauiSettings)
    {
        InitializeComponent();
        AppWebView.RootComponents.Add(new()
        {
            ComponentType = typeof(HeadOutlet),
            Selector = "head::after"
        });
    }
}
