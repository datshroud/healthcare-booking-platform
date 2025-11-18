using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.SupportChat.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.SupportChat;

namespace BookingCareManagement.Application.Features.SupportChat.Commands;

public class EnsureSupportConversationCommand
{
    public string CustomerId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public SupportConversationStaffRole StaffRole { get; set; }
    public Guid? DoctorId { get; set; }
    public string? Subject { get; set; }
    public bool IncludeMessages { get; set; } = true;
}

public class EnsureSupportConversationCommandHandler
{
    private readonly ISupportConversationRepository _conversationRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnsureSupportConversationCommandHandler(
        ISupportConversationRepository conversationRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupportConversationDto> Handle(EnsureSupportConversationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerId))
        {
            throw new ValidationException("Thiếu thông tin khách hàng.");
        }

        if (string.IsNullOrWhiteSpace(command.StaffId))
        {
            throw new ValidationException("Thiếu thông tin người nhận.");
        }

        if (command.StaffRole == SupportConversationStaffRole.Doctor)
        {
            await EnsureDoctorConversationIsAllowed(command, cancellationToken);
        }

        var conversation = await _conversationRepository.GetByParticipantsAsync(
            command.CustomerId,
            command.StaffId,
            includeMessages: command.IncludeMessages,
            cancellationToken);

        if (conversation is null)
        {
            conversation = new SupportConversation(
                command.CustomerId,
                command.StaffId,
                command.StaffRole,
                command.DoctorId,
                command.Subject);

            _conversationRepository.Add(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return conversation.ToDto();
    }

    private async Task EnsureDoctorConversationIsAllowed(EnsureSupportConversationCommand command, CancellationToken cancellationToken)
    {
        var doctorId = command.DoctorId ?? throw new ValidationException("Thiếu mã bác sĩ.");

        var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken)
                     ?? throw new ValidationException("Không tìm thấy bác sĩ.");

        if (!string.Equals(doctor.AppUserId, command.StaffId, StringComparison.Ordinal))
        {
            throw new ValidationException("Thông tin bác sĩ không hợp lệ.");
        }

        var hasAppointment = await _appointmentRepository.CustomerHasAppointmentWithDoctorAsync(command.CustomerId, doctorId, cancellationToken);
        if (!hasAppointment)
        {
            throw new ValidationException("Bạn chưa có cuộc hẹn với bác sĩ này.");
        }
    }
}
