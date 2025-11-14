using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Features.Customers.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Customers.Queries;

public class GetAllCustomersQuery { }

public class GetAllCustomersQueryHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerDto>> Handle(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllCustomersAsync(cancellationToken);

        var customerDtos = new List<CustomerDto>();

        foreach (var user in customers)
        {
            customerDtos.Add(new CustomerDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                InternalNote = user.InternalNote,
                CreatedAt = user.CreatedAt,
                // Appointment info not available from this layer without additional abstractions; return defaults
                AppointmentCount = 0,
                LastAppointment = null
            });
        }
        return customerDtos;
    }
}