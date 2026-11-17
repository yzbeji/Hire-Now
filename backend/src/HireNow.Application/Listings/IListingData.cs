using HireNow.Domain;

namespace HireNow.Application.Listings;

public interface IListingData
{
    Task<IReadOnlyList<Listing>> GetBySellerAsync(string? userId, CancellationToken cancellationToken);
    Task<Listing?> FindAsync(int id, CancellationToken cancellationToken);
    Task<Category?> FindCategoryAsync(string name, CancellationToken cancellationToken);
    void Add(Listing listing);
    void Remove(Listing listing);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
