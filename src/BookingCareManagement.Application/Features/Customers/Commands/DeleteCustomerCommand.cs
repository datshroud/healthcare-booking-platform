using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Customers.Commands;

public class DeleteCustomerCommand
{
    public string Id { get; set; } = string.Empty; // User ID
}

public class DeleteCustomerCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public DeleteCustomerCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.Id);
        if (user == null)
        {
            throw new NotFoundException($"Customer with ID {command.Id} not found.");
        }

        // TODO: Cần kiểm tra xem khách hàng có lịch hẹn (Appointments) không
        // Nếu có, không nên xóa vĩnh viễn?

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}