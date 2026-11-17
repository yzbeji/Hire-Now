namespace HireNow.Application.Chat;

public sealed class ChatService(IChatData data) : IChatService
{
    public async Task<Conversation> StartConversationAsync(string buyerId, string sellerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buyerId);

        if (string.IsNullOrWhiteSpace(sellerId) || buyerId == sellerId)
        {
            throw new ArgumentException("Choose another user's profile to start a conversation.");
        }

        var conversation = await data.FindExistingAsync(buyerId, sellerId, cancellationToken);

        if (conversation is not null)
        {
            return conversation;
        }

        var hasProfile = await data.HasProfileAsync(sellerId, cancellationToken);
        var hasPublishedService = !hasProfile && await data.HasPublishedServiceAsync(sellerId, cancellationToken);

        if (!hasProfile && !hasPublishedService)
        {
            throw new ArgumentException("The recipient's profile or published service does not exist.");
        }

        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
        };

        data.Add(conversation);
        await data.SaveChangesAsync(cancellationToken);

        return conversation;
    }

    public async Task<Conversation?> GetConversationAsync(Guid conversationId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await data.FindParticipantConversationAsync(conversationId, userId, cancellationToken);
    }

    public Task<IReadOnlyList<(Conversation Conversation, string ParticipantName)>> GetConversationsAsync(string userId,
                                                                                                          CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return data.GetParticipantConversationsAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, string userId, 
                                                                   CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (!await data.IsParticipantAsync(conversationId, userId, cancellationToken))
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        return await data.GetMessagesAsync(conversationId, cancellationToken);
    }

    public async Task<ChatMessage> SendMessageAsync(Guid conversationId, string senderId,
                                                    string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senderId);

        content = content?.Trim() ?? string.Empty;

        if (content.Length == 0 || content.Length > 2000)
        {
            throw new ArgumentException("Messages must contain between 1 and 2000 characters.");
        }

        if (!await data.IsParticipantAsync(conversationId, senderId, cancellationToken))
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
        };

        data.Add(message);

        await data.SaveChangesAsync(cancellationToken);

        return message;
    }
}
