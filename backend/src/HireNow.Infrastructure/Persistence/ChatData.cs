using HireNow.Application.Chat;
using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

internal sealed class ChatData(HireNowDbContext db) : IChatData
{
    public Task<Conversation?> FindExistingAsync(string buyerId, string sellerId, CancellationToken cancellationToken)
    {
        return db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.BuyerId == buyerId &&
                                                                        c.SellerId == sellerId, cancellationToken);
    }

    public Task<bool> HasProfileAsync(string userId, CancellationToken cancellationToken)
    {
        return db.Profiles.AnyAsync(p => p.UserId == userId, cancellationToken);
    }

    public Task<bool> HasPublishedServiceAsync(string userId, CancellationToken cancellationToken)
    {
        return db.Listings.AnyAsync(l => l.UserId == userId && l.Status == ListingStatus.Published, cancellationToken);
    }

    public Task<Conversation?> FindParticipantConversationAsync(Guid id, string userId, CancellationToken cancellationToken)
    {
        return ParticipantConversations(userId).SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<(Conversation Conversation, string ParticipantName)>> GetParticipantConversationsAsync(string userId, CancellationToken cancellationToken)
    {
        var conversations = await ParticipantConversations(userId)
                                  .OrderByDescending(c => c.CreatedAt)
                                  .Select(c => new
                                  {
                                      Conversation = c,
                                      ParticipantName = db.Profiles.Where(p => p.UserId == (c.BuyerId == userId ? c.SellerId : c.BuyerId))
                                                                   .Select(p => p.DisplayName)
                                                                   .FirstOrDefault() ?? (c.BuyerId == userId ? "Service provider" : "Service seeker"),
                                  })
                                  .ToListAsync(cancellationToken);

        return conversations.Select(c => (c.Conversation, c.ParticipantName)).ToList();
    }

    public Task<bool> IsParticipantAsync(Guid id, string userId, CancellationToken cancellationToken)
    {
        return ParticipantConversations(userId).AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.ChatMessages.AsNoTracking()
                                    .Where(m => m.ConversationId == id)
                                    .OrderBy(m => m.SentAt).ThenBy(m => m.Id)
                                    .ToListAsync(cancellationToken);
    }

    public void Add(Conversation conversation)
    {
        db.Conversations.Add(conversation);
    }

    public void Add(ChatMessage message)
    {
        db.ChatMessages.Add(message);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Conversation> ParticipantConversations(string userId) =>
        db.Conversations.AsNoTracking().Where(c => c.BuyerId == userId || c.SellerId == userId);
}
