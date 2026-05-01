using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Telehealth.Platform.Infrastructure.Persistence;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlatformDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=telehealth_platform;Username=telehealth;Password=telehealth");

        return new PlatformDbContext(optionsBuilder.Options);
    }
}
