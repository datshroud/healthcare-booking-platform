using System;
using System.Collections.Generic;

namespace BookingCareManagement.Domain.Aggregates.Appointment;

public static class AppointmentStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Canceled = "Canceled";
    public const string Rejected = "Rejected";
    public const string NoShow = "NoShow";
    public const string PaidTransfer = "PaidTransfer";
    public const string PaidMomo = "PaidMomo";
    public const string PaidVnpay = "PaidVnpay";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Approved,
        Canceled,
        Rejected,
        NoShow,
        PaidTransfer,
        PaidMomo,
        PaidVnpay
    };

    public static bool IsValid(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        return ValidStatuses.Contains(status.Trim());
    }

    public static string NormalizeOrDefault(string? status)
    {
        return IsValid(status) ? status!.Trim() : Pending;
    }
}