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

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Approved,
        Canceled,
        Rejected,
        NoShow
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