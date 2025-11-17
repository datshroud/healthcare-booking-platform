using System;
using System.Linq;
using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Application.Features.SupportChat.Dtos;

public static class SupportChatMappingExtensions
{
    public static SupportMessageDto ToDto(this SupportMessage message)
    {
        return new SupportMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            Content = message.Content,
            Author = message.Author,
            CreatedAtUtc = message.CreatedAtUtc
        };
    }

    public static SupportConversationDto ToDto(this SupportConversation conversation)
    {
        var orderedMessages = conversation.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .ThenBy(m => m.Id)
            .Select(m => m.ToDto())
            .ToArray();

        return new SupportConversationDto
        {
            Id = conversation.Id,
            CustomerId = conversation.CustomerId,
            StaffId = conversation.StaffId,
            StaffRole = conversation.StaffRole,
            DoctorId = conversation.DoctorId,
            Subject = conversation.Subject,
            IsClosed = conversation.IsClosed,
            CreatedAtUtc = conversation.CreatedAtUtc,
            UpdatedAtUtc = conversation.UpdatedAtUtc,
            Messages = orderedMessages
        };
    }
}
