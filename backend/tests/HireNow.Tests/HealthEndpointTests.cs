using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HireNow.Tests;

public class HealthEndpointTests
{
    [Fact]
    public async Task Health_returns_200()
    {
        // Not "Development", so startup doesn't try to migrate a real database.
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Testing")
                .UseSetting("Keycloak:Authority", "https://keycloak.test/realms/hirenow"));

        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
