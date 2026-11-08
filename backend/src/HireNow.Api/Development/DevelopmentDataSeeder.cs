using HireNow.Domain;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Api.Development;

/// <summary>
/// Gives the realm's test users (seller, buyer) a profile and some listings, so the app has data to walk through.
/// Runs in Development only, and does nothing once the seller has listings.
/// </summary>
internal sealed class DevelopmentDataSeeder(IServiceScopeFactory scopes, ILogger<DevelopmentDataSeeder> logger) : BackgroundService
{
    // The fixed ids of the test users in keycloak/hirenow-realm.json.
    internal const string SellerId = "5e11e400-0000-4000-8000-000000000001";
    internal const string BuyerId = "b0b0b0b0-0000-4000-8000-000000000002";

    // A seller who exists only in this sample data, so search has more than one provider to show.
    internal const string OtherSellerId = "sample-seller-rita";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await SeedAsync(stoppingToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Sample data wasn't seeded.");
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HireNowDbContext>();
        const string sellerId = SellerId;

        if (await db.Listings.AnyAsync(l => l.UserId == sellerId, cancellationToken))
        {
            logger.LogInformation("Sample data is already there.");
            return;
        }

        var categories = await db.Categories.ToDictionaryAsync(c => c.Name, cancellationToken);

        AddProfileIfMissing(db, sellerId, "Sam Seller", "Freelance designer and developer. I build small business websites and brand identities, usually within two weeks.", "Cluj-Napoca");
        AddProfileIfMissing(db, OtherSellerId, "Rita Pop", "Maths and physics tutor with eight years of experience preparing students for the baccalaureate.", "Iași");
        AddProfileIfMissing(db, BuyerId, "Bo Buyer", "Running a small bakery and looking for help with the website and marketing.", "Bucharest");

        db.Listings.AddRange(
            NewListing(sellerId, categories["Graphic Design"], ListingStatus.Published, 150, 600,
                "Logo and brand identity",
                "A logo, colour palette and type pairing for your business.\n\n- Three initial concepts\n- Two rounds of changes\n- Files for print and web"),
            NewListing(sellerId, categories["Programming"], ListingStatus.Published, 400, 2500,
                "Website for a small business",
                "A fast, mobile-friendly website with up to 6 pages, a contact form and basic SEO. I can also connect a booking or shop system."),
            NewListing(sellerId, categories["Consulting"], ListingStatus.Published, 50, 200,
                "One-hour website review",
                "I go through your website on a call and send a written list of fixes for speed, accessibility and search ranking."),
            NewListing(sellerId, categories["Photography"], ListingStatus.Unpublished, 200, 800,
                "Product photography",
                "Clean product photos on a white or styled background. Paused while my studio is being renovated."),
            NewListing(sellerId, categories["Programming"], ListingStatus.Draft, 300, 1500,
                "Mobile app prototype",
                "A clickable prototype of your app idea, ready to show to investors or test with users."),
            NewListing(OtherSellerId, categories["Tutoring"], ListingStatus.Published, 60, 120,
                "Maths tutoring for the baccalaureate",
                "Weekly one-to-one lessons online or in person, with practice papers and progress notes after each session."),
            NewListing(OtherSellerId, categories["Tutoring"], ListingStatus.Published, 60, 120,
                "Physics tutoring for high school",
                "Help with homework, lab reports and exam preparation for grades 9 to 12."),
            NewListing(OtherSellerId, categories["Home Repairs"], ListingStatus.Published, 80, 400,
                "Small home repairs",
                "Shelves, curtain rails, leaking taps and furniture assembly. Tools included."));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded sample profiles and listings for the 'seller' and 'buyer' users.");
    }

    private static void AddProfileIfMissing(HireNowDbContext db, string userId, string displayName, string bio, string location)
    {
        if (db.Profiles.Any(p => p.UserId == userId))
        {
            return;
        }

        db.Profiles.Add(new Profile
        {
            UserId = userId,
            DisplayName = displayName,
            Bio = bio,
            Location = location,
            ProfilePhoto = string.Empty,
        });
    }

    private static Listing NewListing(string userId, Category category, ListingStatus status, long minimumPrice, long maximumPrice, string title, string description) => new()
    {
        UserId = userId,
        Category = category,
        Status = status,
        MinimumPrice = minimumPrice,
        MaximumPrice = maximumPrice,
        Title = title,
        Description = description,
    };
}
