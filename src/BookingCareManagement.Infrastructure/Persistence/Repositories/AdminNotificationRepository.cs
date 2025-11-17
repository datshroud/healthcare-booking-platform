using System.Linq;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Notification;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class AdminNotificationRepository : IAdminNotificationRepository
{
    private readonly ApplicationDBContext _context;

    public AdminNotificationRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public void Add(AdminNotification notification)
    {
        _context.AdminNotifications.Add(notification);
    }

    public async Task<AdminNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AdminNotifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminNotification>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        take = take <= 0 ? 10 : Math.Min(take, 100);

        return await _context.AdminNotifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AdminNotifications
            .AsNoTracking()
            .Where(n => !n.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        await _context.AdminNotifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);
    }
}
