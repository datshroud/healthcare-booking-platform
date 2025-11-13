using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Services.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.Service;

namespace BookingCareManagement.Application.Features.Services.Commands;

public class CreateServiceCommand
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInMinutes { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public Guid SpecialtyId { get; set; }
    public IEnumerable<Guid> DoctorIds { get; set; } = new List<Guid>();
}

public class CreateServiceCommandHandler
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(
        IServiceRepository serviceRepository,
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        // 1. Tạo Service
        var service = new Service(
            command.Name,
            command.Price,
            command.DurationInMinutes,
            command.SpecialtyId,
            command.Description,
            command.Color,
            command.ImageUrl
        );

        // 2. Tìm và gán Bác sĩ
        if (command.DoctorIds.Any())
        {
            // Chúng ta không dùng GetByIdsAsync vì nó trả về List<Specialty>
            // Bạn cần thêm hàm GetDoctorsByIds vào IDoctorRepository
            // Tạm thời, chúng ta tìm từng bác sĩ (chậm, nhưng dễ hiểu)
            foreach (var docId in command.DoctorIds)
            {
                var doctor = await _doctorRepository.GetByIdWithTrackingAsync(docId, cancellationToken);
                if (doctor != null)
                {
                    service.AddDoctor(doctor);
                }
            }
        }

        // 3. Lưu
        _serviceRepository.Add(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Trả về DTO
        // (Tạm thời trả về DTO rỗng, vì load DTO đúng rất phức tạp)
        return new ServiceDto { Id = service.Id, Name = service.Name };
    }
}