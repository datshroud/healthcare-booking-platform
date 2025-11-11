using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDBContext _context;

    public DoctorRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.AppUser) // <-- THÊM DÒNG NÀY
            .Include(d => d.Specialties)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
    }

    // Bản AsNoTracking() để ĐỌC
    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.AppUser) // <-- THÊM DÒNG NÀY
            .Include(d => d.Specialties)
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    // Bản CÓ Tracking (bỏ AsNoTracking) để SỬA/XÓA
    public async Task<Doctor?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.AppUser) // <-- THÊM DÒNG NÀY
            .Include(d => d.Specialties)
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}