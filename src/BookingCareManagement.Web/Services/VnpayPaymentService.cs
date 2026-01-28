using System.Net;
using System.Security.Cryptography;
using System.Text;
using BookingCareManagement.Web.Options;
using Microsoft.Extensions.Options;

namespace BookingCareManagement.Web.Services;

public sealed class VnpayPaymentService
{
    private readonly VnpayOptions _options;

    public VnpayPaymentService(IOptions<VnpayOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.TmnCode)
        && !string.IsNullOrWhiteSpace(_options.HashSecret)
        && !string.IsNullOrWhiteSpace(_options.ReturnUrl)
        && !string.IsNullOrWhiteSpace(_options.PaymentUrl);

    public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string ipAddress)
    {
        var amountValue = decimal.Round(amount, 0, MidpointRounding.AwayFromZero) * 100;
        var vnpAmount = amountValue.ToString("0");

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = _options.Command,
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = vnpAmount,
            ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = _options.CurrCode,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_Locale"] = _options.Locale,
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = _options.OrderType,
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_TxnRef"] = orderId
        };

        var hashData = BuildQuery(parameters, encode: true);
        var secureHash = Sign(hashData, _options.HashSecret);

        var query = BuildQuery(parameters, encode: true);
        return $"{_options.PaymentUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateSignature(IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("vnp_SecureHash", out var secureHash))
        {
            return false;
        }

        var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in parameters)
        {
            if (string.Equals(key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            data[key] = value;
        }

        var hashData = BuildQuery(data, encode: true);
        var expected = Sign(hashData, _options.HashSecret);
        return string.Equals(expected, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildQuery(SortedDictionary<string, string> data, bool encode)
    {
        var builder = new StringBuilder();
        foreach (var (key, value) in data)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(key).Append('=').Append(encode ? UrlEncode(value) : value);
        }

        return builder.ToString();
    }

    private static string Sign(string raw, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var rawBytes = Encoding.UTF8.GetBytes(raw);
        using var hmac = new HMACSHA512(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(rawBytes)).ToLowerInvariant();
    }

    private static string UrlEncode(string value)
    {
        return WebUtility.UrlEncode(value ?? string.Empty).Replace("%20", "+");
    }
}
