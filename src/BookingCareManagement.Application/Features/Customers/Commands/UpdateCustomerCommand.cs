using System;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
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
        var firstName = command.FirstName?.Trim() ?? string.Empty;
        var lastName = command.LastName?.Trim() ?? string.Empty;
        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

        user.FirstName = firstName;
        user.LastName = lastName;
        user.FullName = string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
        user.Email = command.Email;
        user.UserName = command.Email;
        user.PhoneNumber = command.PhoneNumber;
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