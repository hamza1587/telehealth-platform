namespace Telehealth.Platform.Api.PMS;

/// <summary>
/// Temporary in-memory implementation. Replace with EF Core + PostgreSQL in the Infrastructure layer.
/// </summary>
internal sealed class InMemoryPmsRepository : IPmsRepository
{
    private readonly Dictionary<string, List<EncounterDto>> _encounters = [];
    private readonly Dictionary<string, List<ReferralDto>> _referrals = [];

    public Task<DoctorStatsDto> GetDoctorStatsAsync(string doctorId, CancellationToken ct)
    {
        var stats = new DoctorStatsDto(0, 0, 0, 0, 0.0, 0.0);
        return Task.FromResult(stats);
    }

    public Task<IReadOnlyList<EncounterDto>> GetEncountersAsync(string doctorId, int page, int pageSize, CancellationToken ct)
    {
        _encounters.TryGetValue(doctorId, out var list);
        IReadOnlyList<EncounterDto> result = (list ?? [])
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<EncounterDto> CreateEncounterAsync(string doctorId, EncounterRequest req, CancellationToken ct)
    {
        var e = new EncounterDto(
            Guid.NewGuid().ToString(),
            PatientName: req.PatientId, // resolve from patient service in production
            PatientId: req.PatientId,
            Date: DateTimeOffset.UtcNow.ToString("O"),
            Duration: 0,
            ChiefComplaint: req.ChiefComplaint,
            Subjective: req.Subjective,
            Objective: req.Objective,
            Assessment: req.Assessment,
            Plan: req.Plan,
            Status: req.Status);

        if (!_encounters.TryGetValue(doctorId, out var list))
        {
            list = [];
            _encounters[doctorId] = list;
        }
        list.Add(e);
        return Task.FromResult(e);
    }

    public Task<EncounterDto?> GetEncounterAsync(string doctorId, string id, CancellationToken ct)
    {
        _encounters.TryGetValue(doctorId, out var list);
        return Task.FromResult(list?.FirstOrDefault(e => e.Id == id));
    }

    public Task<EncounterDto?> UpdateEncounterAsync(string doctorId, string id, EncounterRequest req, CancellationToken ct)
    {
        if (!_encounters.TryGetValue(doctorId, out var list))
            return Task.FromResult<EncounterDto?>(null);

        var idx = list.FindIndex(e => e.Id == id);
        if (idx < 0) return Task.FromResult<EncounterDto?>(null);

        var updated = list[idx] with
        {
            ChiefComplaint = req.ChiefComplaint,
            Subjective = req.Subjective,
            Objective = req.Objective,
            Assessment = req.Assessment,
            Plan = req.Plan,
            Status = req.Status,
        };
        list[idx] = updated;
        return Task.FromResult<EncounterDto?>(updated);
    }

    public Task<IReadOnlyList<ReferralDto>> GetReferralsAsync(string doctorId, CancellationToken ct)
    {
        _referrals.TryGetValue(doctorId, out var list);
        return Task.FromResult<IReadOnlyList<ReferralDto>>(list ?? []);
    }

    public Task<ReferralDto> CreateReferralAsync(string doctorId, ReferralRequest req, CancellationToken ct)
    {
        var r = new ReferralDto(
            Guid.NewGuid().ToString(),
            PatientName: req.PatientId,
            PatientId: req.PatientId,
            ToSpecialty: req.ToSpecialty,
            ToDoctor: req.ToDoctor,
            Reason: req.Reason,
            Urgency: req.Urgency,
            Status: "pending",
            CreatedAt: DateTimeOffset.UtcNow.ToString("O"));

        if (!_referrals.TryGetValue(doctorId, out var list))
        {
            list = [];
            _referrals[doctorId] = list;
        }
        list.Add(r);
        return Task.FromResult(r);
    }

    public Task<ReferralDto?> GetReferralAsync(string doctorId, string id, CancellationToken ct)
    {
        _referrals.TryGetValue(doctorId, out var list);
        return Task.FromResult(list?.FirstOrDefault(r => r.Id == id));
    }

    public Task<ReferralDto?> UpdateReferralStatusAsync(string doctorId, string id, string status, CancellationToken ct)
    {
        if (!_referrals.TryGetValue(doctorId, out var list))
            return Task.FromResult<ReferralDto?>(null);

        var idx = list.FindIndex(r => r.Id == id);
        if (idx < 0) return Task.FromResult<ReferralDto?>(null);

        var updated = list[idx] with { Status = status };
        list[idx] = updated;
        return Task.FromResult<ReferralDto?>(updated);
    }
}
