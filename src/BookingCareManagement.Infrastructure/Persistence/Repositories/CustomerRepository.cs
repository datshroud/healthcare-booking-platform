using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly UserManager<AppUser> _userManager;

    public CustomerRepository(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<AppUser>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        // Lấy tất cả user có vai trò "Customer"
        return await _userManager.GetUsersInRoleAsync("Customer");
    }

    public async Task<AppUser?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Tìm user theo ID và đảm bảo họ là Customer
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        return await _userManager.IsInRoleAsync(user, "Customer") ? user : null;
    }
}