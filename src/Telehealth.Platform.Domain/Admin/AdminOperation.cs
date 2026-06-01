using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Admin;

/// <summary>
/// Administrative operation log for sensitive actions.
/// </summary>
public sealed class AdminOperation : Entity<Guid>
{
    public AdminOperation(
        Guid id,
        Guid adminId,
        string adminName,
        string operationType,
        string targetType,
        string targetId,
        string? targetName,
        string action,
        object? oldValues,
        object? newValues,
        string? reason,
        string ipAddress,
        string userAgent,
        bool success,
        string? failureReason,
        DateTimeOffset createdAt)
        : base(id)
    {
        AdminId = adminId;
        AdminName = adminName;
        OperationType = operationType;
        TargetType = targetType;
        TargetId = targetId;
        TargetName = targetName;
        Action = action;
        OldValues = oldValues;
        NewValues = newValues;
        Reason = reason;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Success = success;
        FailureReason = failureReason;
        CreatedAt = createdAt;
    }

    public Guid AdminId { get; private set; }
    public string AdminName { get; private set; }
    public string OperationType { get; private set; }
    public string TargetType { get; private set; }
    public string TargetId { get; private set; }
    public string? TargetName { get; private set; }
    public string Action { get; private set; }
    public object? OldValues { get; private set; }
    public object? NewValues { get; private set; }
    public string? Reason { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsReverted { get; private set; }
    public DateTimeOffset? RevertedAt { get; private set; }
    public Guid? RevertedBy { get; private set; }

    public void MarkAsReverted(Guid revertedBy, DateTimeOffset revertedAt)
    {
        IsReverted = true;
        RevertedAt = revertedAt;
        RevertedBy = revertedBy;
    }

    public bool IsDataModification()
    {
        return OperationType is "Update" or "Delete" or "Create";
    }
}

public enum AdminOperationType
{
    View,
    Create,
    Update,
    Delete,
    Suspend,
    Activate,
    Verify,
    Reject,
    Refund,
    Export,
    Import,
    Override,
    Revert
}
