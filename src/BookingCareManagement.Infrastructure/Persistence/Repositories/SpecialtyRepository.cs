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

    // Hàm cũ:
    public async Task<List<Specialty>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    // THÊM CÁC HÀM MỚI:

    public async Task<IEnumerable<Specialty>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .Include(s => s.Doctors)
                .ThenInclude(d => d.AppUser)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .Include(s => s.Doctors)
                .ThenInclude(d => d.AppUser)
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Specialty?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Specialties
            .Include(s => s.Doctors)
                .ThenInclude(d => d.AppUser)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public void Add(Specialty specialty)
    {
        _context.Specialties.Add(specialty);
    }

    public void Remove(Specialty specialty)
    {
        _context.Specialties.Remove(specialty);
    }
}
