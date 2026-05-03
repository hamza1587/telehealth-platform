using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Analytics;

public class ConsultationAnalytics : Entity<Guid>
{
    public DateTimeOffset Date { get; private set; }
    public string ConsultationMode { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int Count { get; private set; }
    public int CompletedCount { get; private set; }
    public int CancelledCount { get; private set; }
    public int NoShowCount { get; private set; }
    public long TotalBillableSeconds { get; private set; }
    public long TotalRevenueMinor { get; private set; }
    public double AverageDurationSeconds { get; private set; }
    public Guid? DoctorProfileId { get; private set; }
    public Guid? PatientAccountId { get; private set; }

    private ConsultationAnalytics(DateTimeOffset date) : base(Guid.NewGuid())
    {
        Date = date;
    }

    public static ConsultationAnalytics Create(
        DateTimeOffset date,
        string consultationMode,
        string status)
    {
        return new ConsultationAnalytics(date)
        {
            ConsultationMode = consultationMode,
            Status = status
        };
    }

    public void IncrementCount()
    {
        Count++;
    }

    public void AddBillableSeconds(long seconds)
    {
        TotalBillableSeconds += seconds;
    }

    public void AddRevenue(long amountMinor)
    {
        TotalRevenueMinor += amountMinor;
    }

    public void SetDoctorProfile(Guid? doctorProfileId)
    {
        DoctorProfileId = doctorProfileId;
    }

    public void SetPatientAccount(Guid? patientAccountId)
    {
        PatientAccountId = patientAccountId;
    }

    public void CalculateAverageDuration(long totalSeconds, int totalCount)
    {
        if (totalCount > 0)
        {
            AverageDurationSeconds = (double)totalSeconds / totalCount;
        }
    }
}