namespace HireNow.Application.Chat;

public interface IChatService
{
    Task<ConversationDto> StartConversationAsync(string buyerId,
                                                 string sellerId,
                                                 CancellationToken cancellationToken = default);

    Task<ConversationDto?> GetConversationAsync(Guid conversationId,
                                                string userId,
                                                CancellationToken cancellationToken = default);

    Task<MessageDto> SendMessageAsync(Guid conversationId,
                                      string senderId,
                                      string content,
                                      CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid conversationId,
                                                    string userId,
                                                    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string userId,
                                                               CancellationToken cancellationToken = default);
}