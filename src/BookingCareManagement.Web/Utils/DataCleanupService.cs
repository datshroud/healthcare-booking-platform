using BookingCareManagement.Application.Common.Validation;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Utils;

public sealed class DataCleanupService
{
    private readonly ApplicationDBContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public DataCleanupService(ApplicationDBContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task NormalizeAsync(CancellationToken cancellationToken = default)
    {
        await NormalizeUsersAsync(cancellationToken);
        await NormalizeAppointmentsAsync(cancellationToken);
    }

    private async Task NormalizeUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.ToListAsync(cancellationToken);
        var changed = false;

        foreach (var user in users)
        {
            var updated = false;

            var firstName = InputValidator.SanitizeName(user.FirstName);
            if (!string.IsNullOrWhiteSpace(firstName) && !string.Equals(firstName, user.FirstName, StringComparison.Ordinal))
            {
                user.FirstName = firstName;
                updated = true;
            }

            var lastName = InputValidator.SanitizeName(user.LastName);
            if (!string.IsNullOrWhiteSpace(lastName) && !string.Equals(lastName, user.LastName, StringComparison.Ordinal))
            {
                user.LastName = lastName;
                updated = true;
            }

            var fullName = InputValidator.SanitizeName(user.FullName);
            if (!string.IsNullOrWhiteSpace(fullName) && !string.Equals(fullName, user.FullName, StringComparison.Ordinal))
            {
                user.FullName = fullName;
                updated = true;
            }

            var phone = InputValidator.NormalizePhone(user.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(phone) && InputValidator.IsValidVietnamPhone(phone))
            {
                if (!string.Equals(phone, user.PhoneNumber, StringComparison.Ordinal))
                {
                    user.PhoneNumber = phone;
                    updated = true;
                }
            }

            if (updated)
            {
                changed = true;
            }
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task NormalizeAppointmentsAsync(CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.Appointments.ToListAsync(cancellationToken);
        var changed = false;

        foreach (var appointment in appointments)
        {
            var updated = false;

            var patientName = InputValidator.SanitizeName(appointment.PatientName);
            var phone = InputValidator.NormalizePhone(appointment.CustomerPhone);

            if (InputValidator.IsValidPersonName(patientName) && InputValidator.IsValidVietnamPhone(phone))
            {
                if (!string.Equals(patientName, appointment.PatientName, StringComparison.Ordinal)
                    || !string.Equals(phone, appointment.CustomerPhone, StringComparison.Ordinal))
                {
                    appointment.UpdatePatientProfile(patientName, phone);
                    updated = true;
                }
            }

            if (updated)
            {
                changed = true;
            }
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
