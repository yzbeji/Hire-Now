using System.Security.Claims;
using HireNow.Application.Profiles;

namespace HireNow.Api.Endpoints;

internal static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/profile", GetCurrentProfile).RequireAuthorization();
        app.MapGet("/profiles/{userId}", GetProfileByUserId);
        app.MapPut("/me/profile", UpdateCurrentProfile).RequireAuthorization();
    }

    private static async Task<IResult> GetCurrentProfile(ClaimsPrincipal user, ProfileService profiles, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return TypedResults.Unauthorized();
        }

        var profile = await profiles.FindAsync(userId, cancellationToken);
        return TypedResults.Ok(profile is null
            ? new ProfileResponse(userId, string.Empty, string.Empty, string.Empty, string.Empty)
            : new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
    }

    private static async Task<IResult> GetProfileByUserId(string userId, ProfileService profiles, CancellationToken cancellationToken)
    {
        var profile = await profiles.FindAsync(userId, cancellationToken);
        if (profile is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
    }

    private static async Task<IResult> UpdateCurrentProfile(UpdateProfileRequest request, ClaimsPrincipal user, ProfileService profiles, CancellationToken cancellationToken)
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

        var profile = await profiles.UpdateCurrentAsync(userId, request.DisplayName, request.Bio, request.Location,
                                                        request.ProfilePhoto, cancellationToken);

        return TypedResults.Ok(new ProfileResponse(profile.UserId, profile.DisplayName, profile.Bio, profile.Location, profile.ProfilePhoto));
    }
}

internal sealed record UpdateProfileRequest(string DisplayName, string? Bio, string? Location, string? ProfilePhoto);
internal sealed record ProfileResponse(string UserId, string DisplayName, string Bio, string Location, string ProfilePhoto);
