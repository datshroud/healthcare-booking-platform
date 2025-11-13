using BookingCareManagement.Domain.Aggregates.Service;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Abstractions;

public interface IServiceRepository
{
    // 1. Hàm cũ (Dùng cho GET All)
    Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default);

    // 2. THÊM MỚI: Dùng cho GET (by ID) - Bản NoTracking
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // 3. THÊM MỚI: Dùng cho Update/Delete - Bản CÓ Tracking
    Task<Service?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    // 4. THÊM MỚI: Dùng cho POST (Create)
    void Add(Service service);

    // 5. THÊM MỚI: Dùng cho DELETE
    void Remove(Service service);
    Task<List<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}