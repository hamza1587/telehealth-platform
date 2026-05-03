using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Clinical;
using Telehealth.Platform.Domain.Clinical;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Clinical;

public class ClinicalRecordService : IClinicalRecordService
{
    private readonly PlatformDbContext _dbContext;

    public ClinicalRecordService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClinicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClinicalRecords
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ClinicalRecord>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClinicalRecords
            .Where(r => r.PatientAccountId == patientId)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicalRecord> CreateAsync(
        Guid patientId,
        string recordType,
        string title,
        string content,
        string recordedBy,
        string? medplumResourceId = null,
        CancellationToken cancellationToken = default)
    {
        var record = ClinicalRecord.Create(patientId, recordType, title, content, recordedBy, medplumResourceId);
        await _dbContext.ClinicalRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task UpdateContentAsync(Guid id, string content, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.ClinicalRecords.FindAsync([id], cancellationToken);
        if (record != null)
        {
            record.UpdateContent(content);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetStatusAsync(Guid id, ClinicalRecordStatus status, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.ClinicalRecords.FindAsync([id], cancellationToken);
        if (record != null)
        {
            record.SetStatus(status);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.ClinicalRecords.FindAsync([id], cancellationToken);
        if (record != null)
        {
            record.SetStatus(ClinicalRecordStatus.Deleted);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}