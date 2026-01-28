using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BookingCareManagement.Application.Common.Validation;

public static class InputValidator
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static string NormalizeName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var previousWasSpace = false;
        foreach (var ch in trimmed)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasSpace)
                {
                    sb.Append(' ');
                    previousWasSpace = true;
                }
                continue;
            }

            sb.Append(ch);
            previousWasSpace = false;
        }

        return sb.ToString();
    }

    public static string SanitizeName(string? name)
    {
        var normalized = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (char.IsLetter(ch) || char.IsWhiteSpace(ch) || ch is '.' or '-' or '\'')
            {
                sb.Append(ch);
            }
        }

        return NormalizeName(sb.ToString());
    }

    public static bool IsValidPersonName(string? name)
    {
        var candidate = NormalizeName(name);
        if (candidate.Length < 2)
        {
            return false;
        }

        foreach (var ch in candidate)
        {
            if (char.IsDigit(ch))
            {
                return false;
            }
        }

        return candidate.All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch) || ch is '.' or '-' or '\'');
    }

    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length == 11)
        {
            digits = "0" + digits.Substring(2);
        }

        return digits;
    }

    public static bool IsValidVietnamPhone(string? phone)
    {
        var digits = NormalizePhone(phone);
        return digits.Length == 10 && digits.StartsWith("0", StringComparison.Ordinal);
    }

    public static string NormalizeEmail(string? email)
    {
        return email?.Trim() ?? string.Empty;
    }

    public static bool IsValidEmail(string? email)
    {
        var candidate = NormalizeEmail(email);
        return !string.IsNullOrWhiteSpace(candidate) && EmailValidator.IsValid(candidate);
    }
}
