using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BookingCareManagement.Web.Options;
using Microsoft.Extensions.Options;

namespace BookingCareManagement.Web.Services;

public sealed class MomoPaymentService
{
    private readonly HttpClient _http;
    private readonly MomoOptions _options;

    public MomoPaymentService(HttpClient http, IOptions<MomoOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.PartnerCode)
        && !string.IsNullOrWhiteSpace(_options.AccessKey)
        && !string.IsNullOrWhiteSpace(_options.SecretKey)
        && !string.IsNullOrWhiteSpace(_options.RedirectUrl)
        && !string.IsNullOrWhiteSpace(_options.IpnUrl);

    public async Task<MomoCreateResult?> CreateQrPaymentAsync(string orderId, decimal amount, string orderInfo, string extraData = "")
    {
        if (!IsConfigured)
        {
            return null;
        }

        var requestId = orderId;
        var amountStr = decimal.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("0");
        extraData ??= string.Empty;
        orderInfo ??= "Thanh toan lich hen";
        orderInfo = orderInfo.Replace("#", string.Empty);
        var endpoint = _options.Endpoint ?? string.Empty;
        var requestType = string.IsNullOrWhiteSpace(_options.RequestType)
            ? "payWithMethod"
            : _options.RequestType;

        var rawSignature =
            $"accessKey={_options.AccessKey}" +
            $"&amount={amountStr}" +
            $"&extraData={extraData}" +
            $"&ipnUrl={_options.IpnUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&partnerCode={_options.PartnerCode}" +
            $"&redirectUrl={_options.RedirectUrl}" +
            $"&requestId={requestId}" +
            $"&requestType={requestType}";

        var signature = Sign(rawSignature, _options.SecretKey);

        var payload = new
        {
            partnerCode = _options.PartnerCode,
            partnerName = _options.PartnerName,
            storeId = _options.StoreId,
            accessKey = _options.AccessKey,
            requestId,
            amount = amountStr,
            orderId,
            orderInfo,
            redirectUrl = _options.RedirectUrl,
            ipnUrl = _options.IpnUrl,
            lang = "vi",
            requestType,
            autoCapture = _options.AutoCapture,
            extraData,
            orderGroupId = _options.OrderGroupId,
            paymentCode = string.IsNullOrWhiteSpace(_options.PaymentCode) ? null : _options.PaymentCode,
            signature
        };

        using var resp = await _http.PostAsJsonAsync(endpoint, payload);
        var body = await resp.Content.ReadAsStringAsync();

        return ParseResponse(body, resp.IsSuccessStatusCode, resp);
    }

    private static string Sign(string raw, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var rawBytes = Encoding.UTF8.GetBytes(raw);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(rawBytes)).ToLowerInvariant();
    }

    public sealed record MomoCreateResult(bool Success, string? PayUrl, string? Message, string? RawResponse);

    private static string? GetString(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int? GetInt(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
    }

    private static MomoCreateResult ParseResponse(string body, bool isSuccessStatus, HttpResponseMessage resp)
    {
        if (!isSuccessStatus)
        {
            return new MomoCreateResult(false, null, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}", body);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return new MomoCreateResult(false, null, "Phản hồi MoMo rỗng", body);
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var payUrl = GetString(root, "payUrl")
                ?? GetString(root, "deeplink")
                ?? GetString(root, "qrCodeUrl");

            var message = GetString(root, "message") ?? GetString(root, "localMessage");
            var resultCode = GetInt(root, "resultCode");
            var errorCode = GetInt(root, "errorCode");

            var success = (resultCode.HasValue && resultCode.Value == 0)
                || (errorCode.HasValue && errorCode.Value == 0)
                || !string.IsNullOrWhiteSpace(payUrl);

            return new MomoCreateResult(success, payUrl, message, body);
        }
        catch (JsonException)
        {
            return new MomoCreateResult(false, null, "Không parse được phản hồi MoMo", body);
        }
    }
}
