namespace HireNow.Application.Chat;

public interface IChatService
{
    Task<Conversation> StartConversationAsync(string buyerId, string sellerId,
                                              CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationAsync(Guid conversationId, string userId,
                                             CancellationToken cancellationToken = default);

    Task<ChatMessage> SendMessageAsync(Guid conversationId, string senderId,
                                      string content, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, string userId,
                                                    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Conversation Conversation, string ParticipantName)>> GetConversationsAsync(string userId,
                                                                                                   CancellationToken cancellationToken = default);
}
