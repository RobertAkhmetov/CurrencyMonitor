using CurrencyMonitor.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory
});

var services = new ServiceCollection();
services.AddCurrencyMonitorPersistence(builder.Configuration);
await using var serviceProvider = services.BuildServiceProvider();

await using var scope = serviceProvider.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyMonitorDbContext>();
await dbContext.Database.MigrateAsync();

Console.WriteLine("Database migration completed.");
