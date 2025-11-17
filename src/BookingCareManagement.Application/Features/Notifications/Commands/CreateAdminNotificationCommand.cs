using BookingCareManagement.Application.Features.Notifications.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Notification;

namespace BookingCareManagement.Application.Features.Notifications.Commands;

public class CreateAdminNotificationCommand
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public Guid? AppointmentId { get; set; }
}

public class CreateAdminNotificationCommandHandler
{
    private readonly IAdminNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdminNotificationCommandHandler(
        IAdminNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminNotificationDto> Handle(CreateAdminNotificationCommand command, CancellationToken cancellationToken)
    {
        var notification = new AdminNotification(
            command.Title,
            command.Message,
            command.Category,
            command.AppointmentId);

        _notificationRepository.Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return notification.ToDto();
    }
}
