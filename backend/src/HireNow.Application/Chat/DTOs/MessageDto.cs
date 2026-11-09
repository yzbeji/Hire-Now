namespace HireNow.Application.Chat;

public sealed record MessageDto(Guid Id,
                                Guid ConversationId,
                                string SenderId,
                                string Content,
                                DateTime SentAt)
{
    public static MessageDto From(ChatMessage message) =>
        new(message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc));
}
