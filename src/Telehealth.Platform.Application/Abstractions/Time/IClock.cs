namespace Telehealth.Platform.Application.Abstractions.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
