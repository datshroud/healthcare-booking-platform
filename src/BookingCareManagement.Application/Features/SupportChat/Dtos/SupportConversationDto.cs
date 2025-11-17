using System;
using System.Collections.Generic;
using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Application.Features.SupportChat.Dtos;

public class SupportConversationDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public SupportConversationStaffRole StaffRole { get; set; }
    public Guid? DoctorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IReadOnlyList<SupportMessageDto> Messages { get; set; } = Array.Empty<SupportMessageDto>();
}
