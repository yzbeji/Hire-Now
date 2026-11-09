using HireNow.Application.Listings;

namespace HireNow.Infrastructure.Persistence;

// TODO(HN-16): query the Order table once it exists, counting orders not in Completed, Declined or Cancelled.
internal sealed class ListingOrders : IListingOrders
{
    public Task<bool> HasActiveOrdersAsync(int listingId, CancellationToken cancellationToken) =>
        Task.FromResult(false);
}
