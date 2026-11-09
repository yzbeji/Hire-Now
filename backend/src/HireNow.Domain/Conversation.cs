public class Conversation
{
    public Guid Id { get; set; }

    public string BuyerId { get; set; } = null!;

    public string SellerId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}