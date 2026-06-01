using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Analytics;

public class DashboardMetrics : Entity<Guid>
{
    public string MetricType { get; private set; } = string.Empty;
    public long Value { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public DashboardMetrics() : base(Guid.NewGuid())
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private DashboardMetrics(
        Guid id,
        string metricType,
        long value,
        string unit,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd) : base(id)
    {
        MetricType = metricType;
        Value = value;
        Unit = unit;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static DashboardMetrics Create(
        string metricType,
        long value,
        string unit,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd)
    {
        return new DashboardMetrics(
            Guid.NewGuid(),
            metricType,
            value,
            unit,
            periodStart,
            periodEnd);
    }
}

public static class MetricTypes
{
    public const string TotalPatients = "total_patients";
    public const string TotalDoctors = "total_doctors";
    public const string TotalConsultations = "total_consultations";
    public const string CompletedConsultations = "completed_consultations";
    public const string Revenue = "revenue";
    public const string ActiveUsers = "active_users";
    public const string PendingAppointments = "pending_appointments";
}