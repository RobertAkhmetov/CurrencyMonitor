using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CurrencyMonitor.Persistence;

public sealed class CurrencyMonitorDbContextFactory : IDesignTimeDbContextFactory<CurrencyMonitorDbContext>
{
    public CurrencyMonitorDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");

        var optionsBuilder = new DbContextOptionsBuilder<CurrencyMonitorDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new CurrencyMonitorDbContext(optionsBuilder.Options);
    }
}
