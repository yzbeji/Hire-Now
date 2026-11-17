using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireNow.Infrastructure.Persistence;

public static class DatabaseInitialization
{
    public static void MigrateDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<HireNowDbContext>().Database.Migrate();
    }
}
