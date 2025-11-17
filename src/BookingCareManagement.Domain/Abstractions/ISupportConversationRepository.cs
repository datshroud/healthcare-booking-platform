using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Domain.Abstractions;

public interface ISupportConversationRepository
{
    Task<SupportConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SupportConversation?> GetByParticipantsAsync(string customerId, string staffId, bool includeMessages = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupportConversation>> GetForCustomerAsync(string customerId, bool includeMessages = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupportConversation>> GetForStaffAsync(string staffId, bool includeMessages = false, CancellationToken cancellationToken = default);
    void Add(SupportConversation conversation);
    Task<IReadOnlyList<SupportMessage>> GetMessagesAsync(Guid conversationId, DateTime? newerThanUtc = null, CancellationToken cancellationToken = default);
    Task AddMessageAsync(SupportMessage message, CancellationToken cancellationToken = default);
}
