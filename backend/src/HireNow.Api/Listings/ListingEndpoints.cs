using System.Security.Claims;
using HireNow.Application.Listings;
using HireNow.Domain;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Api.Listings;

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

    private static async Task<IResult> GetMyListings(ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        var listings = await db.Listings
            .Include(l => l.Category)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(listings.Select(ToResponse));
    }

    private static async Task<IResult> GetMyListing(int id, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var (listing, denied) = await FindOwnedListing(id, user, db, cancellationToken);
        return denied ?? TypedResults.Ok(ToResponse(listing!));
    }

    private static async Task<IResult> CreateListing(CreateListingRequest request, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var (errors, category) = await Validate(request.Title, request.Description, request.Category, request.MinimumPrice, request.MaximumPrice, db, cancellationToken);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var listing = new Listing
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = category!,
            MinimumPrice = request.MinimumPrice,
            MaximumPrice = request.MaximumPrice,
            Status = Enum.TryParse<ListingStatus>(request.Status, true, out var status) ? status : ListingStatus.Draft,
            UserId = user.FindFirstValue("sub")!,
        };

        db.Listings.Add(listing);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/me/listings/{listing.Id}", ToResponse(listing));
    }

    private static async Task<IResult> UpdateListing(int id, UpdateListingRequest request, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var (listing, denied) = await FindOwnedListing(id, user, db, cancellationToken);
        if (denied is not null)
        {
            return denied;
        }

        var (errors, category) = await Validate(request.Title, request.Description, request.Category, request.MinimumPrice, request.MaximumPrice, db, cancellationToken);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        // Orders store their own agreed price, so changing the range here leaves them alone.
        listing!.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Category = category!;
        listing.MinimumPrice = request.MinimumPrice;
        listing.MaximumPrice = request.MaximumPrice;

        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ToResponse(listing));
    }

    private static async Task<IResult> PublishListing(int id, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var (listing, denied) = await FindOwnedListing(id, user, db, cancellationToken);
        if (denied is not null)
        {
            return denied;
        }

        listing!.Publish();
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ToResponse(listing));
    }

    private static async Task<IResult> UnpublishListing(int id, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var (listing, denied) = await FindOwnedListing(id, user, db, cancellationToken);
        if (denied is not null)
        {
            return denied;
        }

        if (!listing!.TryUnpublish())
        {
            return TypedResults.Problem(
                detail: "Only a published listing can be unpublished.",
                statusCode: StatusCodes.Status409Conflict);
        }

        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ToResponse(listing));
    }

    private static async Task<IResult> DeleteListing(int id, ClaimsPrincipal user, HireNowDbContext db, IListingOrders orders, CancellationToken cancellationToken)
    {
        var (listing, denied) = await FindOwnedListing(id, user, db, cancellationToken);
        if (denied is not null)
        {
            return denied;
        }

        if (!Listing.CanBeDeleted(await orders.HasActiveOrdersAsync(listing!.Id, cancellationToken)))
        {
            return TypedResults.Problem(
                title: "active-orders",
                detail: "This listing has active orders, so it can't be deleted. Unpublish it instead.",
                statusCode: StatusCodes.Status409Conflict);
        }

        db.Listings.Remove(listing);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    // 404 if the listing doesn't exist, 403 if it belongs to another seller.
    private static async Task<(Listing? Listing, IResult? Denied)> FindOwnedListing(int id, ClaimsPrincipal user, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.Include(l => l.Category).SingleOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (listing is null)
        {
            return (null, TypedResults.NotFound());
        }

        return listing.IsOwnedBy(user.FindFirstValue("sub")!) ? (listing, null) : (null, TypedResults.Forbid());
    }

    private static async Task<(Dictionary<string, string[]> Errors, Category? Category)> Validate(
        string? title, string? description, string? categoryName, long minimumPrice, long maximumPrice,
        HireNowDbContext db, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        title = title?.Trim() ?? string.Empty;
        if (title.Length == 0)
        {
            errors["title"] = ["Title is required."];
        }
        else if (title.Length > 100)
        {
            errors["title"] = ["Title must be 100 characters or fewer."];
        }

        description = description?.Trim() ?? string.Empty;
        if (description.Length == 0)
        {
            errors["description"] = ["Description is required."];
        }
        else if (description.Length > 5000)
        {
            errors["description"] = ["Description must be 5000 characters or fewer."];
        }

        Category? category = null;
        categoryName = categoryName?.Trim() ?? string.Empty;
        if (categoryName.Length == 0)
        {
            errors["category"] = ["Category is required."];
        }
        else
        {
            category = await db.Categories.SingleOrDefaultAsync(c => c.Name == categoryName, cancellationToken);
            if (category is null)
            {
                errors["category"] = ["Choose a valid category."];
            }
        }

        if (minimumPrice <= 0)
        {
            errors["minimumPrice"] = ["Minimum price must be greater than 0."];
        }

        if (maximumPrice <= 0)
        {
            errors["maximumPrice"] = ["Maximum price must be greater than 0."];
        }
        else if (minimumPrice > 0 && !Listing.IsValidPriceRange(minimumPrice, maximumPrice))
        {
            errors["minimumPrice"] = ["Minimum price can't exceed maximum price."];
        }

        return (errors, category);
    }

    private static ListingResponse ToResponse(Listing listing) => new(
        listing.Id,
        listing.Title,
        listing.Description,
        listing.Category.Name,
        listing.MinimumPrice,
        listing.MaximumPrice,
        listing.Status.ToString());
}

internal sealed record CreateListingRequest(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string? Status);
internal sealed record UpdateListingRequest(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice);
internal sealed record ListingResponse(int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string Status);
