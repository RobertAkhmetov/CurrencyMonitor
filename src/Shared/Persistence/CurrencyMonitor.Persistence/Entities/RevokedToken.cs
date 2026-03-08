namespace CurrencyMonitor.Persistence.Entities;

public sealed class RevokedToken
{
    public int Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
