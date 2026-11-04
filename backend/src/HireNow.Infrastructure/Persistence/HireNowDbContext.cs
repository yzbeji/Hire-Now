using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

public class HireNowDbContext(DbContextOptions<HireNowDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var category = modelBuilder.Entity<Category>();
        category.Property(c => c.Name).HasMaxLength(100);
        category.HasIndex(c => c.Name).IsUnique();
        category.HasData(
            new Category { Id = 1, Name = "Programming" },
            new Category { Id = 2, Name = "Graphic Design" },
            new Category { Id = 3, Name = "Consulting" },
            new Category { Id = 4, Name = "Tutoring" },
            new Category { Id = 5, Name = "Photography" },
            new Category { Id = 6, Name = "Home Repairs" });
    }
}
