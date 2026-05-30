using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Doctors;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Doctors;

public class DoctorProfileService : IDoctorProfileService
{
	private readonly PlatformDbContext _dbContext;

	public DoctorProfileService(PlatformDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<IEnumerable<DoctorProfile>> SearchDoctorsAsync(string? specialty, string? query, CancellationToken cancellationToken = default)
	{
		var doctors = _dbContext.DoctorProfiles.AsQueryable();

		if (!string.IsNullOrWhiteSpace(specialty))
		{
			doctors = doctors.Where(d => d.Specialty != null && d.Specialty.Contains(specialty));
		}

		if (!string.IsNullOrWhiteSpace(query))
		{
			doctors = doctors.Where(d => (d.FullName != null && d.FullName.Contains(query)) || (d.Specialty != null && d.Specialty.Contains(query)));
		}

		return await doctors.ToListAsync(cancellationToken);
	}

	public async Task<DoctorProfile?> GetByIdAsync(Guid doctorProfileId, CancellationToken cancellationToken = default)
	{
		return await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == doctorProfileId, cancellationToken);
	}

	public async Task<IEnumerable<DoctorAvailabilityWindow>> GetAvailabilityAsync(Guid doctorProfileId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
	{
		return await _dbContext.DoctorAvailabilityWindows
			.Where(w => w.DoctorProfileId == doctorProfileId && w.StartsAt >= from && w.EndsAt <= to)
			.ToListAsync(cancellationToken);
	}
}
