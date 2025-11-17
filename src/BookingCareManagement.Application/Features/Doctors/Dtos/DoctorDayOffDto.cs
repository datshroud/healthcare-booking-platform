namespace BookingCareManagement.Application.Features.Doctors.Dtos;

public class DoctorDayOffDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool RepeatYearly { get; set; }
    public DateOnly? DisplayStart { get; set; }
    public DateOnly? DisplayEnd { get; set; }
}
