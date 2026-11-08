using System.Net;
using System.Net.Http.Json;
using static HireNow.Tests.ListingEndpointTests;

namespace HireNow.Tests;

public class PublicServiceEndpointTests : IDisposable
{
    private readonly TestApi _api = new();

    public void Dispose()
    {
        _api.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<HttpClient> SellerWithProfile(string userId, string displayName)
    {
        var seller = _api.ClientFor(userId);
        var response = await seller.PutAsJsonAsync("/me/profile", new { displayName, bio = "I design logos.", location = "Cluj-Napoca" });
        response.EnsureSuccessStatusCode();
        return seller;
    }

    [Fact]
    public async Task Anyone_sees_a_sellers_profile_with_only_their_published_services()
    {
        var seller = await SellerWithProfile("seller-1", "Sam Seller");
        var live = await Create(seller);
        await Create(seller, status: "Draft");
        var hidden = await Create(seller);
        await seller.PostAsync($"/me/listings/{hidden.Id}/unpublish", null);
        await Create(await SellerWithProfile("seller-2", "Rita Pop"));

        var response = await _api.Anonymous().GetAsync("/sellers/seller-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<SellerProfileDto>();
        Assert.Equal("Sam Seller", profile!.DisplayName);
        Assert.Equal("Cluj-Napoca", profile.Location);
        var card = Assert.Single(profile.Services);
        Assert.Equal(new ServiceSummaryDto(live.Id, "Logo design", "Graphic Design", 50, 200, null, null, 0), card);
    }

    [Fact]
    public async Task A_seller_without_reviews_or_orders_has_no_rating_and_no_completed_orders()
    {
        await SellerWithProfile("seller-1", "Sam Seller");

        var profile = await _api.Anonymous().GetFromJsonAsync<SellerProfileDto>("/sellers/seller-1");

        Assert.Null(profile!.AverageRating);
        Assert.Equal(0, profile.ReviewCount);
        Assert.Equal(0, profile.CompletedOrders);
        Assert.Empty(profile.Services);
    }

    [Fact]
    public async Task An_unknown_seller_is_not_found()
    {
        var response = await _api.Anonymous().GetAsync("/sellers/nobody");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_published_service_page_names_its_seller()
    {
        var seller = await SellerWithProfile("seller-1", "Sam Seller");
        var listing = await Create(seller);

        var service = await _api.Anonymous().GetFromJsonAsync<ServiceDto>($"/services/{listing.Id}");

        Assert.Equal("Logo design", service!.Title);
        Assert.Equal("Clean brand identities.", service.Description);
        Assert.Equal(new SellerSummaryDto("seller-1", "Sam Seller"), service.Seller);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Unpublished")]
    public async Task Drafts_and_unpublished_services_are_not_public_even_to_their_owner(string status)
    {
        var seller = await SellerWithProfile("seller-1", "Sam Seller");
        var listing = await Create(seller, status: status == "Draft" ? "Draft" : "Published");
        if (status == "Unpublished")
        {
            await seller.PostAsync($"/me/listings/{listing.Id}/unpublish", null);
        }

        Assert.Equal(HttpStatusCode.NotFound, (await _api.Anonymous().GetAsync($"/services/{listing.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await seller.GetAsync($"/services/{listing.Id}")).StatusCode);
    }

    private sealed record ServiceSummaryDto(int Id, string Title, string Category, long MinimumPrice, long MaximumPrice, string? CoverImageUrl, double? AverageRating, int ReviewCount);

    private sealed record SellerProfileDto(
        string UserId, string DisplayName, string Bio, string Location, string ProfilePhoto,
        double? AverageRating, int ReviewCount, int CompletedOrders, ServiceSummaryDto[] Services);

    private sealed record SellerSummaryDto(string UserId, string DisplayName);

    private sealed record ServiceDto(int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, SellerSummaryDto Seller);
}
