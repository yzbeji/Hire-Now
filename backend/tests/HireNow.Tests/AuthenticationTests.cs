using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HireNow.Tests;

public class AuthenticationTests : IDisposable
{
    private const string Issuer = "https://keycloak.test/realms/hirenow";
    private static readonly SymmetricSecurityKey SigningKey = new(Enumerable.Range(1, 32).Select(i => (byte)i).ToArray());

    // Keeps the app's real JWT options, and only swaps Keycloak's discovery document for a local key.
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Keycloak:Authority", Issuer);
        builder.ConfigureTestServices(services => services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
            configuration.SigningKeys.Add(SigningKey);
            options.Configuration = configuration;
        }));
    });

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string CreateToken(
        string audience = "hirenow-api",
        string issuer = Issuer,
        SecurityKey? key = null,
        DateTime? expires = null,
        string[]? roles = null) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            NotBefore = DateTime.UtcNow.AddHours(-2),
            Expires = expires ?? DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(key ?? SigningKey, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["preferred_username"] = "sam",
                ["roles"] = roles ?? [],
            },
        });

    private Task<HttpResponseMessage> GetMe(string? token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);
        return client.GetAsync("/me");
    }

    [Fact]
    public async Task Me_returns_the_user_from_a_valid_token()
    {
        var response = await GetMe(CreateToken(roles: ["Seller", "Buyer"]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal("user-1", me!.Id);
        Assert.Equal("sam", me.Username);
        Assert.Equal(["Seller", "Buyer"], me.Roles);
    }

    [Fact]
    public async Task Me_returns_only_application_roles()
    {
        var response = await GetMe(CreateToken(roles: ["Buyer", "default-roles-hirenow", "offline_access"]));

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal(["Buyer"], me!.Roles);
    }

    [Fact]
    public void Startup_fails_without_a_keycloak_authority()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Keycloak:Authority", error.Message);
    }

    [Fact]
    public async Task Me_rejects_a_missing_token()
    {
        var response = await GetMe(null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_rejects_a_token_for_another_audience()
    {
        var response = await GetMe(CreateToken(audience: "account"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_rejects_a_token_from_another_issuer()
    {
        var response = await GetMe(CreateToken(issuer: "https://evil.test/realms/hirenow"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_rejects_a_token_signed_with_another_key()
    {
        var response = await GetMe(CreateToken(key: new SymmetricSecurityKey(new byte[32])));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_rejects_an_expired_token()
    {
        var response = await GetMe(CreateToken(expires: DateTime.UtcNow.AddHours(-1)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Seller", "Seller", true)]
    [InlineData("Buyer", "Buyer", true)]
    [InlineData("Admin", "Admin", true)]
    [InlineData("Seller", "Buyer", false)]
    [InlineData("Admin", "Seller", false)]
    public async Task Policies_require_the_matching_role(string policy, string role, bool expected)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("roles", role)], "Test", "preferred_username", "roles"));
        var authorization = _factory.Services.GetRequiredService<IAuthorizationService>();

        var result = await authorization.AuthorizeAsync(user, policy);

        Assert.Equal(expected, result.Succeeded);
    }

    private sealed record MeResponse(string Id, string Username, string[] Roles);
}
