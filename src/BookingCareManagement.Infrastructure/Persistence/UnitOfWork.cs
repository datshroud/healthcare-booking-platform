using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDBContext _context;

    public UnitOfWork(ApplicationDBContext context)
    {
        _context = context;
    }

    // Triển khai hàm SaveChangesAsync
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
