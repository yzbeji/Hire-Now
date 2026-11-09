using HireNow.Application.Chat;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Chat;

public sealed class ChatService(HireNowDbContext db) : IChatService
{
    public async Task<ConversationDto> StartConversationAsync(string buyerId,
                                                              string sellerId,
                                                              CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buyerId);

        if (string.IsNullOrWhiteSpace(sellerId) || buyerId == sellerId)
        {
            throw new ArgumentException("Choose another user's profile to start a conversation.");
        }

        var conversation = await db.Conversations.AsNoTracking()
                                                 .FirstOrDefaultAsync(c =>
                                                    c.BuyerId == buyerId &&
                                                    c.SellerId == sellerId, cancellationToken);

        if (conversation is not null)
        {
            return ConversationDto.From(conversation);
        }

        if (!await db.Profiles.AnyAsync(p => p.UserId == sellerId, cancellationToken))
        {
            throw new ArgumentException("The recipient's profile does not exist.");
        }

        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);
        return ConversationDto.From(conversation);
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationId,
                                                             string userId,
                                                             CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var conversation = await ParticipantConversations(userId)
                                .SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        return conversation is null ? null : ConversationDto.From(conversation);
    }

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string userId,
                                                                            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await ParticipantConversations(userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ConversationDto(c.Id, c.BuyerId, c.SellerId,
                        db.Profiles.Where(p => p.UserId == (c.BuyerId == userId ? c.SellerId : c.BuyerId))
                                   .Select(p => p.DisplayName)
                                   .FirstOrDefault() ?? (c.BuyerId == userId ? "Service provider" : "Service seeker")))
                    .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid conversationId,
                                                                  string userId,
                                                                  CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (!await ParticipantConversations(userId).AnyAsync(c => c.Id == conversationId, cancellationToken))
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        var messages = await db.ChatMessages.AsNoTracking()
                                            .Where(m => m.ConversationId == conversationId)
                                            .OrderBy(m => m.SentAt).ThenBy(m => m.Id)
                                            .ToListAsync(cancellationToken);

        return messages.Select(MessageDto.From).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(Guid conversationId,
                                                   string senderId,
                                                   string content,
                                                   CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senderId);

        content = content?.Trim() ?? string.Empty;

        if (content.Length == 0 || content.Length > 2000)
        {
            throw new ArgumentException("Messages must contain between 1 and 2000 characters.");
        }

        if (!await ParticipantConversations(senderId).AnyAsync(c => c.Id == conversationId, cancellationToken))
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

        db.ChatMessages.Add(message);

        await db.SaveChangesAsync(cancellationToken);

        return MessageDto.From(message);
    }

    private IQueryable<Conversation> ParticipantConversations(string userId) =>
        db.Conversations.AsNoTracking().Where(c => c.BuyerId == userId || c.SellerId == userId);
}
