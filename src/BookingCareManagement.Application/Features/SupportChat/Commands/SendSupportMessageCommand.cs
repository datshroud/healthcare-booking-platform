using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.SupportChat.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Application.Features.SupportChat.Commands;

public class SendSupportMessageCommand
{
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SendSupportMessageCommandHandler
{
    private const int MaxMessageLength = 1000;

    private readonly ISupportConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendSupportMessageCommandHandler(
        ISupportConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupportMessageDto> Handle(SendSupportMessageCommand command, CancellationToken cancellationToken)
    {
        if (command.ConversationId == Guid.Empty)
        {
            throw new ValidationException("Thiếu mã cuộc trò chuyện.");
        }

        if (string.IsNullOrWhiteSpace(command.SenderId))
        {
            throw new ValidationException("Không thể xác định người gửi tin nhắn.");
        }

        var trimmedContent = command.Content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            throw new ValidationException("Vui lòng nhập nội dung tin nhắn.");
        }

        if (trimmedContent.Length > MaxMessageLength)
        {
            throw new ValidationException($"Tin nhắn tối đa {MaxMessageLength} ký tự.");
        }

        var conversation = await GetConversationAsync(command.ConversationId, cancellationToken);

        if (!conversation.InvolvesUser(command.SenderId))
        {
            throw new ValidationException("Bạn không thuộc cuộc trò chuyện này.");
        }

        var author = conversation.ResolveAuthor(command.SenderId);
        var message = conversation.AddMessage(trimmedContent, author);
        await _conversationRepository.AddMessageAsync(message, cancellationToken);
        conversation.Touch();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return message.ToDto();
    }

    private async Task<SupportConversation> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
                           ?? throw new ValidationException("Cuộc trò chuyện không tồn tại.");

        if (conversation.IsClosed)
        {
            conversation.Reopen();
        }

        return conversation;
    }
}
