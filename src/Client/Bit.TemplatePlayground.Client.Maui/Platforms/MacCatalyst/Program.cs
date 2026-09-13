// [mirror] apple entry point - keep in sync with:
// - src/Client/Bit.TemplatePlayground.Client.Maui/Platforms/iOS/Program.cs

using UIKit;

namespace Bit.TemplatePlayground.Client.Maui.Platforms.MacCatalyst;

public partial class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
