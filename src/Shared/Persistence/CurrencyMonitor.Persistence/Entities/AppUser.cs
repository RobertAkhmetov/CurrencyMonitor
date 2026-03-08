namespace CurrencyMonitor.Persistence.Entities;

public sealed class AppUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ICollection<FavoriteCurrency> FavoriteCurrencies { get; set; } = new List<FavoriteCurrency>();
}
