using System.Net.Http.Headers;
using HireNow.Application.Listings;
using HireNow.Domain;
using HireNow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HireNow.Tests;

/// <summary>The real API with its real JWT setup, a database private to one test, and orders faked.</summary>
public sealed class TestApi : IDisposable
{
    private const string Issuer = "https://keycloak.test/realms/hirenow";
    private static readonly SymmetricSecurityKey SigningKey = new(Enumerable.Range(1, 32).Select(i => (byte)i).ToArray());

    private readonly WebApplicationFactory<Program> _factory;

    public FakeListingOrders Orders { get; } = new();

    public TestApi()
    {
        var databaseName = Guid.NewGuid().ToString();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Keycloak:Authority", Issuer);
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IListingOrders>(Orders);

                // Replace SQL Server with a database private to this test.
                foreach (var descriptor in services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<HireNowDbContext>) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<HireNowDbContext>)).ToList())
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<HireNowDbContext>(options => options.UseInMemoryDatabase(databaseName));

                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                    configuration.SigningKeys.Add(SigningKey);
                    options.Configuration = configuration;
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<HireNowDbContext>().Database.EnsureCreated();
    }

    public HttpClient Anonymous() => _factory.CreateClient();

    public HttpClient ClientFor(string userId, string role = AccountRoles.Seller)
    {
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = "hirenow-api",
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object> { ["sub"] = userId, ["roles"] = new[] { role } },
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public void Dispose() => _factory.Dispose();

    public sealed class FakeListingOrders : IListingOrders
    {
        public HashSet<int> ListingsWithActiveOrders { get; } = [];

        public Task<bool> HasActiveOrdersAsync(int listingId, CancellationToken cancellationToken) =>
            Task.FromResult(ListingsWithActiveOrders.Contains(listingId));
    }
}
