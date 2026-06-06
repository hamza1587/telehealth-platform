using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Notifications;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Domain.Notifications;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.BackgroundJobs;

/// <summary>
/// Sends appointment reminders 24h and 1h before scheduled consultations.
/// Registered as a recurring Hangfire job (every 15 minutes).
/// </summary>
public class AppointmentReminderJob
{
    private readonly PlatformDbContext _db;
    private readonly INotificationService _notifications;
    private readonly ILogger<AppointmentReminderJob> _logger;

    private static readonly ConsultationBookingStatus[] ActiveStatuses =
    [
        ConsultationBookingStatus.Confirmed,
        ConsultationBookingStatus.PatientWaiting,
        ConsultationBookingStatus.DoctorWaiting,
    ];

    public AppointmentReminderJob(
        PlatformDbContext db,
        INotificationService notifications,
        ILogger<AppointmentReminderJob> logger)
    {
        _db = db;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var margin = TimeSpan.FromMinutes(8); // job runs every 15m; 8m margin avoids double-fire

        await SendRemindersAsync(now.AddHours(24), "tomorrow", "24h", cancellationToken, margin);
        await SendRemindersAsync(now.AddHours(1), "in 1 hour", "1h", cancellationToken, margin);
    }

    private async Task SendRemindersAsync(
        DateTimeOffset window,
        string timeLabel,
        string tag,
        CancellationToken cancellationToken,
        TimeSpan margin)
    {
        var bookings = await _db.ConsultationBookings
            .Where(b => b.ScheduledStartsAt.HasValue
                     && b.ScheduledStartsAt >= window - margin
                     && b.ScheduledStartsAt <= window + margin
                     && ActiveStatuses.Contains(b.Status))
            .ToListAsync(cancellationToken);

        foreach (var booking in bookings)
        {
            try
            {
                var startsAt = booking.ScheduledStartsAt!.Value;
                await _notifications.CreateNotificationAsync(
                    booking.PatientAccountId,
                    NotificationType.Appointment,
                    $"Appointment {timeLabel}",
                    $"Your consultation with {booking.DoctorName} is scheduled for {startsAt:HH:mm} UTC.",
                    "/consultations",
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "{Tag} reminder sent for booking {BookingId}", tag, booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {Tag} reminder for booking {BookingId}", tag, booking.Id);
            }
        }

        _logger.LogDebug("AppointmentReminderJob [{Tag}]: processed {Count} bookings", tag, bookings.Count);
    }
}
