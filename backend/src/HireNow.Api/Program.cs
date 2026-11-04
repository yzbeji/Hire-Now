using System.Security.Claims;
using HireNow.Application.Accounts;
using HireNow.Domain;
using HireNow.Infrastructure;
using HireNow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
    ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.Audience = "hirenow-api";
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "roles";
});

var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var role in AccountRoles.All)
{
    authorization.AddPolicy(role, policy => policy.RequireRole(role));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "HireNow API"));

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<HireNowDbContext>().Database.Migrate();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { Status = "Healthy" }));

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

app.Run();

internal sealed record ChooseRoleRequest(string Role);
