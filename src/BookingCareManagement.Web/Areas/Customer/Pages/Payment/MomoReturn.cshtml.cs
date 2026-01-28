using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Customer.Pages.Payment;

[AllowAnonymous]
public sealed class MomoReturnModel : PageModel
{
    public string OrderId { get; private set; } = string.Empty;
    public int ResultCode { get; private set; }

    public void OnGet(string? orderId, int? resultCode)
    {
        OrderId = orderId ?? string.Empty;
        ResultCode = resultCode ?? -1;
    }
}
