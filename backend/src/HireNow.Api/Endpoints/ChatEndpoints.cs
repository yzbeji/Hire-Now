using System.Security.Claims;
using HireNow.Application.Chat;
using HireNow.Domain;

namespace HireNow.Api.Endpoints;

internal static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/conversations").RequireAuthorization(policy => policy.RequireClaim("sub"));

        group.MapPost("/", StartConversation).RequireAuthorization(AccountRoles.Buyer);
        group.MapGet("/", GetConversations);
        group.MapGet("/{conversationId:guid}/messages", GetMessages);
    }

    private static async Task<IResult> StartConversation(StartConversationRequest request, ClaimsPrincipal user,
                                                         IChatService chat, CancellationToken cancellationToken)
    {
        try
        {
            var conversation = await chat.StartConversationAsync(user.FindFirstValue("sub")!,
                                                                 request.SellerId,
                                                                 cancellationToken);

            return Results.Ok(new ConversationResponse(conversation.Id, conversation.BuyerId, conversation.SellerId));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }

    private static async Task<IResult> GetConversations(ClaimsPrincipal user, IChatService chat, CancellationToken cancellationToken)
    {
        var conversations = await chat.GetConversationsAsync(user.FindFirstValue("sub")!, cancellationToken);

        return Results.Ok(conversations.Select(c => new ConversationResponse(c.Conversation.Id, c.Conversation.BuyerId, 
                                                                             c.Conversation.SellerId, c.ParticipantName)).ToList());
    }

    private static async Task<IResult> GetMessages(Guid conversationId, ClaimsPrincipal user,
                                                   IChatService chat, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await chat.GetMessagesAsync(conversationId, user.FindFirstValue("sub")!, cancellationToken);

            return Results.Ok(messages.Select(ChatMessageResponse.From).ToList());
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }
}

internal sealed record StartConversationRequest(string SellerId);
internal sealed record ConversationResponse(Guid Id, string BuyerId, string SellerId, string? ParticipantName = null);

public sealed record ChatMessageResponse(Guid Id, Guid ConversationId, string SenderId, string Content, DateTime SentAt)
{
    public static ChatMessageResponse From(ChatMessage message)
    {
        return new(message.Id, message.ConversationId, 
                   message.SenderId, message.Content,
                   DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc));
    }
}
