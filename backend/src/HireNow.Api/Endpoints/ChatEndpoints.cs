using System.Security.Claims;
using HireNow.Application.Chat;
using HireNow.Domain;

namespace HireNow.Api.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/conversations").RequireAuthorization(policy => policy.RequireClaim("sub"));

        group.MapPost("/", async (StartConversationRequest request,
                                  ClaimsPrincipal user,
                                  IChatService chat,
                                  CancellationToken cancellationToken) =>
        {
            try
            {
                var conversation = await chat.StartConversationAsync(user.FindFirstValue("sub")!,
                                                                     request.SellerId,
                                                                     cancellationToken);
                return Results.Ok(conversation);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(exception.Message);
            }
        }).RequireAuthorization(AccountRoles.Buyer);

        group.MapGet("/", async (ClaimsPrincipal user,
                                 IChatService chat,
                                 CancellationToken cancellationToken) =>
            Results.Ok(await chat.GetConversationsAsync(user.FindFirstValue("sub")!,
                                                        cancellationToken)));

        group.MapGet("/{conversationId:guid}/messages", async (Guid conversationId,
                                                               ClaimsPrincipal user,
                                                               IChatService chat,
                                                               CancellationToken cancellationToken) =>
        {
            try
            {
                var messages = await chat.GetMessagesAsync(conversationId,
                                                           user.FindFirstValue("sub")!,
                                                           cancellationToken);
                return Results.Ok(messages);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });
    }

    private sealed record StartConversationRequest(string SellerId);
}
