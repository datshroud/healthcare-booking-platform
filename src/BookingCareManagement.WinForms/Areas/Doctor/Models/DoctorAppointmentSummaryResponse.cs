using System;

namespace BookingCareManagement.WinForms.Areas.Doctor.Models;

public sealed class DoctorAppointmentSummaryResponse
{
    public DateOnly Date { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ApprovedAppointments { get; set; }
    public int CanceledAppointments { get; set; }
    public int RejectedAppointments { get; set; }
    public int NoShowAppointments { get; set; }
}
