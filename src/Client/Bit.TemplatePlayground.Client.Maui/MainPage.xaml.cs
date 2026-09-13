using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;

namespace Bit.TemplatePlayground.Client.Maui;

public partial class MainPage
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HeadOutlet))]
    public MainPage()
    {
        InitializeComponent();
    }
}
