using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

// 1. Command: Chỉ cần ID để xóa
public class DeleteDoctorCommand
{
    public Guid Id { get; set; }
}

// 2. Handler: Xử lý logic (Hard Delete)
public class DeleteDoctorCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public DeleteDoctorCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task Handle(DeleteDoctorCommand command, CancellationToken cancellationToken)
    {
        // Dùng GetByIdWithTrackingAsync
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.Id} was not found.");
        }

        var appUser = await _userManager.FindByIdAsync(doctor.AppUserId.ToString());
        _doctorRepository.Remove(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (appUser is null)
        {
            return;
        }
        var deleteResult = await _userManager.DeleteAsync(appUser);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to delete associated AppUser. Errors: {errors}");
        }
    }
}
