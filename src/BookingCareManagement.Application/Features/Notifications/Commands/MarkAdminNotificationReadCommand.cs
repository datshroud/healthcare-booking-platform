using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Notifications.Commands;

public class MarkAdminNotificationReadCommand
{
    public Guid Id { get; set; }
}

public class MarkAdminNotificationReadCommandHandler
{
    private readonly IAdminNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAdminNotificationReadCommandHandler(
        IAdminNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkAdminNotificationReadCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Id thông báo không hợp lệ.");
        }

        var notification = await _notificationRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy thông báo.");

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
