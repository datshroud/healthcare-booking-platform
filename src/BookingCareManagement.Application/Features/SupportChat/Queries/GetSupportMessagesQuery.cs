using System.Linq;
using BookingCareManagement.Application.Features.SupportChat.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.SupportChat.Queries;

public class GetSupportMessagesQuery
{
    public Guid ConversationId { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public DateTime? AfterUtc { get; set; }
}

public class GetSupportMessagesQueryHandler
{
    private readonly ISupportConversationRepository _conversationRepository;

    public GetSupportMessagesQueryHandler(ISupportConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<IReadOnlyList<SupportMessageDto>> Handle(GetSupportMessagesQuery query, CancellationToken cancellationToken)
    {
        if (query.ConversationId == Guid.Empty || string.IsNullOrWhiteSpace(query.RequesterId))
        {
            return Array.Empty<SupportMessageDto>();
        }

        var conversation = await _conversationRepository.GetByIdAsync(query.ConversationId, cancellationToken);
        if (conversation is null || !conversation.InvolvesUser(query.RequesterId))
        {
            return Array.Empty<SupportMessageDto>();
        }

        var messages = await _conversationRepository.GetMessagesAsync(query.ConversationId, query.AfterUtc, cancellationToken);
        return messages.Select(m => m.ToDto()).ToArray();
    }
}
