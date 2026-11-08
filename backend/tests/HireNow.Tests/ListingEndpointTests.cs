using System.Net;
using System.Net.Http.Json;
using HireNow.Domain;

namespace HireNow.Tests;

public class ListingEndpointTests : IDisposable
{
    private readonly TestApi _api = new();
    private readonly TestApi.FakeListingOrders _orders;

    public ListingEndpointTests() => _orders = _api.Orders;

    public void Dispose()
    {
        _api.Dispose();
        GC.SuppressFinalize(this);
    }

    private HttpClient ClientFor(string userId, string role = AccountRoles.Seller) => _api.ClientFor(userId, role);

    private static readonly object ValidListing = new
    {
        title = "Logo design",
        description = "Clean brand identities.",
        category = "Graphic Design",
        minimumPrice = 50,
        maximumPrice = 200,
        status = "Published",
    };

    internal static async Task<ListingDto> Create(HttpClient client, string status = "Published")
    {
        var response = await client.PostAsJsonAsync("/me/listings", new
        {
            title = "Logo design",
            description = "Clean brand identities.",
            category = "Graphic Design",
            minimumPrice = 50,
            maximumPrice = 200,
            status,
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ListingDto>())!;
    }

    [Fact]
    public async Task A_seller_can_edit_every_field_and_sees_the_change_straight_away()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);

        var response = await seller.PutAsJsonAsync($"/me/listings/{listing.Id}", new
        {
            title = "Logo and brand guide",
            description = "Now with a brand guide.",
            category = "Consulting",
            minimumPrice = 80,
            maximumPrice = 300,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reloaded = await seller.GetFromJsonAsync<ListingDto>($"/me/listings/{listing.Id}");
        Assert.Equal(
            new ListingDto(listing.Id, "Logo and brand guide", "Now with a brand guide.", "Consulting", 80, 300, "Published"),
            reloaded);
    }

    [Fact]
    public async Task An_invalid_edit_is_rejected_with_errors_per_field()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);

        var response = await seller.PutAsJsonAsync($"/me/listings/{listing.Id}", new
        {
            title = "",
            description = "Still here.",
            category = "Not a category",
            minimumPrice = 300,
            maximumPrice = 80,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.Equal(["category", "minimumPrice", "title"], problem!.Errors.Keys.Order());
    }

    [Fact]
    public async Task Unpublishing_hides_a_listing_and_it_can_be_republished()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);

        var unpublished = await seller.PostAsync($"/me/listings/{listing.Id}/unpublish", null);
        Assert.Equal("Unpublished", (await unpublished.Content.ReadFromJsonAsync<ListingDto>())!.Status);

        var republished = await seller.PostAsync($"/me/listings/{listing.Id}/publish", null);
        Assert.Equal("Published", (await republished.Content.ReadFromJsonAsync<ListingDto>())!.Status);
    }

    [Fact]
    public async Task Unpublishing_keeps_the_listing_for_existing_orders()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);
        _orders.ListingsWithActiveOrders.Add(listing.Id);

        var response = await seller.PostAsync($"/me/listings/{listing.Id}/unpublish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reloaded = await seller.GetFromJsonAsync<ListingDto>($"/me/listings/{listing.Id}");
        Assert.Equal("Unpublished", reloaded!.Status);
    }

    [Fact]
    public async Task A_draft_cannot_be_unpublished()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller, status: "Draft");

        var response = await seller.PostAsync($"/me/listings/{listing.Id}/unpublish", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task A_listing_without_active_orders_can_be_deleted()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);

        var response = await seller.DeleteAsync($"/me/listings/{listing.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await seller.GetAsync($"/me/listings/{listing.Id}")).StatusCode);
    }

    [Fact]
    public async Task A_listing_with_active_orders_cannot_be_deleted_and_the_seller_is_told_to_unpublish()
    {
        var seller = ClientFor("seller-1");
        var listing = await Create(seller);
        _orders.ListingsWithActiveOrders.Add(listing.Id);

        var response = await seller.DeleteAsync($"/me/listings/{listing.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Unpublish it instead", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await seller.GetAsync($"/me/listings/{listing.Id}")).StatusCode);
    }

    [Fact]
    public async Task Another_seller_cannot_see_or_change_the_listing()
    {
        var listing = await Create(ClientFor("seller-1"));
        var other = ClientFor("seller-2");

        Assert.Equal(HttpStatusCode.Forbidden, (await other.GetAsync($"/me/listings/{listing.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PutAsJsonAsync($"/me/listings/{listing.Id}", ValidListing)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PostAsync($"/me/listings/{listing.Id}/unpublish", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PostAsync($"/me/listings/{listing.Id}/publish", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/me/listings/{listing.Id}")).StatusCode);
        Assert.Empty((await other.GetFromJsonAsync<ListingDto[]>("/me/listings"))!);
    }

    [Fact]
    public async Task A_buyer_cannot_manage_listings()
    {
        var response = await ClientFor("buyer-1", AccountRoles.Buyer).GetAsync("/me/listings");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task My_listings_include_drafts_and_unpublished_ones()
    {
        var seller = ClientFor("seller-1");
        await Create(seller, status: "Draft");
        var live = await Create(seller);
        await seller.PostAsync($"/me/listings/{live.Id}/unpublish", null);

        var mine = await seller.GetFromJsonAsync<ListingDto[]>("/me/listings");

        Assert.Equal(["Unpublished", "Draft"], mine!.Select(l => l.Status));
    }

    internal sealed record ListingDto(int Id, string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string Status);

    private sealed record ValidationProblem(Dictionary<string, string[]> Errors);
}
