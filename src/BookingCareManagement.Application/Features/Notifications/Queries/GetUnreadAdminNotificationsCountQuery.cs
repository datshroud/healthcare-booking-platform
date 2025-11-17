using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Notifications.Queries;

public class GetUnreadAdminNotificationsCountQuery { }

public class GetUnreadAdminNotificationsCountQueryHandler
{
    private readonly IAdminNotificationRepository _notificationRepository;

    public GetUnreadAdminNotificationsCountQueryHandler(IAdminNotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(GetUnreadAdminNotificationsCountQuery query, CancellationToken cancellationToken)
    {
        return await _notificationRepository.CountUnreadAsync(cancellationToken);
    }
}
