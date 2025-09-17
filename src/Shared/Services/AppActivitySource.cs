using System.Diagnostics.Metrics;

namespace Bit.TemplatePlayground.Shared.Services;

/// <summary>
/// Open telemetry activity source for the application.
/// </summary>
public class AppActivitySource
{
    public static readonly ActivitySource CurrentActivity = new("Bit.TemplatePlayground", typeof(AppActivitySource).Assembly.GetName().Version!.ToString());

    public static readonly Meter CurrentMeter = new("Bit.TemplatePlayground", typeof(AppActivitySource).Assembly.GetName().Version!.ToString());
}
