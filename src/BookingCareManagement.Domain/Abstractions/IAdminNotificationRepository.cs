using BookingCareManagement.Domain.Aggregates.Notification;

namespace BookingCareManagement.Domain.Abstractions;

public interface IAdminNotificationRepository
{
    void Add(AdminNotification notification);
    Task<AdminNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminNotification>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<int> CountUnreadAsync(CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}
