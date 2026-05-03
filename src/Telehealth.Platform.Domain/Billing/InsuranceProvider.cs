using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Billing;

public class InsuranceProvider : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private InsuranceProvider(
        Guid id,
        string name,
        string code,
        string countryCode) : base(id)
    {
        Name = name;
        Code = code;
        CountryCode = countryCode;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static InsuranceProvider Create(
        string name,
        string code,
        string countryCode,
        string? phoneNumber = null,
        string? email = null,
        string? website = null)
    {
        return new InsuranceProvider(Guid.NewGuid(), name, code, countryCode)
        {
            PhoneNumber = phoneNumber,
            Email = email,
            Website = website
        };
    }

    public void Update(
        string name,
        string? phoneNumber,
        string? email,
        string? website,
        bool isActive)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Email = email;
        Website = website;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}