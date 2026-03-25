namespace Bit.TemplatePlayground.Shared.Infrastructure.Services.Contracts;

public interface IDateTimeProvider
{
    DateTimeOffset GetCurrentDateTime();
}
