using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Application.Consultations;

namespace Telehealth.Platform.Application.Abstractions.Consultations;

public interface ITeleconsultationService
{
    Task<ConsultationSession> StartSessionAsync(
        Guid bookingId,
        string videoProvider,
        string videoRoomId,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> GetSessionByBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<ParticipantEvent> RecordParticipantEventAsync(
        Guid sessionId,
        ParticipantEventType eventType,
        string? participantType,
        string? participantId,
        Dictionary<string, object>? metadata,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> UpdateSessionStatusAsync(
        Guid sessionId,
        ConsultationSessionStatus status,
        CancellationToken cancellationToken = default);

    Task EndSessionAsync(
        Guid sessionId,
        long billableSeconds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ConsultationSession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(
        Guid patientId,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> BookAppointmentAsync(
        Guid patientId,
        BookAppointmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task CancelAppointmentAsync(
        Guid appointmentId,
        Guid patientId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> RescheduleAppointmentAsync(
        Guid appointmentId,
        Guid patientId,
        RescheduleAppointmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> GetAppointmentAsync(
        Guid appointmentId,
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task ConfirmAppointmentAsync(
        Guid appointmentId,
        Guid doctorId,
        CancellationToken cancellationToken = default);

    Task RejectAppointmentAsync(
        Guid appointmentId,
        Guid doctorId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DoctorAppointmentDto>> GetDoctorUpcomingAppointmentsAsync(
        Guid doctorId,
        int daysAhead,
        CancellationToken cancellationToken = default);

    Task<DoctorScheduleDto> GetDoctorScheduleAsync(
        Guid doctorId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task SetDoctorAvailabilitySlotsAsync(
        Guid doctorId,
        SetAvailabilityRequestDto request,
        CancellationToken cancellationToken = default);

    Task RemoveDoctorAvailabilitySlotAsync(
        Guid doctorId,
        Guid slotId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AvailabilitySlotDto>> GetDoctorAvailabilitySlotsAsync(
        Guid doctorId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}