namespace CurrencyMonitor.Persistence.Entities;

public sealed class Currency
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public ICollection<FavoriteCurrency> FavoriteCurrencies { get; set; } = new List<FavoriteCurrency>();
}
