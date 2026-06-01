namespace Telehealth.Platform.Domain.Entities;

public class LabResult
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid OrderingProviderId { get; private set; }
    public string LabSystemId { get; private set; }
    public string TestCode { get; private set; }
    public string TestName { get; private set; }
    public string Value { get; private set; }
    public string Unit { get; private set; }
    public string ReferenceRange { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset SampleDate { get; private set; }
    public DateTimeOffset ResultDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsAbnormal { get; private set; }

    public LabResult(
        Guid patientId,
        Guid orderingProviderId,
        string labSystemId,
        string testCode,
        string testName,
        string value,
        string unit,
        string referenceRange,
        DateTimeOffset sampleDate)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        OrderingProviderId = orderingProviderId;
        LabSystemId = labSystemId;
        TestCode = testCode;
        TestName = testName;
        Value = value;
        Unit = unit;
        ReferenceRange = referenceRange;
        Status = "Final";
        SampleDate = sampleDate;
        ResultDate = DateTimeOffset.UtcNow;
        IsAbnormal = false;
    }

    public void MarkAsAbnormal()
    {
        IsAbnormal = true;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }
}

public class InsuranceClaim
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ConsultationId { get; private set; }
    public string InsuranceProviderId { get; private set; }
    public string PolicyNumber { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string ServiceType { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset SubmittedDate { get; private set; }
    public DateTimeOffset? ProcessedDate { get; private set; }
    public string? ClaimNumber { get; private set; }
    public string? RejectionReason { get; private set; }

    public InsuranceClaim(
        Guid patientId,
        Guid consultationId,
        string insuranceProviderId,
        string policyNumber,
        decimal amount,
        string currency,
        string serviceType)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        ConsultationId = consultationId;
        InsuranceProviderId = insuranceProviderId;
        PolicyNumber = policyNumber;
        Amount = amount;
        Currency = currency;
        ServiceType = serviceType;
        Status = "Pending";
        SubmittedDate = DateTimeOffset.UtcNow;
    }

    public void Approve(string claimNumber)
    {
        Status = "Approved";
        ClaimNumber = claimNumber;
        ProcessedDate = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = "Rejected";
        RejectionReason = reason;
        ProcessedDate = DateTimeOffset.UtcNow;
    }
}
