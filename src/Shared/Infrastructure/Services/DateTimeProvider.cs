namespace Bit.TemplatePlayground.Shared.Infrastructure.Services;

public partial class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset GetCurrentDateTime()
    {
        return DateTimeOffset.UtcNow;
    }
}
