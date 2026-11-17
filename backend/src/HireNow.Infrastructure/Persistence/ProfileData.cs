using HireNow.Application.Profiles;
using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

internal sealed class ProfileData(HireNowDbContext db) : IProfileData
{
    public Task<Profile?> FindAsync(string userId, CancellationToken cancellationToken) =>
        db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public void Add(Profile profile) => db.Profiles.Add(profile);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
