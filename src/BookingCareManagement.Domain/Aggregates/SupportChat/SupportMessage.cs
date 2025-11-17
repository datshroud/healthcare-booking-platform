using System;

namespace BookingCareManagement.Domain.Aggregates.SupportChat;

public class SupportMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ConversationId { get; private set; }
    public SupportConversation Conversation { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public SupportMessageAuthor Author { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public bool SeenByAgent { get; private set; }
    public DateTime? SeenAtUtc { get; private set; }
    public string? Metadata { get; private set; }

    private SupportMessage() { }

    private SupportMessage(Guid conversationId, string content, SupportMessageAuthor author, string? metadata)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.", nameof(content));
        }

        ConversationId = conversationId;
        Content = content.Trim();
        Author = author;
        Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
    }

    public static SupportMessage Create(Guid conversationId, string content, SupportMessageAuthor author, string? metadata = null)
    {
        return new SupportMessage(conversationId, content, author, metadata);
    }

    public void MarkSeen()
    {
        if (SeenByAgent)
        {
            return;
        }

        SeenByAgent = true;
        SeenAtUtc = DateTime.UtcNow;
    }
}
