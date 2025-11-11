using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Domain.Abstractions;

// Interface này sẽ chịu trách nhiệm gọi "SaveChangesAsync"
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
