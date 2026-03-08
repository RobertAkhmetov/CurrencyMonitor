using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using CurrencyMonitor.Persistence;
using CurrencyMonitor.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRates.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory serviceScopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
{
    private const string SourceUrl = "http://www.cbr.ru/scripts/XML_daily.asp";
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(configuration.GetValue("Worker:UpdateIntervalMinutes", 30));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateRates(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update currency rates.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task UpdateRates(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, SourceUrl);
        request.Headers.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var xdoc = XDocument.Parse(content);

        var rates = xdoc.Root?
            .Elements("Valute")
            .Select(x =>
            {
                var charCode = x.Element("CharCode")?.Value?.Trim();
                var valueText = x.Element("Value")?.Value?.Trim();
                var nominalText = x.Element("Nominal")?.Value?.Trim();
                if (string.IsNullOrWhiteSpace(charCode) ||
                    string.IsNullOrWhiteSpace(valueText) ||
                    string.IsNullOrWhiteSpace(nominalText))
                {
                    return null;
                }

                var nominal = decimal.Parse(nominalText.Replace(',', '.'), CultureInfo.InvariantCulture);
                var value = decimal.Parse(valueText.Replace(',', '.'), CultureInfo.InvariantCulture);
                var rate = decimal.Round(value / nominal, 6, MidpointRounding.AwayFromZero);
                return new Currency { Name = charCode.ToUpperInvariant(), Rate = rate };
            })
            .Where(x => x is not null)
            .Cast<Currency>()
            .ToList() ?? [];

        rates.Add(new Currency { Name = "RUB", Rate = 1m });

        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyMonitorDbContext>();

        var existing = await dbContext.Currencies.ToListAsync(cancellationToken);
        var map = existing.ToDictionary(x => x.Name, x => x);

        foreach (var rate in rates)
        {
            if (map.TryGetValue(rate.Name, out var current))
            {
                current.Rate = rate.Rate;
            }
            else
            {
                dbContext.Currencies.Add(rate);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Currency table updated: {Count}", rates.Count);
    }
}
