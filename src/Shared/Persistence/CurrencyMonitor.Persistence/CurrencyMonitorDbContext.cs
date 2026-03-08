using CurrencyMonitor.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyMonitor.Persistence;

public sealed class CurrencyMonitorDbContext(DbContextOptions<CurrencyMonitorDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<FavoriteCurrency> FavoriteCurrencies => Set<FavoriteCurrency>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Password).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currency");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Rate).HasPrecision(10, 4).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<FavoriteCurrency>(entity =>
        {
            entity.ToTable("favorites");
            entity.HasKey(x => new { x.UserId, x.CurrencyId });
            entity.HasOne(x => x.User)
                .WithMany(x => x.FavoriteCurrencies)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Currency)
                .WithMany(x => x.FavoriteCurrencies)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.ToTable("revoked_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Jti).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.Jti).IsUnique();
        });
    }
}
