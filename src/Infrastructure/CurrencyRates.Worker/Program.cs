using CurrencyRates.Worker;
using CurrencyMonitor.Persistence;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCurrencyMonitorPersistence(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
