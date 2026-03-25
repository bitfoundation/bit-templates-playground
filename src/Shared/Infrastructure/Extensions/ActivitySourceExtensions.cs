namespace System.Diagnostics;

public static class ActivitySourceExtensions
{
    private static readonly ActivitySource current = new("Bit.TemplatePlayground", typeof(ActivitySourceExtensions).Assembly.GetName().Version!.ToString());

    extension(ActivitySource source)
    {
        /// <summary>
        /// Open telemetry activity source for the application.
        /// </summary>
        public static ActivitySource Current => current;
    }
}
