using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HireNow.Application.Accounts;
using HireNow.Domain;
using Microsoft.Extensions.Configuration;

namespace HireNow.Infrastructure.Keycloak;

public class KeycloakAccountRoleService(HttpClient http, IConfiguration configuration) : IAccountRoleService
{
    private const string ClientId = "hirenow-api";

    private readonly string _authority = configuration["Keycloak:Authority"]
        ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");
    private readonly string _clientSecret = configuration["Keycloak:ApiClientSecret"]
        ?? throw new InvalidOperationException("Keycloak:ApiClientSecret is not configured.");

    public async Task<bool> TryAssignInitialRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(cancellationToken));

        var adminUrl = _authority.Replace("/realms/", "/admin/realms/");
        var mappingsUrl = $"{adminUrl}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm";

        // Checked against Keycloak, not the caller's token, which can be out of date.
        var currentRoles = await http.GetFromJsonAsync<KeycloakRole[]>(mappingsUrl, cancellationToken);
        if (currentRoles!.Any(current => AccountRoles.All.Contains(current.Name)))
        {
            return false;
        }

        var keycloakRole = await http.GetFromJsonAsync<KeycloakRole>($"{adminUrl}/roles/{Uri.EscapeDataString(role)}", cancellationToken);
        var response = await http.PostAsJsonAsync(mappingsUrl, new[] { keycloakRole }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return true;
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var response = await http.PostAsync(
            $"{_authority}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ClientId,
                ["client_secret"] = _clientSecret,
            }),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        return token!.AccessToken;
    }

    private sealed record KeycloakRole(string Id, string Name);

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
