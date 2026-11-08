using HireNow.Domain;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Api.Services;

/// <summary>What anyone can see, signed in or not: seller profiles and Published services.</summary>
/// <remarks>
/// Ratings (HN-28), completed orders (HN-22) and cover images (HN-08) don't exist yet.
/// Their fields are in the responses already, so the frontend and HN-13's search can rely on the shape.
/// </remarks>
internal static class PublicServiceEndpoints
{
    // Shown when a seller has listings but hasn't filled in their profile yet.
    private const string UnnamedSeller = "Service provider";

    public static void MapPublicServiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sellers/{userId}", GetSeller);
        app.MapGet("/services/{id:int}", GetService);
    }

    private static async Task<IResult> GetSeller(string userId, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var services = await db.Listings
            .Include(l => l.Category)
            .Where(l => l.UserId == userId && l.Status == ListingStatus.Published)
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);

        if (profile is null && services.Count == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new SellerProfileResponse(
            userId,
            profile?.DisplayName is { Length: > 0 } name ? name : UnnamedSeller,
            profile?.Bio ?? string.Empty,
            profile?.Location ?? string.Empty,
            profile?.ProfilePhoto ?? string.Empty,
            AverageRating: null,
            ReviewCount: 0,
            CompletedOrders: 0,
            services.Select(ToSummary).ToArray()));
    }

    // Only Published services are public. Drafts and unpublished ones are a 404, even for their owner,
    // who manages them under "My services" instead.
    private static async Task<IResult> GetService(int id, HireNowDbContext db, CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .Include(l => l.Category)
            .SingleOrDefaultAsync(l => l.Id == id && l.Status == ListingStatus.Published, cancellationToken);
        if (listing is null)
        {
            return TypedResults.NotFound();
        }

        var sellerName = await db.Profiles
            .Where(p => p.UserId == listing.UserId)
            .Select(p => p.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

        return TypedResults.Ok(new ServiceResponse(
            listing.Id,
            listing.Title,
            listing.Description,
            listing.Category.Name,
            listing.MinimumPrice,
            listing.MaximumPrice,
            CoverImageUrl: null,
            AverageRating: null,
            ReviewCount: 0,
            new SellerSummary(listing.UserId, string.IsNullOrWhiteSpace(sellerName) ? UnnamedSeller : sellerName)));
    }

    private static ServiceSummary ToSummary(Listing listing) => new(
        listing.Id,
        listing.Title,
        listing.Category.Name,
        listing.MinimumPrice,
        listing.MaximumPrice,
        CoverImageUrl: null,
        AverageRating: null,
        ReviewCount: 0);
}

/// <summary>A service as it appears on a card: on a seller's profile now, and in search results (HN-13).</summary>
internal sealed record ServiceSummary(int Id, string Title, string Category, long MinimumPrice, long MaximumPrice, string? CoverImageUrl, double? AverageRating, int ReviewCount);

internal sealed record SellerProfileResponse(
    string UserId, string DisplayName, string Bio, string Location, string ProfilePhoto,
    double? AverageRating, int ReviewCount, int CompletedOrders, ServiceSummary[] Services);

internal sealed record SellerSummary(string UserId, string DisplayName);

internal sealed record ServiceResponse(
    int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice,
    string? CoverImageUrl, double? AverageRating, int ReviewCount, SellerSummary Seller);
