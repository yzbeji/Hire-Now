using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

public class HireNowDbContext(DbContextOptions<HireNowDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Listing> Listings => Set<Listing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var category = modelBuilder.Entity<Category>();
        category.Property(c => c.Name).HasMaxLength(100);
        category.HasIndex(c => c.Name).IsUnique();
        category.HasData(
            new Category { Id = 1, Name = "Other" },
            new Category { Id = 2, Name = "Programming" },
            new Category { Id = 3, Name = "Graphic Design" },
            new Category { Id = 4, Name = "Consulting" },
            new Category { Id = 5, Name = "Tutoring" },
            new Category { Id = 6, Name = "Photography" },
            new Category { Id = 7, Name = "Home Repairs" });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.Property(p => p.DisplayName).HasMaxLength(200);
            entity.Property(p => p.Bio).HasMaxLength(1000);
            entity.Property(p => p.Location).HasMaxLength(200);
            entity.Property(p => p.ProfilePhoto).HasMaxLength(2048);
            entity.Property(p => p.UserId).HasColumnName("UserId");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.Property(l => l.Title).HasMaxLength(100);
            entity.Property(l => l.Description).HasMaxLength(5000);
            entity.Property(l => l.UserId).IsRequired();
        });
    }
}
