using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Domain.Abstractions;

public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Doctor doctor);

    // THÊM 3 DÒNG NÀY:
    // Dùng để ĐỌC (READ) - Không theo dõi thay đổi
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Dùng để SỬA/XÓA (UPDATE/DELETE) - Cần theo dõi thay đổi
    Task<Doctor?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Doctor>> GetByIdsWithTrackingAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    void RemoveWorkingHours(IEnumerable<DoctorWorkingHour> workingHours);
    void AddWorkingHours(IEnumerable<DoctorWorkingHour> workingHours);
    void AddDayOff(DoctorDayOff dayOff);
    void RemoveDayOff(DoctorDayOff dayOff);
}