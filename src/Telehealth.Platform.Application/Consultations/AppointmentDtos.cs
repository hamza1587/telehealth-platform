using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Application.Consultations;

public record AppointmentDto(
    Guid Id,
    Guid DoctorProfileId,
    string DoctorName,
    string Specialty,
    ConsultationMode Mode,
    ConsultationBookingStatus Status,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    Money Price,
    long DurationSeconds,
    DateTimeOffset CreatedAt);

public record DoctorAppointmentDto(
    Guid Id,
    string PatientName,
    string Purpose,
    ConsultationMode Mode,
    ConsultationBookingStatus Status,
    DateTimeOffset ScheduledAt,
    DateTimeOffset EndAt,
    bool RequiresConfirmation,
    string? MeetingLink);

public record BookAppointmentRequestDto(
    Guid DoctorProfileId,
    string SpecialtyCode,
    ConsultationMode ConsultationMode,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    string Notes);

public record CancelAppointmentRequestDto(string Reason);

public record RescheduleAppointmentRequestDto(
    DateTimeOffset NewStartsAt,
    DateTimeOffset NewEndsAt,
    string? Notes);

public record RejectAppointmentRequestDto(string Reason);

public record SetAvailabilityRequestDto(List<SetAvailabilitySlotDto> Slots);

public record SetAvailabilitySlotDto(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    ConsultationMode ConsultationMode,
    bool IsInstantEnabled);

public record ScheduleSlotDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsAvailable);

public record BookedSlotDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string PatientName,
    ConsultationBookingStatus Status);

public record ScheduleExceptionDto(
    Guid Id,
    DateTimeOffset Date,
    TimeSpan Duration,
    string Reason,
    bool IsHalfDay);

public record AvailabilitySlotDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    ConsultationMode Mode,
    bool IsInstantEnabled);

public record DoctorScheduleDto(
    Guid DoctorProfileId,
    List<ScheduleSlotDto> AvailableSlots,
    List<BookedSlotDto> BookedSlots,
    List<ScheduleExceptionDto> Exceptions);