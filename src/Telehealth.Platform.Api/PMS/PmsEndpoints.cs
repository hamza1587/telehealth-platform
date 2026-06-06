using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Telehealth.Platform.Api.PMS;

/// <summary>
/// Practice Management System endpoints — encounters, referrals, stats.
/// All routes require an authenticated doctor principal.
/// </summary>
[ApiController]
[Route("api/v1/pms")]
[Authorize(Roles = "Doctor")]
public class PmsEndpoints(IPmsRepository pms) : ControllerBase
{
    private string DoctorId =>
        User.FindFirst("doctor_id")?.Value
        ?? throw new UnauthorizedAccessException("doctor_id claim missing.");

    // ─── Stats ────────────────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct) =>
        Ok(await pms.GetDoctorStatsAsync(DoctorId, ct));

    // ─── Encounters ───────────────────────────────────────────────────────────

    [HttpGet("encounters")]
    public async Task<IActionResult> GetEncounters(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return Ok(await pms.GetEncountersAsync(DoctorId, page, pageSize, ct));
    }

    [HttpPost("encounters")]
    public async Task<IActionResult> CreateEncounter([FromBody] EncounterRequest req, CancellationToken ct)
    {
        var encounter = await pms.CreateEncounterAsync(DoctorId, req, ct);
        return CreatedAtAction(nameof(GetEncounter), new { id = encounter.Id }, encounter);
    }

    [HttpGet("encounters/{id}")]
    public async Task<IActionResult> GetEncounter(string id, CancellationToken ct)
    {
        var e = await pms.GetEncounterAsync(DoctorId, id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPut("encounters/{id}")]
    public async Task<IActionResult> UpdateEncounter(string id, [FromBody] EncounterRequest req, CancellationToken ct)
    {
        var e = await pms.UpdateEncounterAsync(DoctorId, id, req, ct);
        return e is null ? NotFound() : Ok(e);
    }

    // ─── Referrals ────────────────────────────────────────────────────────────

    [HttpGet("referrals")]
    public async Task<IActionResult> GetReferrals(CancellationToken ct) =>
        Ok(await pms.GetReferralsAsync(DoctorId, ct));

    [HttpPost("referrals")]
    public async Task<IActionResult> CreateReferral([FromBody] ReferralRequest req, CancellationToken ct)
    {
        var referral = await pms.CreateReferralAsync(DoctorId, req, ct);
        return CreatedAtAction(nameof(GetReferral), new { id = referral.Id }, referral);
    }

    [HttpGet("referrals/{id}")]
    public async Task<IActionResult> GetReferral(string id, CancellationToken ct)
    {
        var r = await pms.GetReferralAsync(DoctorId, id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPatch("referrals/{id}/status")]
    public async Task<IActionResult> UpdateReferralStatus(
        string id,
        [FromBody] UpdateReferralStatusRequest req,
        CancellationToken ct)
    {
        var r = await pms.UpdateReferralStatusAsync(DoctorId, id, req.Status, ct);
        return r is null ? NotFound() : Ok(r);
    }
}

// ─── Request / response models ────────────────────────────────────────────────

public record EncounterRequest(
    string PatientId,
    string? ChiefComplaint,
    string? Subjective,
    string? Objective,
    string? Assessment,
    string? Plan,
    string Status = "draft");

public record ReferralRequest(
    string PatientId,
    string ToSpecialty,
    string? ToDoctor,
    string Reason,
    string Urgency = "routine");

public record UpdateReferralStatusRequest(string Status);

// ─── Repository abstraction ───────────────────────────────────────────────────

public interface IPmsRepository
{
    Task<DoctorStatsDto> GetDoctorStatsAsync(string doctorId, CancellationToken ct);

    Task<IReadOnlyList<EncounterDto>> GetEncountersAsync(string doctorId, int page, int pageSize, CancellationToken ct);
    Task<EncounterDto> CreateEncounterAsync(string doctorId, EncounterRequest req, CancellationToken ct);
    Task<EncounterDto?> GetEncounterAsync(string doctorId, string id, CancellationToken ct);
    Task<EncounterDto?> UpdateEncounterAsync(string doctorId, string id, EncounterRequest req, CancellationToken ct);

    Task<IReadOnlyList<ReferralDto>> GetReferralsAsync(string doctorId, CancellationToken ct);
    Task<ReferralDto> CreateReferralAsync(string doctorId, ReferralRequest req, CancellationToken ct);
    Task<ReferralDto?> GetReferralAsync(string doctorId, string id, CancellationToken ct);
    Task<ReferralDto?> UpdateReferralStatusAsync(string doctorId, string id, string status, CancellationToken ct);
}

public record DoctorStatsDto(
    int TodayAppointments,
    int WeekAppointments,
    int TotalPatients,
    int AvgConsultMinutes,
    double CompletionRate,
    double AvgRating);

public record EncounterDto(
    string Id,
    string PatientName,
    string PatientId,
    string Date,
    int Duration,
    string? ChiefComplaint,
    string? Subjective,
    string? Objective,
    string? Assessment,
    string? Plan,
    string Status);

public record ReferralDto(
    string Id,
    string PatientName,
    string PatientId,
    string ToSpecialty,
    string? ToDoctor,
    string Reason,
    string Urgency,
    string Status,
    string CreatedAt);
