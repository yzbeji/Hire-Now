public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public string SenderId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; }
}