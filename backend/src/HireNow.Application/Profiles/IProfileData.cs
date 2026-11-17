using HireNow.Domain;

namespace HireNow.Application.Profiles;

public interface IProfileData
{
    Task<Profile?> FindAsync(string userId, CancellationToken cancellationToken);
    void Add(Profile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
