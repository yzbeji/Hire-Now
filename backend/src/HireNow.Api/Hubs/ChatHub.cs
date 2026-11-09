using HireNow.Application.Chat;
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

    public async Task<MessageDto> SendMessage(Guid conversationId,
                                              string content)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Unable to identify your account.");
        }

        ConversationDto conversation;
        MessageDto message;

        try
        {
            conversation = await _chatService.GetConversationAsync(conversationId,
                                                                   userId,
                                                                   Context.ConnectionAborted)
                ?? throw new KeyNotFoundException("Conversation not found.");

            message = await _chatService.SendMessageAsync(conversationId,
                                                          userId,
                                                          content,
                                                          Context.ConnectionAborted);
        }
        catch (ArgumentException exception)
        {
            throw new HubException(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            throw new HubException(exception.Message);
        }

        await Clients.Users(
            conversation.BuyerId,
            conversation.SellerId
        ).SendAsync("ReceiveMessage", message);

        return message;
    }
}
