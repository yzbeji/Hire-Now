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

app.MapGet("/me/profile", GetCurrentProfile).RequireAuthorization();
app.MapGet("/profiles/{userId}", GetProfileByUserId);
app.MapGet("/categories", async (HireNowDbContext db, CancellationToken cancellationToken) =>
    TypedResults.Ok(await db.Categories
        .Where(c => c.Name != Category.Other.Name)
        .OrderBy(c => c.Name)
        .Select(c => new { c.Id, c.Name })
        .ToListAsync(cancellationToken)));
app.MapPut("/me/profile", UpdateCurrentProfile).RequireAuthorization();
app.MapPost("/me/listings", CreateListing).RequireAuthorization();

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

static async Task<IResult> GetCurrentProfile(ClaimsPrincipal user, HireNowDbContext db)
{
    var userId = user.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
    {
        return TypedResults.Unauthorized();
    }

    var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);
    return TypedResults.Ok(profile is null
        ? new ProfileResponse(userId, string.Empty, string.Empty, string.Empty, string.Empty)
        : new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
}

static async Task<IResult> GetProfileByUserId(string userId, HireNowDbContext db)
{
    var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);
    if (profile is null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
}

static async Task<IResult> UpdateCurrentProfile(UpdateProfileRequest request, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.DisplayName))
    {
        return TypedResults.BadRequest("Display name is required.");
    }

    if ((request.Bio?.Length ?? 0) > 1000)
    {
        return TypedResults.BadRequest("Bio cannot exceed 1000 characters.");
    }

    var userId = user.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
    {
        return TypedResults.BadRequest("Unable to resolve your user id.");
    }

    var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    if (profile is null)
    {
        profile = new Profile
        {
            UserId = userId,
            DisplayName = request.DisplayName.Trim(),
            Bio = request.Bio ?? string.Empty,
            Location = request.Location ?? string.Empty,
            ProfilePhoto = request.ProfilePhoto ?? string.Empty,
        };
        db.Profiles.Add(profile);
    }
    else
    {
        profile.DisplayName = request.DisplayName.Trim();
        profile.Bio = request.Bio ?? string.Empty;
        profile.Location = request.Location ?? string.Empty;
        profile.ProfilePhoto = request.ProfilePhoto ?? string.Empty;
    }

    await db.SaveChangesAsync(cancellationToken);
    return TypedResults.Ok(new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
}

static async Task<IResult> CreateListing(CreateListingRequest request, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
{
    var userId = user.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(CreateListingRequest.Title)] = ["Unable to resolve your user account."],
        });
    }

    var errors = new Dictionary<string, string[]>();
    var title = request.Title?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(title))
    {
        errors[nameof(CreateListingRequest.Title)] = ["Title is required."];
    }
    else if (title.Length > 100)
    {
        errors[nameof(CreateListingRequest.Title)] = ["Title must be 100 characters or fewer."];
    }

    var description = request.Description?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(description))
    {
        errors[nameof(CreateListingRequest.Description)] = ["Description is required."];
    }
    else if (description.Length > 5000)
    {
        errors[nameof(CreateListingRequest.Description)] = ["Description must be 5000 characters or fewer."];
    }

    var categoryName = request.Category?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(categoryName))
    {
        errors[nameof(CreateListingRequest.Category)] = ["Category is required."];
    }
    else
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Name == categoryName, cancellationToken);
        if (category is null)
        {
            errors[nameof(CreateListingRequest.Category)] = ["Choose a valid category."];
        }
    }

    if (request.MinimumPrice <= 0)
    {
        errors[nameof(CreateListingRequest.MinimumPrice)] = ["Minimum price must be greater than 0."];
    }

    if (request.MaximumPrice <= 0)
    {
        errors[nameof(CreateListingRequest.MaximumPrice)] = ["Maximum price must be greater than 0."];
    }

    if (request.MinimumPrice > 0 && request.MaximumPrice > 0 && request.MinimumPrice > request.MaximumPrice)
    {
        errors[nameof(CreateListingRequest.MinimumPrice)] = ["Minimum price can't exceed maximum price."];
    }

    if (request.MinimumPrice > 0 && request.MaximumPrice > 0 && request.MinimumPrice == request.MaximumPrice)
    {
        errors[nameof(CreateListingRequest.MaximumPrice)] = ["Minimum and maximum price must not be the same."];
    }

    if (errors.Count > 0)
    {
        return TypedResults.ValidationProblem(errors);
    }

    var categoryEntity = await db.Categories.SingleAsync(c => c.Name == categoryName, cancellationToken);
    var listing = new Listing
    {
        Title = title,
        Description = description,
        Category = categoryEntity,
        MinimumPrice = request.MinimumPrice,
        MaximumPrice = request.MaximumPrice,
        Status = Enum.TryParse<ListingStatus>(request.Status, true, out var status) ? status : ListingStatus.Draft,
        UserId = userId,
    };

    db.Listings.Add(listing);
    await db.SaveChangesAsync(cancellationToken);

    return TypedResults.Created($"/me/listings/{listing.Id}", new ListingResponse(
        listing.Id,
        listing.Title,
        listing.Description,
        listing.Category.Name,
        listing.MinimumPrice,
        listing.MaximumPrice,
        listing.Status.ToString()));
}

internal sealed record ChooseRoleRequest(string Role);
internal sealed record UpdateProfileRequest(string DisplayName, string? Bio, string? Location, string? ProfilePhoto);
internal sealed record ProfileResponse(string UserId, string DisplayName, string Bio, string Location, string ProfilePhoto);
internal sealed record CreateListingRequest(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string? Status);
internal sealed record ListingResponse(int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string Status);
