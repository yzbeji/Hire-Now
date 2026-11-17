using HireNow.Api.Endpoints;
using HireNow.Application.Chat;
using HireNow.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HireNow.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public const string Path = "/hubs/chat";

    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override Task OnConnectedAsync()
    {
        if (string.IsNullOrWhiteSpace(Context.UserIdentifier))
        {
            Context.Abort();
            throw new HubException("Unable to identify your account.");
        }

        return base.OnConnectedAsync();
    }

    public async Task<ChatMessageResponse> SendMessage(Guid conversationId, string content)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Unable to identify your account.");
        }

        Conversation conversation;
        ChatMessage message;

        try
        {
            conversation = await _chatService.GetConversationAsync(conversationId, userId, Context.ConnectionAborted)
                ?? throw new KeyNotFoundException("Conversation not found.");

            message = await _chatService.SendMessageAsync(conversationId, userId, content, Context.ConnectionAborted);
        }
        catch (ArgumentException exception)
        {
            throw new HubException(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            throw new HubException(exception.Message);
        }

        var response = ChatMessageResponse.From(message);

        await Clients.Users(conversation.BuyerId, conversation.SellerId).SendAsync("ReceiveMessage", response);

        return response;
    }
}
