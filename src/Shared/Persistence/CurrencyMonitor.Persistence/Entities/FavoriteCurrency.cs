namespace CurrencyMonitor.Persistence.Entities;

public sealed class FavoriteCurrency
{
    public int UserId { get; set; }
    public int CurrencyId { get; set; }
    public AppUser User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
