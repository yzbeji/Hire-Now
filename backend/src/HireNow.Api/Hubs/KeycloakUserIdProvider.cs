using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace HireNow.Api.Hubs;

public sealed class KeycloakUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User?.FindFirstValue("sub");
}
