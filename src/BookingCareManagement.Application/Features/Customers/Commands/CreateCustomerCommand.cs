using System;
using System.Linq;
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
        var firstName = command.FirstName?.Trim() ?? string.Empty;
        var lastName = command.LastName?.Trim() ?? string.Empty;
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

        // Mật khẩu tạm thời
        var result = await _userManager.CreateAsync(appUser, "Customer123!");
        if (!result.Succeeded)
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
}