using System;
using System.Collections.Generic;
using System.Linq;
using BookingCareManagement.Domain.Aggregates.User; // Cần cho AppUser

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class Doctor
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public bool Active { get; private set; } = true;
    public bool LimitAppointments { get; private set; }

    // 1. Thêm khóa ngoại trỏ đến AppUser
    public string AppUserId { get; private set; } = null!;

    // 2. Thêm thuộc tính điều hướng (navigation property)
    public AppUser AppUser { get; private set; } = null!;

    // 3. Quan hệ nhiều-nhiều với Specialty (giữ nguyên)
    private readonly List<Specialty> _specialties = new();
    public IReadOnlyCollection<Specialty> Specialties => _specialties;

    private readonly List<DoctorWorkingHour> _workingHours = new();
    public IReadOnlyCollection<DoctorWorkingHour> WorkingHours => _workingHours;

    private readonly List<DoctorDayOff> _daysOff = new();
    public IReadOnlyCollection<DoctorDayOff> DaysOff => _daysOff;

    // Constructor rỗng cho EF Core
    private Doctor() { }

    // Constructor mới: Một Doctor PHẢI được tạo từ một AppUserId
    public Doctor(string appUserId)
    {
        AppUserId = appUserId;
    }

    // Giữ nguyên logic AddSpecialty
    public void AddSpecialty(Specialty s)
    {
        if (_specialties.All(x => x.Id != s.Id)) _specialties.Add(s);
    }

    // Giữ nguyên logic ClearSpecialties
    public void ClearSpecialties()
    {
        _specialties.Clear();
    }

    // Giữ nguyên logic Deactivate
    public void Deactivate()
    {
        Active = false;
    }

    public void SetAppointmentLimit(bool limitAppointments)
    {
        LimitAppointments = limitAppointments;
    }

    public void ReplaceWorkingHours(IEnumerable<(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Location)>? hours)
    {
        _workingHours.Clear();

        if (hours == null)
        {
            return;
        }

        foreach (var (day, start, end, location) in hours)
        {
            var hour = DoctorWorkingHour.Create(Id, day, start, end, location);
            hour.AttachDoctor(this);
            _workingHours.Add(hour);
        }
    }

    public DoctorDayOff AddDayOff(string name, DateOnly startDate, DateOnly endDate, bool repeatYearly)
    {
        EnsureUniqueDayOffName(name, null);
        var dayOff = DoctorDayOff.Create(Id, name, startDate, endDate, repeatYearly);
        dayOff.AttachDoctor(this);
        _daysOff.Add(dayOff);
        return dayOff;
    }

    public DoctorDayOff UpdateDayOff(Guid id, string name, DateOnly startDate, DateOnly endDate, bool repeatYearly)
    {
        var dayOff = _daysOff.FirstOrDefault(x => x.Id == id) ?? throw new ArgumentException("Day off was not found.");
        EnsureUniqueDayOffName(name, id);
        dayOff.Update(name, startDate, endDate, repeatYearly);
        return dayOff;
    }

    public DoctorDayOff? FindDayOff(Guid id)
    {
        return _daysOff.FirstOrDefault(x => x.Id == id);
    }

    public void RemoveDayOff(Guid id)
    {
        var target = _daysOff.FirstOrDefault(x => x.Id == id);
        if (target != null)
        {
            _daysOff.Remove(target);
        }
    }

    public void ReplaceDaysOff(IEnumerable<(string Name, DateOnly StartDate, DateOnly EndDate, bool RepeatYearly)>? dayOffs)
    {
        _daysOff.Clear();

        if (dayOffs == null)
        {
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, startDate, endDate, repeatYearly) in dayOffs)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Day off name is required.");
            }

            var trimmedName = name.Trim();
            if (!seen.Add(trimmedName))
            {
                throw new InvalidOperationException($"Day off name '{trimmedName}' must be unique per doctor.");
            }

            var dayOff = DoctorDayOff.Create(Id, name, startDate, endDate, repeatYearly);
            dayOff.AttachDoctor(this);
            _daysOff.Add(dayOff);
        }
    }

    private void EnsureUniqueDayOffName(string name, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var trimmedName = name.Trim();
        // giải thích đoạn dưới
        var exists = _daysOff.Any(x => (excludeId is null || x.Id != excludeId) && string.Equals(x.Name, trimmedName, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            throw new InvalidOperationException($"Day off name '{trimmedName}' must be unique per doctor.");
        }
    }
}