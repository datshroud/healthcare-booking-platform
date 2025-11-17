using System.Linq;
using BookingCareManagement.Application.Features.Notifications.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Notifications.Queries;

public class GetRecentAdminNotificationsQuery
{
    public int Take { get; set; } = 10;
}

public class GetRecentAdminNotificationsQueryHandler
{
    private readonly IAdminNotificationRepository _notificationRepository;

    public GetRecentAdminNotificationsQueryHandler(IAdminNotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<AdminNotificationDto>> Handle(GetRecentAdminNotificationsQuery query, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetRecentAsync(query.Take, cancellationToken);
        return notifications.Select(n => n.ToDto()).ToArray();
    }
}
