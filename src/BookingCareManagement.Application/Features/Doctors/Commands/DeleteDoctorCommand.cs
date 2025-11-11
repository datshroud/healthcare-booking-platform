using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

// 1. Command: Chỉ cần ID để xóa
public class DeleteDoctorCommand
{
    public Guid Id { get; set; }
}

// 2. Handler: Xử lý logic (Soft Delete)
public class DeleteDoctorCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDoctorCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDoctorCommand command, CancellationToken cancellationToken)
    {
        // Dùng GetByIdWithTrackingAsync
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.Id} was not found.");
        }

        // Thay vì XÓA HẲN, chúng ta "Soft Delete"
        doctor.Deactivate();

        // Lưu thay đổi
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
