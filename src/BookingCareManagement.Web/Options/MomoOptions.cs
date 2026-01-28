namespace BookingCareManagement.Web.Options;

public sealed class MomoOptions
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PartnerName { get; set; } = "Test";
    public string StoreId { get; set; } = "MomoTestStore";
    public string RedirectUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
    public string RequestType { get; set; } = "payWithMethod";
    public string PaymentCode { get; set; } = string.Empty;
    public string OrderGroupId { get; set; } = string.Empty;
    public bool AutoCapture { get; set; } = true;
}
