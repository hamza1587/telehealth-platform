using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Compliance;

/// <summary>
/// Data retention policy configuration.
/// </summary>
public sealed class DataRetentionPolicy : Entity<Guid>
{
    public DataRetentionPolicy(
        Guid id,
        string dataType,
        string? userType,
        int retentionDays,
        bool allowEarlyDeletion,
        string legalBasis,
        string purpose,
        bool autoDelete,
        DateTimeOffset createdAt)
        : base(id)
    {
        DataType = dataType;
        UserType = userType;
        RetentionDays = retentionDays;
        AllowEarlyDeletion = allowEarlyDeletion;
        LegalBasis = legalBasis;
        Purpose = purpose;
        AutoDelete = autoDelete;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string DataType { get; private set; }
    public string? UserType { get; private set; }
    public int RetentionDays { get; private set; }
    public bool AllowEarlyDeletion { get; private set; }
    public string LegalBasis { get; private set; }
    public string Purpose { get; private set; }
    public bool AutoDelete { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        int retentionDays,
        bool allowEarlyDeletion,
        bool autoDelete,
        DateTimeOffset updatedAt)
    {
        RetentionDays = retentionDays;
        AllowEarlyDeletion = allowEarlyDeletion;
        AutoDelete = autoDelete;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    public DateTimeOffset CalculateExpirationDate(DateTimeOffset createdAt)
    {
        return createdAt.AddDays(RetentionDays);
    }

    public bool CanDeleteEarly()
    {
        return AllowEarlyDeletion;
    }
}
