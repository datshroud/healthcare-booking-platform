using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Notifications.Commands;

public class MarkAllAdminNotificationsReadCommand { }

public class MarkAllAdminNotificationsReadCommandHandler
{
    private readonly IAdminNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAdminNotificationsReadCommandHandler(
        IAdminNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkAllAdminNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
