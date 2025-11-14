using BookingCareManagement.Domain.Aggregates.User;

namespace BookingCareManagement.Application.Abstractions;

public interface ICustomerRepository
{
    // Lấy tất cả user có vai trò "Customer"
    Task<IEnumerable<AppUser>> GetAllCustomersAsync(CancellationToken cancellationToken = default);

    // Tìm Customer (AppUser) bằng ID
    Task<AppUser?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);
}