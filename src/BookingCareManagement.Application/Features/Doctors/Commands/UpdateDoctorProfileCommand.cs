using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Common.Validation;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

public class UpdateDoctorProfileCommand
{
    public Guid DoctorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateDoctorProfileCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public UpdateDoctorProfileCommandHandler(
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task Handle(UpdateDoctorProfileCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        var firstName = InputValidator.SanitizeName(command.FirstName);
        var lastName = InputValidator.SanitizeName(command.LastName);
        var phone = InputValidator.NormalizePhone(command.PhoneNumber);

        if (!InputValidator.IsValidPersonName(firstName) || !InputValidator.IsValidPersonName(lastName))
        {
            throw new ArgumentException("Họ tên không hợp lệ (không chứa số hoặc ký tự đặc biệt).");
        }

        if (!string.IsNullOrWhiteSpace(phone) && !InputValidator.IsValidVietnamPhone(phone))
        {
            throw new ArgumentException("Số điện thoại không hợp lệ (phải đúng 10 số và bắt đầu bằng 0).");
        }

        var user = doctor.AppUser;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
        user.DateOfBirth = command.DateOfBirth;
        user.Description = command.Description;

        if (command.AvatarUrl is not null)
        {
            user.AvatarUrl = string.IsNullOrWhiteSpace(command.AvatarUrl) ? null : command.AvatarUrl;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to update doctor profile: {errorMessage}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
