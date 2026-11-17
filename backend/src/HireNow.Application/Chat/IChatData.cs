namespace HireNow.Application.Chat;

public interface IChatData
{
    Task<Conversation?> FindExistingAsync(string buyerId, string sellerId, CancellationToken cancellationToken);
    Task<bool> HasProfileAsync(string userId, CancellationToken cancellationToken);
    Task<bool> HasPublishedServiceAsync(string userId, CancellationToken cancellationToken);
    Task<Conversation?> FindParticipantConversationAsync(Guid id, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<(Conversation Conversation, string ParticipantName)>> GetParticipantConversationsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> IsParticipantAsync(Guid id, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid id, CancellationToken cancellationToken);
    void Add(Conversation conversation);
    void Add(ChatMessage message);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
