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

    private static Listing NewListing(ListingStatus status) => new()
    {
        Title = "Logo design",
        Description = "I create clean brand identities.",
        MinimumPrice = 50,
        MaximumPrice = 200,
        UserId = "seller-1",
        Status = status,
    };

    [Fact]
    public void A_published_listing_can_be_unpublished_and_republished()
    {
        var listing = NewListing(ListingStatus.Published);

        Assert.True(listing.TryUnpublish());
        Assert.Equal(ListingStatus.Unpublished, listing.Status);

        listing.Publish();
        Assert.Equal(ListingStatus.Published, listing.Status);
    }

    [Theory]
    [InlineData(ListingStatus.Draft)]
    [InlineData(ListingStatus.Unpublished)]
    public void Only_a_published_listing_can_be_unpublished(ListingStatus status)
    {
        var listing = NewListing(status);

        Assert.False(listing.TryUnpublish());
        Assert.Equal(status, listing.Status);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void A_listing_can_be_deleted_only_without_active_orders(bool hasActiveOrders, bool expected)
    {
        Assert.Equal(expected, Listing.CanBeDeleted(hasActiveOrders));
    }

    [Fact]
    public void Ownership_is_by_user_id()
    {
        var listing = NewListing(ListingStatus.Published);

        Assert.True(listing.IsOwnedBy("seller-1"));
        Assert.False(listing.IsOwnedBy("seller-2"));
    }
}
