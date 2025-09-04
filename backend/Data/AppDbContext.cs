using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Listing> Listings => Set<Listing>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Pris som numeric(18,2) i Postgres
        b.Entity<Listing>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // Vanliga index för sök/sortering
        b.Entity<Listing>().HasIndex(p => p.City);
        b.Entity<Listing>().HasIndex(p => new { p.City, p.Price });
        b.Entity<Listing>().HasIndex(p => p.ListedAtUtc);
   }
}