using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Features.Customers.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Customers.Queries;

public class GetAllCustomersQuery { }

public class GetAllCustomersQueryHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAllCustomersQueryHandler(
        ICustomerRepository customerRepository,
        IAppointmentRepository appointmentRepository)
    {
        _customerRepository = customerRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IEnumerable<CustomerDto>> Handle(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllCustomersAsync(cancellationToken);
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);

        var appointmentStats = appointments
            .Where(a => !string.IsNullOrWhiteSpace(a.PatientId))
            .GroupBy(a => a.PatientId!)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Count = g.Count(),
                    Last = g.Max(a => a.StartUtc)
                });

        var customerDtos = new List<CustomerDto>();

        foreach (var user in customers)
        {
            var firstName = user.FirstName?.Trim() ?? string.Empty;
            var lastName = user.LastName?.Trim() ?? string.Empty;
            var composedName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var fullName = string.IsNullOrWhiteSpace(composedName)
                ? (user.FullName?.Trim() ?? string.Empty)
                : composedName;

            appointmentStats.TryGetValue(user.Id, out var stats);

            customerDtos.Add(new CustomerDto
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                InternalNote = user.InternalNote,
                CreatedAt = user.CreatedAt,
                AppointmentCount = stats?.Count ?? 0,
                LastAppointment = stats?.Last
            });
        }
        return customerDtos;
    }
}