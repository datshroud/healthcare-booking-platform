using System;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Common.Validation;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Customers.Commands;

public class UpdateCustomerCommand
{
    public string Id { get; set; } = string.Empty; // User ID
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? InternalNote { get; set; }
}

public class UpdateCustomerCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public UpdateCustomerCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.Id);
        if (user == null)
        {
            throw new NotFoundException($"Customer with ID {command.Id} not found.");
        }

        // Cập nhật thông tin
        var firstName = InputValidator.SanitizeName(command.FirstName);
        var lastName = InputValidator.SanitizeName(command.LastName);
        var email = InputValidator.NormalizeEmail(command.Email);
        var phone = InputValidator.NormalizePhone(command.PhoneNumber);

        if (!InputValidator.IsValidPersonName(firstName) || !InputValidator.IsValidPersonName(lastName))
        {
            throw new ArgumentException("Họ tên không hợp lệ (không chứa số hoặc ký tự đặc biệt).");
        }

        if (!InputValidator.IsValidEmail(email))
        {
            throw new ArgumentException("Email không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(phone) && !InputValidator.IsValidVietnamPhone(phone))
        {
            throw new ArgumentException("Số điện thoại không hợp lệ (phải đúng 10 số và bắt đầu bằng 0).");
        }

        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

        user.FirstName = firstName;
        user.LastName = lastName;
        user.FullName = string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
        user.Gender = command.Gender;
        user.DateOfBirth = command.DateOfBirth;
        user.InternalNote = command.InternalNote;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}