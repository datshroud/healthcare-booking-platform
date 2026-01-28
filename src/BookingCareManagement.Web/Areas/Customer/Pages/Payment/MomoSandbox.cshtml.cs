using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Customer.Pages.Payment;

[AllowAnonymous]
public sealed class MomoSandboxModel : PageModel
{
    public string Token { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Method { get; private set; } = "momo";
    public string OrderCode { get; private set; } = string.Empty;

    public string AmountDisplay => Amount.ToString("N0") + " VNĐ";
    public string MethodDisplay => Method.Equals("vnpay", System.StringComparison.OrdinalIgnoreCase)
        ? "VNPay (sandbox)"
        : "MoMo (sandbox)";

    public void OnGet(string? token, decimal? amount, string? method)
    {
        Token = token ?? string.Empty;
        Amount = amount ?? 0m;
        Method = string.IsNullOrWhiteSpace(method) ? "momo" : method.Trim().ToLowerInvariant();
        OrderCode = string.IsNullOrWhiteSpace(Token)
            ? $"MOMO{DateTime.UtcNow:yyyyMMddHHmmss}"
            : $"MOMO{Token.Substring(0, Math.Min(8, Token.Length)).ToUpperInvariant()}";
    }
}
