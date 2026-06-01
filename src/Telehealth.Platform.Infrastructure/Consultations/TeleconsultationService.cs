using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Application.Abstractions.Doctors;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Application.Consultations;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Consultations;

public class TeleconsultationService : ITeleconsultationService
{
	private readonly PlatformDbContext _dbContext;
	private readonly IDoctorProfileService _doctorProfileService;

	public TeleconsultationService(PlatformDbContext dbContext, IDoctorProfileService doctorProfileService)
	{
		_dbContext = dbContext;
		_doctorProfileService = doctorProfileService;
	}

	public async Task<ConsultationSession> StartSessionAsync(Guid bookingId, string videoProvider, string videoRoomId, CancellationToken cancellationToken = default)
	{
		var sessionId = Guid.NewGuid();
		var session = new ConsultationSession(sessionId, bookingId, videoProvider, videoRoomId);
		await _dbContext.ConsultationSessions.AddAsync(session, cancellationToken);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return session;
	}

	public async Task<ConsultationSession> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
	{
		return await _dbContext.ConsultationSessions.Include(s => s.ParticipantEvents).FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
			?? throw new InvalidOperationException($"Session with ID {sessionId} not found");
	}

	public async Task<ConsultationSession> GetSessionByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
	{
		return await _dbContext.ConsultationSessions.Include(s => s.ParticipantEvents).FirstOrDefaultAsync(s => s.ConsultationBookingId == bookingId, cancellationToken)
			?? throw new InvalidOperationException($"No session found for booking ID {bookingId}");
	}

	public async Task<ParticipantEvent> RecordParticipantEventAsync(Guid sessionId, ParticipantEventType eventType, string? participantType, string? participantId, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default)
	{
		var session = await _dbContext.ConsultationSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
			?? throw new InvalidOperationException($"Session with ID {sessionId} not found");
		var participantEvent = new ParticipantEvent(Guid.NewGuid(), sessionId, eventType, DateTimeOffset.UtcNow);
		participantEvent.SetParticipant(participantType, participantId);
		if (metadata != null)
		{
			participantEvent.SetMetadata(metadata);
		}
		await _dbContext.ParticipantEvents.AddAsync(participantEvent, cancellationToken);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return participantEvent;
	}

	public async Task<ConsultationSession> UpdateSessionStatusAsync(Guid sessionId, ConsultationSessionStatus status, CancellationToken cancellationToken = default)
	{
		var session = await _dbContext.ConsultationSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
			?? throw new InvalidOperationException($"Session with ID {sessionId} not found");
		session.GetType().GetProperty("Status")?.SetValue(session, status);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return session;
	}

