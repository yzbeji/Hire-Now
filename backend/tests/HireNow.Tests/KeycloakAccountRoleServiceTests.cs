using System.Net;
using System.Net.Http.Json;
using HireNow.Infrastructure.Keycloak;
using Microsoft.Extensions.Configuration;

namespace HireNow.Tests;

public class KeycloakAccountRoleServiceTests
{
    private const string MappingsPath = "/admin/realms/hirenow/users/user-1/role-mappings/realm";

    private sealed class FakeKeycloak(params string[] currentRoles) : HttpMessageHandler
    {
        public List<string> AssignedBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/protocol/openid-connect/token"))
            {
                return Json(new { access_token = "service-token" });
            }

            Assert.Equal("service-token", request.Headers.Authorization?.Parameter);
            if (request.Method == HttpMethod.Post && path == MappingsPath)
            {
                AssignedBodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return path == MappingsPath
                ? Json(currentRoles.Select(role => new { id = $"id-{role}", name = role }))
                : Json(new { id = "id-Seller", name = "Seller" });
        }

        private static HttpResponseMessage Json(object body) => new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };
    }

    private static KeycloakAccountRoleService CreateService(FakeKeycloak keycloak) => new(
        new HttpClient(keycloak),
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Keycloak:Authority"] = "http://keycloak.test/realms/hirenow",
            ["Keycloak:ApiClientSecret"] = "secret",
        }).Build());

    [Fact]
    public async Task Assigns_the_role_to_a_user_without_one()
    {
        var keycloak = new FakeKeycloak("default-roles-hirenow", "offline_access");

        var assigned = await CreateService(keycloak).TryAssignInitialRoleAsync("user-1", "Seller", CancellationToken.None);

        Assert.True(assigned);
        Assert.Contains("\"name\":\"Seller\"", Assert.Single(keycloak.AssignedBodies));
    }

    [Theory]
    [InlineData("Seller")]
    [InlineData("Buyer")]
    [InlineData("Admin")]
    public async Task Refuses_when_the_user_already_has_an_application_role(string existingRole)
    {
        var keycloak = new FakeKeycloak("default-roles-hirenow", existingRole);

        var assigned = await CreateService(keycloak).TryAssignInitialRoleAsync("user-1", "Seller", CancellationToken.None);

        Assert.False(assigned);
        Assert.Empty(keycloak.AssignedBodies);
    }
}
