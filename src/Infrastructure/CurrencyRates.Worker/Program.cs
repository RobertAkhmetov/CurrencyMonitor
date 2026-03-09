using CurrencyMonitor.Persistence;
using CurrencyRates.Worker;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCurrencyMonitorPersistence(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
