using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

public sealed class MfaChallenge : Entity<Guid>
{
    public MfaChallenge(
        Guid id,
        Guid userId,
        MfaMethodType challengeType,
        string challengeData,
        TimeSpan expirationTime) : base(id)
    {
        UserId = userId;
        ChallengeType = challengeType;
        ChallengeData = challengeData;
        ExpiresAt = DateTimeOffset.UtcNow.Add(expirationTime);
        CreatedAt = DateTimeOffset.UtcNow;
        VerificationAttempts = 0;
        IsVerified = false;
    }

    public Guid UserId { get; private set; }
    public MfaMethodType ChallengeType { get; private set; }
    public string ChallengeData { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int VerificationAttempts { get; private set; }
    public bool IsVerified { get; private set; }

    public void IncrementAttempts()
    {
        VerificationAttempts++;
    }

    public void MarkVerified()
    {
        IsVerified = true;
    }

    public bool IsExpired()
    {
        return DateTimeOffset.UtcNow > ExpiresAt;
    }
}