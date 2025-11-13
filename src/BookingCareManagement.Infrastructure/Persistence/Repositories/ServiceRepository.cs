using BookingCareManagement.Application.Abstractions; // Sửa namespace
using BookingCareManagement.Domain.Aggregates.Service;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Thêm
using System.Linq; // Thêm
using System.Threading; // Thêm
using System.Threading.Tasks; // Thêm

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class ServiceRepository : IServiceRepository // Đảm bảo : IServiceRepository
{
    private readonly ApplicationDBContext _context;

    public ServiceRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    // 1. Hàm cũ
    public async Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Services
            // SỬA Ở ĐÂY:
            .Include(s => s.Specialty) // Gộp "Danh mục"
            .Include(s => s.Doctors)   // Gộp danh sách "Bác sĩ"
                .ThenInclude(d => d.AppUser) // VỚI MỖI BÁC SĨ, gộp "AppUser" của họ
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // 2. THÊM MỚI
    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Doctors)
            .Include(s => s.Specialty)
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // 3. THÊM MỚI
    public async Task<Service?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Doctors)
            .Include(s => s.Specialty)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // 4. THÊM MỚI
    public void Add(Service service)
    {
        _context.Services.Add(service);
    }

    // 5. THÊM MỚI
    public void Remove(Service service)
    {
        _context.Services.Remove(service);
    }

    public async Task<List<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}