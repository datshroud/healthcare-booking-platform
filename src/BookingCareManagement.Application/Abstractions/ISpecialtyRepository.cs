using System;

using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Domain.Abstractions;

public interface ISpecialtyRepository
{
    // Hàm này để tìm các chuyên khoa khi chúng ta tạo bác sĩ mới
    Task<List<Specialty>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<Specialty>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
