using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Domain.Abstractions;

public interface ISpecialtyRepository
{
    // Hàm này bạn đã có:
    Task<List<Specialty>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    // THÊM CÁC HÀM MỚI:

    // Dùng cho GET (All)
    Task<IEnumerable<Specialty>> GetAllAsync(CancellationToken cancellationToken = default);

    // Dùng cho GET (by ID) - Bản NoTracking
    Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Dùng cho Update/Delete - Bản CÓ Tracking
    Task<Specialty?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    // Dùng cho POST (Create)
    void Add(Specialty specialty);

    // Dùng cho DELETE
    void Remove(Specialty specialty);
}