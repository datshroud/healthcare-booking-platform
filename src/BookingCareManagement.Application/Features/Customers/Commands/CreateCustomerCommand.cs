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
        var appUser = new AppUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            FullName = $"{command.FirstName} {command.LastName}",
            Email = command.Email,
            UserName = command.Email,
            PhoneNumber = command.PhoneNumber,
            EmailConfirmed = true // Tạm thời xác nhận luôn
            // CreatedAt = DateTime.UtcNow // Gán ngày tạo
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
            FullName = appUser.FullName,
            Email = appUser.Email,
            PhoneNumber = appUser.PhoneNumber,
            // CreatedAt = appUser.CreatedAt
        };
    }
}