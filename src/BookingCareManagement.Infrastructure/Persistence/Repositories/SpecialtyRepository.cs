using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class SpecialtyRepository : ISpecialtyRepository
{
    private readonly ApplicationDBContext _context;

    public SpecialtyRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<List<Specialty>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        // Tìm tất cả chuyên khoa có ID nằm trong danh sách
        return await _context.Specialties
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Specialty>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
