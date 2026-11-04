using HireNow.Application.Accounts;
using HireNow.Infrastructure.Keycloak;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HireNow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HireNowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));
        services.AddHttpClient<IAccountRoleService, KeycloakAccountRoleService>();
        return services;
    }
}
