using HireNow.Application.Accounts;
using HireNow.Application.Categories;
using HireNow.Application.Listings;
using HireNow.Application.Chat;
using HireNow.Application.Profiles;
using HireNow.Application.PublicServices;
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
        services.AddScoped<IListingOrders, ListingOrders>();
        services.AddScoped<IListingData, ListingData>();
        services.AddScoped<IProfileData, ProfileData>();
        services.AddScoped<ICategoryData, CategoryData>();
        services.AddScoped<IPublicServiceData, PublicServiceData>();
        services.AddScoped<IChatData, ChatData>();
        return services;
    }
}
