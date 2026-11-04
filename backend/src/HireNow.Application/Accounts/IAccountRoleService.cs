namespace HireNow.Application.Accounts;

public interface IAccountRoleService
{
    /// <summary>Gives a new user their first role. Returns false if they already have an application role.</summary>
    Task<bool> TryAssignInitialRoleAsync(string userId, string role, CancellationToken cancellationToken);
}
