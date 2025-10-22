// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabeller
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.id);     // du använder 'id' (litet i)
                entity.HasIndex(p => p.Title);

                // (valfritt) rimliga constraints/typer
                entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("numeric(18,2)");
            });
        }
    }
}
