using HireNow.Domain;

namespace HireNow.Application.PublicServices;

public sealed class PublicServiceService(IPublicServiceData data)
{
    public async Task<(Profile? Profile, IReadOnlyList<Listing> Services)> GetSellerAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await data.FindProfileAsync(userId, cancellationToken);
        var services = await data.GetPublishedServicesAsync(userId, cancellationToken);

        return (profile, services);
    }

    public async Task<(Listing? Listing, string? SellerName)> GetServiceAsync(int id, CancellationToken cancellationToken)
    {
        var listing = await data.FindPublishedServiceAsync(id, cancellationToken);
        if (listing is null)
        {
            return (null, null);
        }

        var sellerName = await data.GetSellerNameAsync(listing.UserId, cancellationToken);

        return (listing, sellerName);
    }
}
