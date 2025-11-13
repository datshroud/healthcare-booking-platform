using System;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

public record CreateDoctorDayOffCommand(Guid DoctorId, string Name, DateOnly StartDate, DateOnly EndDate, bool RepeatYearly);

public class CreateDoctorDayOffCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDoctorDayOffCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDoctorDayOffCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        var dayOff = doctor.AddDayOff(command.Name, command.StartDate, command.EndDate, command.RepeatYearly);
        _doctorRepository.AddDayOff(dayOff);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dayOff.Id;
    }
}

public record UpdateDoctorDayOffCommand(Guid DoctorId, Guid DayOffId, string Name, DateOnly StartDate, DateOnly EndDate, bool RepeatYearly);

public class UpdateDoctorDayOffCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDoctorDayOffCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDoctorDayOffCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        var dayOff = doctor.FindDayOff(command.DayOffId);
        if (dayOff is null)
        {
            throw new NotFoundException($"Day off with ID {command.DayOffId} was not found.");
        }

        doctor.UpdateDayOff(command.DayOffId, command.Name, command.StartDate, command.EndDate, command.RepeatYearly);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteDoctorDayOffCommand(Guid DoctorId, Guid DayOffId);

public class DeleteDoctorDayOffCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDoctorDayOffCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDoctorDayOffCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        var dayOff = doctor.FindDayOff(command.DayOffId);
        if (dayOff is null)
        {
            throw new NotFoundException($"Day off with ID {command.DayOffId} was not found.");
        }

        doctor.RemoveDayOff(command.DayOffId);
        _doctorRepository.RemoveDayOff(dayOff);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
