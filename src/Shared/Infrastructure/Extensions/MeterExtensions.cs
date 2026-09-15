namespace System.Diagnostics.Metrics;

public static class MeterExtensions
{
    private static readonly Meter current = new("Bit.TemplatePlayground", typeof(MeterExtensions).Assembly.GetName().Version!.ToString());

    extension(Meter source)
    {
        /// <summary>
        /// Open telemetry meter for the application.
        /// </summary>
        public static Meter Current => current;
    }
}
