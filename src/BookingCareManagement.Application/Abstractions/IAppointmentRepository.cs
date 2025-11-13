using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.Appointment;

namespace BookingCareManagement.Domain.Abstractions;

public interface IAppointmentRepository
{
	Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Appointment?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
	void Add(Appointment appointment);
	void Remove(Appointment appointment);
}
