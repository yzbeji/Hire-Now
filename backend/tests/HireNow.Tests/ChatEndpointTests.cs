using System.Net;
using System.Net.Http.Json;
using HireNow.Domain;

namespace HireNow.Tests;

public sealed class ChatEndpointTests : IDisposable
{
    private readonly TestApi _api = new();

    [Fact]
    public async Task Buyer_can_start_and_reopen_a_conversation_with_a_seller()
    {
        var seller = _api.ClientFor("seller-1");
        var profile = await seller.PutAsJsonAsync("/me/profile", new { displayName = "Sam Seller" });
        profile.EnsureSuccessStatusCode();

        var buyer = _api.ClientFor("buyer-1", AccountRoles.Buyer);
        var first = await buyer.PostAsJsonAsync("/conversations", new { sellerId = "seller-1" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var conversation = await first.Content.ReadFromJsonAsync<ConversationResponse>();
        Assert.NotNull(conversation);
        Assert.Equal("buyer-1", conversation.BuyerId);
        Assert.Equal("seller-1", conversation.SellerId);

        var reopened = await buyer.PostAsJsonAsync("/conversations", new { sellerId = "seller-1" });
        Assert.Equal(conversation.Id, (await reopened.Content.ReadFromJsonAsync<ConversationResponse>())!.Id);

        var conversations = await buyer.GetFromJsonAsync<ConversationResponse[]>("/conversations");
        var listed = Assert.Single(conversations!);
        Assert.Equal(conversation.Id, listed.Id);
        Assert.Equal("Sam Seller", listed.ParticipantName);
    }

    public void Dispose()
    {
        _api.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed record ConversationResponse(Guid Id, string BuyerId, string SellerId, string? ParticipantName);
}
