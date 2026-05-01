using Telehealth.Platform.Application.Abstractions.Time;

namespace Telehealth.Platform.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
