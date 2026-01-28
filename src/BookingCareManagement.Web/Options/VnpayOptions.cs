namespace BookingCareManagement.Web.Options;

public sealed class VnpayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
    public string OrderType { get; set; } = "other";
}
