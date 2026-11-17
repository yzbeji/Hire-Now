using System.Security.Claims;
using HireNow.Application.Listings;
using HireNow.Domain;

namespace HireNow.Api.Endpoints;

internal static class ListingEndpoints
{
    public static void MapListingEndpoints(this IEndpointRouteBuilder app)
    {
        var listings = app.MapGroup("/me/listings").RequireAuthorization(AccountRoles.Seller);

        listings.MapGet("/", GetMyListings);
        listings.MapGet("/{id:int}", GetMyListing);
        listings.MapPost("/", CreateListing);
        listings.MapPut("/{id:int}", UpdateListing);
        listings.MapPost("/{id:int}/publish", PublishListing);
        listings.MapPost("/{id:int}/unpublish", UnpublishListing);
        listings.MapDelete("/{id:int}", DeleteListing);
    }

    private static async Task<IResult> GetMyListings(ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken) =>
        TypedResults.Ok((await listings.GetMineAsync(user.FindFirstValue("sub"), cancellationToken)).Select(ToResponse));

    private static async Task<IResult> GetMyListing(int id, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken) =>
        ToResult(await listings.GetOwnedAsync(id, user.FindFirstValue("sub")!, cancellationToken));

    private static async Task<IResult> CreateListing(CreateListingRequest request, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken)
    {
        var outcome = await listings.CreateAsync(ToInput(request), user.FindFirstValue("sub")!, cancellationToken);
        return outcome.Kind == ListingOutcomeKind.Success
            ? TypedResults.Created($"/me/listings/{outcome.Listing!.Id}", ToResponse(outcome.Listing))
            : ToResult(outcome);
    }

    private static async Task<IResult> UpdateListing(int id, UpdateListingRequest request, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken) =>
        ToResult(await listings.UpdateAsync(id, ToInput(request), user.FindFirstValue("sub")!, cancellationToken));

    private static async Task<IResult> PublishListing(int id, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken) =>
        ToResult(await listings.PublishAsync(id, user.FindFirstValue("sub")!, cancellationToken));

    private static async Task<IResult> UnpublishListing(int id, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken) =>
        ToResult(await listings.UnpublishAsync(id, user.FindFirstValue("sub")!, cancellationToken));

    private static async Task<IResult> DeleteListing(int id, ClaimsPrincipal user, ListingService listings, CancellationToken cancellationToken)
    {
        var outcome = await listings.DeleteAsync(id, user.FindFirstValue("sub")!, cancellationToken);
        return outcome.Kind == ListingOutcomeKind.Success ? TypedResults.NoContent() : ToResult(outcome);
    }

    private static IResult ToResult(ListingOutcome outcome) => outcome.Kind switch
    {
        ListingOutcomeKind.Success => TypedResults.Ok(ToResponse(outcome.Listing!)),
        ListingOutcomeKind.NotFound => TypedResults.NotFound(),
        ListingOutcomeKind.Forbidden => TypedResults.Forbid(),
        ListingOutcomeKind.Validation => TypedResults.ValidationProblem(outcome.Errors!),
        ListingOutcomeKind.NotPublished => TypedResults.Problem(
            detail: "Only a published listing can be unpublished.", statusCode: StatusCodes.Status409Conflict),
        ListingOutcomeKind.ActiveOrders => TypedResults.Problem(
            title: "active-orders",
            detail: "This listing has active orders, so it can't be deleted. Unpublish it instead.",
            statusCode: StatusCodes.Status409Conflict),
        _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };

    private static ListingInput ToInput(CreateListingRequest request) => new(
        request.Title, request.Description, request.Category, request.MinimumPrice, request.MaximumPrice, request.Status);

    private static ListingInput ToInput(UpdateListingRequest request) => new(
        request.Title, request.Description, request.Category, request.MinimumPrice, request.MaximumPrice);

    private static ListingResponse ToResponse(Listing listing) => new(
        listing.Id, listing.Title, listing.Description, listing.Category.Name,
        listing.MinimumPrice, listing.MaximumPrice, listing.Status.ToString());
}

internal sealed record CreateListingRequest(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string? Status);
internal sealed record UpdateListingRequest(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice);
internal sealed record ListingResponse(int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string Status);
