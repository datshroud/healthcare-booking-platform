using System;
using System.Linq;
    using BookingCareManagement.Application.Common.Validation;
    using BookingCareManagement.Application.Features.Customers.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Customers.Commands;

public class CreateCustomerCommand
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class CreateCustomerCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public CreateCustomerCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
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

        var appUser = new AppUser
        {
            FirstName = firstName,
            LastName = lastName,
            FullName = string.IsNullOrWhiteSpace(fullName) ? command.Email : fullName,
            Email = command.Email,
            UserName = command.Email,
            PhoneNumber = command.PhoneNumber,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

            Email = email,
            UserName = email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
        {
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Gán vai trò "Customer"
        await _userManager.AddToRoleAsync(appUser, "Customer");

        return new CustomerDto
        {
            Id = appUser.Id,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            FullName = appUser.FullName ?? appUser.Email ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            PhoneNumber = appUser.PhoneNumber ?? string.Empty,
            CreatedAt = appUser.CreatedAt
        };
    }
            FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
            Email = email,
            PhoneNumber = appUser.PhoneNumber ?? string.Empty,