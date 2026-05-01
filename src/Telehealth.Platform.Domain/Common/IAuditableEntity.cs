namespace Telehealth.Platform.Domain.Common;

/// <summary>
/// Interface for entities that track audit information.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// When the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the entity was last updated.
    /// </summary>
    DateTimeOffset UpdatedAt { get; set; }
}
