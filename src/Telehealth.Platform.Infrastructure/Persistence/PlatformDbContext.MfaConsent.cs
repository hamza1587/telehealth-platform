using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Consent;

namespace Telehealth.Platform.Infrastructure.Persistence;

public partial class PlatformDbContext
{
    public DbSet<ConsentTemplate> ConsentTemplates => Set<ConsentTemplate>();
    public DbSet<PatientConsent> PatientConsents => Set<PatientConsent>();
}