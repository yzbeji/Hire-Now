using System.Security.Claims;
using HireNow.Application.Accounts;
using HireNow.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HireNow.Api.Endpoints;

internal static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", (ClaimsPrincipal user) => TypedResults.Ok(new
        {
            Id = user.FindFirstValue("sub"),
            Username = user.Identity?.Name,
            // Keycloak also emits its own realm roles, such as offline_access.
            Roles = user.FindAll("roles").Select(claim => claim.Value).Where(role => AccountRoles.All.Contains(role)),
        })).RequireAuthorization();

        app.MapPost("/me/role", async Task<Results<NoContent, BadRequest, Conflict>> (
            ChooseRoleRequest request,
            ClaimsPrincipal user,
            IAccountRoleService accountRoles,
            CancellationToken cancellationToken) =>
        {
            if (!AccountRoles.SelfService.Contains(request.Role))
            {
                return TypedResults.BadRequest();
            }

            var assigned = await accountRoles.TryAssignInitialRoleAsync(user.FindFirstValue("sub")!, request.Role, cancellationToken);
            return assigned ? TypedResults.NoContent() : TypedResults.Conflict();
        }).RequireAuthorization();
    }
}

internal sealed record ChooseRoleRequest(string Role);
