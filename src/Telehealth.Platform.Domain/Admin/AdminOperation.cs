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

    public Guid AdminId { get; }
    public string AdminName { get; }
    public string OperationType { get; }
    public string TargetType { get; }
    public string TargetId { get; }
    public string? TargetName { get; }
    public string Action { get; }
    public object? OldValues { get; }
    public object? NewValues { get; }
    public string? Reason { get; }
    public string IpAddress { get; }
    public string UserAgent { get; }
    public bool Success { get; }
    public string? FailureReason { get; }
    public DateTimeOffset CreatedAt { get; }
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
