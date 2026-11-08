using HireNow.Domain;

namespace HireNow.Tests;

public class ListingTests
{
    [Fact]
    public void Listing_supports_draft_and_published_statuses()
    {
        var draft = new Listing
        {
            Title = "Logo design",
            Description = "I create clean brand identities.",
            Category = Category.Other,
            MinimumPrice = 50,
            MaximumPrice = 200,
            UserId = "seller-1",
            Status = ListingStatus.Draft,
        };

        var published = new Listing
        {
            Title = "Logo design",
            Description = "I create clean brand identities.",
            Category = Category.Other,
            MinimumPrice = 50,
            MaximumPrice = 200,
            UserId = "seller-1",
            Status = ListingStatus.Published,
        };

        Assert.Equal(ListingStatus.Draft, draft.Status);
        Assert.Equal(ListingStatus.Published, published.Status);
    }

    [Fact]
    public void Listing_price_range_is_valid_when_minimum_is_at_or_below_maximum()
    {
        Assert.True(Listing.IsValidPriceRange(50, 200));
        Assert.False(Listing.IsValidPriceRange(250, 200));
    }
}
