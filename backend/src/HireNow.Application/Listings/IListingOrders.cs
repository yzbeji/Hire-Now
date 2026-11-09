namespace HireNow.Application.Listings;

/// <summary>What the listing rules need to know about orders.</summary>
/// <remarks>
/// Orders keep their own agreed price (HN-17), so editing a listing's price never changes them.
/// </remarks>
public interface IListingOrders
{
    /// <summary>True if the listing has an order that isn't finished: Requested, Accepted, Paid or Delivered.</summary>
    Task<bool> HasActiveOrdersAsync(int listingId, CancellationToken cancellationToken);
}
