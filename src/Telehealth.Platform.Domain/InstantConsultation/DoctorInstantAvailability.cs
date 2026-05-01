using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Domain.InstantConsultation;

/// <summary>
/// Doctor's availability for instant consultations.
/// </summary>
public sealed class DoctorInstantAvailability : Entity<Guid>
{
    public DoctorInstantAvailability(
        Guid id,
        Guid doctorProfileId,
        bool isAvailable,
        int? maxQueueSize,
        decimal? minPricePerSecond,
        string currency,
        List<string> availableSpecialties,
        List<ConsultationMode> availableModes,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        IsAvailable = isAvailable;
        MaxQueueSize = maxQueueSize;
        MinPricePerSecond = minPricePerSecond;
        Currency = currency;
        AvailableSpecialties = availableSpecialties;
        AvailableModes = availableModes;
        CurrentQueueSize = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; }
    public bool IsAvailable { get; private set; }
    public int? MaxQueueSize { get; private set; }
    public int CurrentQueueSize { get; private set; }
    public decimal? MinPricePerSecond { get; private set; }
    public string Currency { get; private set; }
    public List<string> AvailableSpecialties { get; private set; }
    public List<ConsultationMode> AvailableModes { get; private set; }
    public DateTimeOffset LastStatusChangeAt { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void SetAvailable(DateTimeOffset changedAt)
    {
        IsAvailable = true;
        LastStatusChangeAt = changedAt;
        UpdatedAt = changedAt;
    }

    public void SetUnavailable(DateTimeOffset changedAt)
    {
        IsAvailable = false;
        LastStatusChangeAt = changedAt;
        UpdatedAt = changedAt;
    }

    public void UpdateSettings(
        int? maxQueueSize,
        decimal? minPricePerSecond,
        List<string> specialties,
        List<ConsultationMode> modes,
        DateTimeOffset updatedAt)
    {
        MaxQueueSize = maxQueueSize;
        MinPricePerSecond = minPricePerSecond;
        AvailableSpecialties = specialties;
        AvailableModes = modes;
        UpdatedAt = updatedAt;
    }

    public bool CanAcceptPatient()
    {
        if (!IsAvailable)
            return false;

        if (MaxQueueSize.HasValue && CurrentQueueSize >= MaxQueueSize.Value)
            return false;

        return true;
    }

    public void IncrementQueue(DateTimeOffset updatedAt)
    {
        CurrentQueueSize++;
        UpdatedAt = updatedAt;
    }

    public void DecrementQueue(DateTimeOffset updatedAt)
    {
        if (CurrentQueueSize > 0)
            CurrentQueueSize--;
        UpdatedAt = updatedAt;
    }

    public bool AcceptsSpecialty(string specialtyCode)
    {
        return AvailableSpecialties.Contains(specialtyCode, StringComparer.OrdinalIgnoreCase);
    }

    public bool AcceptsMode(ConsultationMode mode)
    {
        return AvailableModes.Contains(mode);
    }
}
