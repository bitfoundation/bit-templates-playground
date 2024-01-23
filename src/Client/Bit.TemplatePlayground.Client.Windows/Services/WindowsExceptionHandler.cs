﻿using Bit.TemplatePlayground.Client.Core.Services;

namespace Bit.TemplatePlayground.Client.Windows.Services;

public partial class WindowsExceptionHandler : ExceptionHandlerBase
{
    public override void Handle(Exception exception, IDictionary<string, object?>? parameters = null)
    {
        if (exception is TaskCanceledException)
        {
            return;
        }

        base.Handle(exception, parameters);
    }
}
