using HireNow.Application.Listings;
using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

internal sealed class ListingData(HireNowDbContext db) : IListingData
{
    public async Task<IReadOnlyList<Listing>> GetBySellerAsync(string? userId, CancellationToken cancellationToken) =>
        await db.Listings.Include(l => l.Category)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);

    public Task<Listing?> FindAsync(int id, CancellationToken cancellationToken) =>
        db.Listings.Include(l => l.Category).SingleOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<Category?> FindCategoryAsync(string name, CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(c => c.Name == name, cancellationToken);

    public void Add(Listing listing) => db.Listings.Add(listing);
    public void Remove(Listing listing) => db.Listings.Remove(listing);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
