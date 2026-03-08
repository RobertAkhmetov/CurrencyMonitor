using CurrencyMonitor.Persistence;
using FinanceService.Application.Abstractions;
using FinanceService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCurrencyMonitorPersistence(configuration);
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        return services;
    }
}
