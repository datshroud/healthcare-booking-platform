using System;

namespace BookingCareManagement.Domain.Aggregates.Notification;

public class AdminNotification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public string Message { get; private set; }
    public string Category { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public Guid? AppointmentId { get; private set; }

    private AdminNotification()
    {
        Title = string.Empty;
        Message = string.Empty;
        Category = "general";
    }

    public AdminNotification(
        string title,
        string message,
        string category = "general",
        Guid? appointmentId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message is required", nameof(message));
        }

        Title = title.Trim();
        Message = message.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim();
        AppointmentId = appointmentId;
    }

    public void AttachAppointment(Guid appointmentId)
    {
        AppointmentId = appointmentId;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
        }
    }
}
