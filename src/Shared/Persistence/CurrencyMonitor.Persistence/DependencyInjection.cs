using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyMonitor.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrencyMonitorPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                               ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<CurrencyMonitorDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
