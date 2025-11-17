using System;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Appointment;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDBContext _context;

    public AppointmentRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .OrderBy(a => a.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Appointment?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public void Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
    }

    public void Remove(Appointment appointment)
    {
        _context.Appointments.Remove(appointment);
    }

    public async Task<bool> HasAppointmentsForPatientAsync(string patientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(patientId))
        {
            return false;
        }

        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a => a.PatientId == patientId, cancellationToken);
    }

    public async Task<bool> CustomerHasAppointmentWithDoctorAsync(string patientId, Guid doctorId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(patientId) || doctorId == Guid.Empty)
        {
            return false;
        }

        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId, cancellationToken);
    }
}
