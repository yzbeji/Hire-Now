namespace HireNow.Application.Chat;

public sealed record ConversationDto(Guid Id,
                                    string BuyerId,
                                    string SellerId,
                                    string? ParticipantName = null)
{
    public static ConversationDto From(Conversation conversation, string? participantName = null) =>
        new(conversation.Id,
            conversation.BuyerId,
            conversation.SellerId,
            participantName);
}
