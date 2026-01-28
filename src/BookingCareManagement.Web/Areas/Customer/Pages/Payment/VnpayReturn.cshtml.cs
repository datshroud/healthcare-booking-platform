using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingCareManagement.Web.Services;

namespace BookingCareManagement.Areas.Customer.Pages.Payment;

[AllowAnonymous]
public sealed class VnpayReturnModel : PageModel
{
    public string OrderId { get; private set; } = string.Empty;
    public string ResponseCode { get; private set; } = string.Empty;
    public bool SignatureValid { get; private set; }

    private readonly VnpayPaymentService _vnpay;

    public VnpayReturnModel(VnpayPaymentService vnpay)
    {
        _vnpay = vnpay;
    }

    public void OnGet()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in Request.Query)
        {
            parameters[key] = value.ToString();
        }

        OrderId = parameters.TryGetValue("vnp_TxnRef", out var txnRef) ? txnRef : string.Empty;
        ResponseCode = parameters.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        SignatureValid = _vnpay.ValidateSignature(parameters);
    }
}
