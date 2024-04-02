using Bit.TemplatePlayground.Client.Core.Services;

namespace Bit.TemplatePlayground.Client.Windows.Services;

public partial class WindowsExceptionHandler : ExceptionHandlerBase
{
    protected override void Handle(Exception exception, Dictionary<string, object> parameters)
    {
        base.Handle(exception, parameters);
    }
}
