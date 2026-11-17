using HireNow.Application.PublicServices;
using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

internal sealed class PublicServiceData(HireNowDbContext db) : IPublicServiceData
{
    public Task<Profile?> FindProfileAsync(string userId, CancellationToken cancellationToken)
    {
        return db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Listing>> GetPublishedServicesAsync(string userId, CancellationToken cancellationToken)
    {
        return await db.Listings
            .Include(l => l.Category)
            .Where(l => l.UserId == userId && l.Status == ListingStatus.Published)
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Listing?> FindPublishedServiceAsync(int id, CancellationToken cancellationToken)
    {
        return db.Listings
            .Include(l => l.Category)
            .SingleOrDefaultAsync(l => l.Id == id && l.Status == ListingStatus.Published, cancellationToken);
    }

    public Task<string?> GetSellerNameAsync(string userId, CancellationToken cancellationToken)
    {
        return db.Profiles
            .Where(p => p.UserId == userId)
            .Select(p => p.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
