using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

/// <summary>
/// Patient's credit wallet for day pass and consultation credits.
/// </summary>
public sealed class PatientWallet : Entity<Guid>
{
    public PatientWallet(
        Guid id,
        Guid patientAccountId,
        string currency,
        DateTimeOffset createdAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        Currency = currency;
        Balance = 0;
        TotalDeposited = 0;
        TotalSpent = 0;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid PatientAccountId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal TotalDeposited { get; private set; }
    public decimal TotalSpent { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Deposit(decimal amount, DateTimeOffset updatedAt)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));

        Balance += amount;
        TotalDeposited += amount;
        UpdatedAt = updatedAt;
    }

    public void Spend(decimal amount, DateTimeOffset updatedAt)
    {
        if (amount <= 0)
            throw new ArgumentException("Spend amount must be positive", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient wallet balance");

        Balance -= amount;
        TotalSpent += amount;
        UpdatedAt = updatedAt;
    }

    public void Hold(decimal amount, DateTimeOffset updatedAt)
    {
        if (amount <= 0)
            throw new ArgumentException("Hold amount must be positive", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient wallet balance for hold");

        UpdatedAt = updatedAt;
    }

    public void Refund(decimal amount, DateTimeOffset updatedAt)
    {
        if (amount <= 0)
            throw new ArgumentException("Refund amount must be positive", nameof(amount));

        Balance += amount;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        UpdatedAt = updatedAt;
    }

    public bool HasSufficientBalance(decimal amount)
    {
        return Balance >= amount && IsActive;
    }
}