	public async Task EndSessionAsync(Guid sessionId, long billableSeconds, CancellationToken cancellationToken = default)
	{
		var session = await _dbContext.ConsultationSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
			?? throw new InvalidOperationException($"Session with ID {sessionId} not found");
		session.EndSession(billableSeconds);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task<IEnumerable<ConsultationSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.ConsultationSessions.Where(s => s.Status == ConsultationSessionStatus.InProgress).ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(Guid patientId, string? status, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
	{
		var query = _dbContext.ConsultationBookings.Where(b => b.PatientAccountId == patientId);

		if (!string.IsNullOrWhiteSpace(status))
		{
			query = query.Where(b => b.Status.ToString() == status);
		}

		if (from.HasValue)
		{
			query = query.Where(b => b.ScheduledStartsAt >= from);
		}

		if (to.HasValue)
		{
			query = query.Where(b => b.ScheduledEndsAt <= to);
		}

		return await query.Select(b => new AppointmentDto(b.Id, b.DoctorProfileId, b.DoctorName, b.Specialty, b.ConsultationMode, b.Status, b.ScheduledStartsAt ?? DateTimeOffset.UtcNow, b.ScheduledEndsAt ?? DateTimeOffset.UtcNow, new Money(b.PriceMinor, b.Currency), (long)(b.ScheduledEndsAt - b.ScheduledStartsAt ?? TimeSpan.Zero).TotalSeconds, b.CreatedAt)).ToListAsync(cancellationToken);
	}

	public async Task<AppointmentDto> BookAppointmentAsync(Guid patientId, BookAppointmentRequestDto request, CancellationToken cancellationToken = default)
	{
		var doctor = await _doctorProfileService.GetByIdAsync(request.DoctorProfileId, cancellationToken);

		if (doctor == null)
		{
			throw new InvalidOperationException("Doctor not found");
		}

		var slot = await _dbContext.DoctorAvailabilityWindows.FirstOrDefaultAsync(w => w.DoctorProfileId == request.DoctorProfileId && w.StartsAt == request.ScheduledStartsAt && w.EndsAt == request.ScheduledEndsAt, cancellationToken);

		if (slot == null)
		{
			throw new InvalidOperationException("Requested time slot is not available");
		}

		var duration = (long)(request.ScheduledEndsAt - request.ScheduledStartsAt).TotalSeconds;
		if (duration <= 0)
		{
			throw new InvalidOperationException("Invalid appointment duration");
		}

		var priceMinor = doctor.PricePerSecond * duration;

		var booking = new ConsultationBooking(Guid.NewGuid(), patientId, request.DoctorProfileId, request.SpecialtyCode, request.ConsultationMode, doctor.PrimarySpecialty, priceMinor, doctor.Currency);
		booking.SetScheduledAt(request.ScheduledStartsAt, request.ScheduledEndsAt);
		booking.SetDoctorInfo(doctor.DisplayName);
		booking.SetNotes(request.Notes);

		await _dbContext.ConsultationBookings.AddAsync(booking, cancellationToken);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return new AppointmentDto(booking.Id, booking.DoctorProfileId, booking.DoctorName, booking.Specialty, booking.ConsultationMode, booking.Status, booking.ScheduledStartsAt ?? DateTimeOffset.UtcNow, booking.ScheduledEndsAt ?? DateTimeOffset.UtcNow, new Money(booking.PriceMinor, booking.Currency), duration, booking.CreatedAt);
	}

	public async Task CancelAppointmentAsync(Guid appointmentId, Guid patientId, string reason, CancellationToken cancellationToken = default)
	{
		var booking = await _dbContext.ConsultationBookings.FirstOrDefaultAsync(b => b.Id == appointmentId && b.PatientAccountId == patientId, cancellationToken)
			?? throw new InvalidOperationException("Appointment not found");
		booking.Cancel(reason);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task<AppointmentDto> RescheduleAppointmentAsync(Guid appointmentId, Guid patientId, RescheduleAppointmentRequestDto request, CancellationToken cancellationToken = default)
	{
		var booking = await _dbContext.ConsultationBookings.FirstOrDefaultAsync(b => b.Id == appointmentId && b.PatientAccountId == patientId, cancellationToken)
			?? throw new InvalidOperationException("Appointment not found");
		booking.Reschedule(request.NewStartsAt, request.NewEndsAt);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return new AppointmentDto(booking.Id, booking.DoctorProfileId, booking.DoctorName, booking.Specialty, booking.ConsultationMode, booking.Status, booking.ScheduledStartsAt ?? DateTimeOffset.UtcNow, booking.ScheduledEndsAt ?? DateTimeOffset.UtcNow, new Money(booking.PriceMinor, booking.Currency), (long)(booking.ScheduledEndsAt - booking.ScheduledStartsAt ?? TimeSpan.Zero).TotalSeconds, booking.CreatedAt);
	}

	public async Task<IEnumerable<DoctorAvailabilityWindow>> GetAvailabilityAsync(Guid doctorProfileId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
	{
		return await _dbContext.DoctorAvailabilityWindows.Where(w => w.DoctorProfileId == doctorProfileId && w.StartsAt >= from && w.EndsAt <= to).ToListAsync(cancellationToken);
	}

	public async Task<AppointmentDto> GetAppointmentAsync(Guid appointmentId, Guid patientId, CancellationToken cancellationToken = default)
	{
		var booking = await _dbContext.ConsultationBookings.FirstOrDefaultAsync(b => b.Id == appointmentId && b.PatientAccountId == patientId, cancellationToken)
			?? throw new InvalidOperationException("Appointment not found");
		return new AppointmentDto(booking.Id, booking.DoctorProfileId, booking.DoctorName, booking.Specialty, booking.ConsultationMode, booking.Status, booking.ScheduledStartsAt ?? DateTimeOffset.UtcNow, booking.ScheduledEndsAt ?? DateTimeOffset.UtcNow, new Money(booking.PriceMinor, booking.Currency), (long)(booking.ScheduledEndsAt - booking.ScheduledStartsAt ?? TimeSpan.Zero).TotalSeconds, booking.CreatedAt);
	}

	public async Task ConfirmAppointmentAsync(Guid appointmentId, Guid doctorId, CancellationToken cancellationToken = default)
	{
		var booking = await _dbContext.ConsultationBookings.FirstOrDefaultAsync(b => b.Id == appointmentId, cancellationToken)
			?? throw new InvalidOperationException("Appointment not found");
		booking.Confirm();
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task RejectAppointmentAsync(Guid appointmentId, Guid doctorId, string reason, CancellationToken cancellationToken = default)
	{
		var booking = await _dbContext.ConsultationBookings.FirstOrDefaultAsync(b => b.Id == appointmentId, cancellationToken)
			?? throw new InvalidOperationException("Appointment not found");
		booking.Reject(reason);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task<IEnumerable<DoctorAppointmentDto>> GetDoctorUpcomingAppointmentsAsync(Guid doctorId, int daysAhead, CancellationToken cancellationToken = default)
	{
		var from = DateTimeOffset.UtcNow;
		var to = from.AddDays(daysAhead);
		return await _dbContext.ConsultationBookings.Where(b => b.DoctorProfileId == doctorId && b.ScheduledStartsAt >= from && b.ScheduledStartsAt <= to).Select(b => new DoctorAppointmentDto(b.Id, b.PatientName, b.Notes, b.ConsultationMode, b.Status, b.ScheduledStartsAt ?? DateTimeOffset.UtcNow, b.ScheduledEndsAt ?? DateTimeOffset.UtcNow, false, null)).ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<AvailabilitySlotDto>> GetDoctorAvailabilitySlotsAsync(Guid doctorId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
	{
		var fromDate = from ?? DateTimeOffset.UtcNow;
		var toDate = to ?? fromDate.AddDays(30);
		// ConsultationMode is stored as string in the domain entity; parse to enum for the DTO.
		var windows = await _dbContext.DoctorAvailabilityWindows
			.Where(w => w.DoctorProfileId == doctorId && w.StartsAt >= fromDate && w.EndsAt <= toDate)
			.ToListAsync(cancellationToken);
		return windows.Select(w => new AvailabilitySlotDto(
			w.Id,
			w.StartsAt,
			w.EndsAt,
			Enum.TryParse<ConsultationMode>(w.ConsultationMode, ignoreCase: true, out var mode) ? mode : ConsultationMode.Video,
			w.IsInstantEnabled));
	}

	public async Task<DoctorScheduleDto> GetDoctorScheduleAsync(Guid doctorId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
	{
		var fromDate = from ?? DateTimeOffset.UtcNow.Date;
		var toDate = to ?? fromDate.AddDays(14);
		var slots = await _dbContext.DoctorAvailabilityWindows.Where(w => w.DoctorProfileId == doctorId && w.StartsAt >= fromDate && w.EndsAt <= toDate).Select(w => new ScheduleSlotDto(w.Id, w.StartsAt, w.EndsAt, true)).ToListAsync(cancellationToken);
		var booked = await _dbContext.ConsultationBookings.Where(b => b.DoctorProfileId == doctorId && b.ScheduledStartsAt >= fromDate && b.ScheduledStartsAt <= toDate).Select(b => new BookedSlotDto(b.Id, b.ScheduledStartsAt ?? DateTimeOffset.UtcNow, b.ScheduledEndsAt ?? DateTimeOffset.UtcNow, b.PatientName, b.Status)).ToListAsync(cancellationToken);
		return new DoctorScheduleDto(doctorId, slots, booked, new List<ScheduleExceptionDto>());
	}

	public async Task SetDoctorAvailabilitySlotsAsync(Guid doctorId, SetAvailabilityRequestDto request, CancellationToken cancellationToken = default)
	{
		foreach (var slot in request.Slots)
		{
			// DoctorAvailabilityWindow stores ConsultationMode as a string; convert from enum.
			var window = DoctorAvailabilityWindow.Create(doctorId, slot.StartsAt, slot.EndsAt, slot.ConsultationMode.ToString(), slot.IsInstantEnabled);
			await _dbContext.DoctorAvailabilityWindows.AddAsync(window, cancellationToken);
		}
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task RemoveDoctorAvailabilitySlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default)
	{
		var slot = await _dbContext.DoctorAvailabilityWindows.FirstOrDefaultAsync(w => w.Id == slotId && w.DoctorProfileId == doctorId, cancellationToken)
			?? throw new InvalidOperationException("Slot not found");
		_dbContext.DoctorAvailabilityWindows.Remove(slot);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
