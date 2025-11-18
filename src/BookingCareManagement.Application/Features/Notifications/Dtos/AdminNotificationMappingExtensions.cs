using BookingCareManagement.Domain.Aggregates.Notification;

namespace BookingCareManagement.Application.Features.Notifications.Dtos;

public static class AdminNotificationMappingExtensions
{
    public static AdminNotificationDto ToDto(this AdminNotification notification)
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        return new AdminNotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Category = notification.Category,
            IsRead = notification.IsRead,
            AppointmentId = notification.AppointmentId,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }
}
