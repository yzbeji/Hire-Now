using HireNow.Domain;

namespace HireNow.Application.PublicServices;

public interface IPublicServiceData
{
    Task<Profile?> FindProfileAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Listing>> GetPublishedServicesAsync(string userId, CancellationToken cancellationToken);
    Task<Listing?> FindPublishedServiceAsync(int id, CancellationToken cancellationToken);
    Task<string?> GetSellerNameAsync(string userId, CancellationToken cancellationToken);
}
