using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Admin;

/// <summary>
/// System-wide configuration settings.
/// </summary>
public sealed class SystemSetting : Entity<Guid>
{
    public SystemSetting(
        Guid id,
        string key,
        string value,
        string valueType,
        string? description,
        string? category,
        bool isEncrypted,
        DateTimeOffset createdAt)
        : base(id)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
        Description = description;
        Category = category;
        IsEncrypted = isEncrypted;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Key { get; private set; }
    public string Value { get; private set; }
    public string ValueType { get; private set; }
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string value, string? description, DateTimeOffset updatedAt)
    {
        Value = value;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public void ChangeCategory(string category, DateTimeOffset updatedAt)
    {
        Category = category;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    public T? GetValueAs<T>()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(Value);
        }
        catch
        {
            return default;
        }
    }

    public void SetValue<T>(T value, DateTimeOffset updatedAt)
    {
        Value = System.Text.Json.JsonSerializer.Serialize(value);
        UpdatedAt = updatedAt;
    }
}
