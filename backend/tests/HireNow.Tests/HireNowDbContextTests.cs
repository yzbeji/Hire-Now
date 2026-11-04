using HireNow.Domain;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HireNow.Tests;

public class HireNowDbContextTests
{
    // The model can be inspected without ever opening this connection.
    private static HireNowDbContext CreateContext() => new(
        new DbContextOptionsBuilder<HireNowDbContext>().UseSqlServer("Server=unused").Options);

    [Fact]
    public void Service_categories_are_seeded()
    {
        using var context = CreateContext();

        var seeded = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(Category))!
            .GetSeedData()
            .Select(row => (string)row[nameof(Category.Name)]!);

        Assert.Equal(
            ["Programming", "Graphic Design", "Consulting", "Tutoring", "Photography", "Home Repairs"],
            seeded);
    }

    [Fact]
    public void Migrations_match_the_model()
    {
        using var context = CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
