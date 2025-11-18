using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;
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
    private readonly IAppointmentRepository _appointmentRepository;

    public DeleteCustomerCommandHandler(
        UserManager<AppUser> userManager,
        IAppointmentRepository appointmentRepository)
    {
        _userManager = userManager;
        _appointmentRepository = appointmentRepository;
    }

    public async Task Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.Id);
        if (user == null)
        {
            throw new NotFoundException($"Customer with ID {command.Id} not found.");
        }

		var hasAppointments = await _appointmentRepository.HasAppointmentsForPatientAsync(user.Id, cancellationToken);
		if (hasAppointments)
		{
			throw new ValidationException("Không thể xóa khách hàng vì vẫn còn lịch hẹn đã đặt. Hãy hủy hoặc chuyển lịch trước.");
		}

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}