using System;
using System.Text.Json.Serialization;
using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Application.Features.SupportChat.Dtos;

public class SupportMessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SupportMessageAuthor Author { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
