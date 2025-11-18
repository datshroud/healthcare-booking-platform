using System;
using System.Collections.Generic;
using DoctorAggregate = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;
using BookingCareManagement.Domain.Aggregates.User;

namespace BookingCareManagement.Domain.Aggregates.SupportChat;

public class SupportConversation
{
    private readonly List<SupportMessage> _messages = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string CustomerId { get; private set; } = null!;
    public AppUser Customer { get; private set; } = null!;
    public string StaffId { get; private set; } = null!;
    public AppUser Staff { get; private set; } = null!;
    public SupportConversationStaffRole StaffRole { get; private set; }
    public Guid? DoctorId { get; private set; }
    public DoctorAggregate? Doctor { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
    public bool IsClosed { get; private set; }
    public string Subject { get; private set; } = "general";

    public IReadOnlyCollection<SupportMessage> Messages => _messages;

    private SupportConversation() { }

    public SupportConversation(
        string customerId,
        string staffId,
        SupportConversationStaffRole staffRole,
        Guid? doctorId = null,
        string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(staffId))
        {
            throw new ArgumentException("Staff id is required.", nameof(staffId));
        }

        if (staffRole == SupportConversationStaffRole.Doctor && (!doctorId.HasValue || doctorId == Guid.Empty))
        {
            throw new ArgumentException("Doctor conversations require a doctor id.", nameof(doctorId));
        }

        CustomerId = customerId;
        StaffId = staffId;
        StaffRole = staffRole;
        DoctorId = doctorId;
        Subject = string.IsNullOrWhiteSpace(subject) ? "general" : subject.Trim();
    }

    public SupportMessage AddMessage(string content, SupportMessageAuthor author, string? metadata = null)
    {
        var message = SupportMessage.Create(Id, content, author, metadata);
        _messages.Add(message);
        UpdatedAtUtc = message.CreatedAtUtc;
        return message;
    }

    public void AttachCustomer(AppUser user)
    {
        Customer = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void AttachStaff(AppUser user)
    {
        Staff = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void AttachDoctor(DoctorAggregate doctor)
    {
        if (doctor == null)
        {
            throw new ArgumentNullException(nameof(doctor));
        }

        if (doctor.Id != DoctorId)
        {
            throw new InvalidOperationException("Doctor mismatch while attaching conversation doctor.");
        }

        Doctor = doctor;
    }

    public void Close()
    {
        IsClosed = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsClosed = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool InvolvesUser(string appUserId)
    {
        if (string.IsNullOrWhiteSpace(appUserId))
        {
            return false;
        }

        return string.Equals(appUserId, CustomerId, StringComparison.Ordinal) ||
               string.Equals(appUserId, StaffId, StringComparison.Ordinal);
    }

    public SupportMessageAuthor ResolveAuthor(string appUserId)
    {
        if (string.Equals(appUserId, CustomerId, StringComparison.Ordinal))
        {
            return SupportMessageAuthor.User;
        }

        if (string.Equals(appUserId, StaffId, StringComparison.Ordinal))
        {
            return SupportMessageAuthor.Agent;
        }

        throw new InvalidOperationException("Sender is not part of this conversation.");
    }
}
