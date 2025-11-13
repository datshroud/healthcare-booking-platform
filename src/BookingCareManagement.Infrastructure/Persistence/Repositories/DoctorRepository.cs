using System.Linq;
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
            .Include(d => d.AppUser)
            .Include(d => d.Specialties)
            .Include(d => d.WorkingHours)
            .Include(d => d.DaysOff)
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
            .Include(d => d.AppUser)
            .Include(d => d.Specialties)
            .Include(d => d.WorkingHours)
            .Include(d => d.DaysOff)
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    // Bản CÓ Tracking (bỏ AsNoTracking) để SỬA/XÓA
    public async Task<Doctor?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.AppUser)
            .Include(d => d.Specialties)
            .Include(d => d.WorkingHours)
            .Include(d => d.DaysOff)
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public void RemoveWorkingHours(IEnumerable<DoctorWorkingHour> workingHours)
    {
        if (workingHours is null)
        {
            return;
        }

        _context.Set<DoctorWorkingHour>().RemoveRange(workingHours);
    }

    public void AddWorkingHours(IEnumerable<DoctorWorkingHour> workingHours)
    {
        if (workingHours is null)
        {
            return;
        }

        var toAdd = workingHours.Where(h => h is not null).ToArray();
        if (toAdd.Length == 0)
        {
            return;
        }

        _context.Set<DoctorWorkingHour>().AddRange(toAdd);
    }

    public void AddDayOff(DoctorDayOff dayOff)
    {
        if (dayOff is null)
        {
            return;
        }

        _context.Set<DoctorDayOff>().Add(dayOff);
    }

    public void RemoveDayOff(DoctorDayOff dayOff)
    {
        if (dayOff is null)
        {
            return;
        }

        _context.Set<DoctorDayOff>().Remove(dayOff);
    }
}