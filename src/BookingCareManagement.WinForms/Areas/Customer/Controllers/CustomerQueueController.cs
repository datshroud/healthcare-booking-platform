using System;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Customer.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Customer.Controllers;

public sealed class CustomerQueueController
{
    private readonly CustomerQueueViewModel _viewModel;

    public CustomerQueueController(CustomerQueueViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public Task SimulateQueueAsync(CancellationToken cancellationToken = default)
    {
        _viewModel.WaitingCustomers = Random.Shared.Next(0, 25);
        return Task.CompletedTask;
    }
}
