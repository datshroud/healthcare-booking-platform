using BookingCareManagement.Domain.Aggregates.User; // Cần cho AppUser

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class Doctor
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public bool Active { get; private set; } = true;

    // 1. Thêm khóa ngoại trỏ đến AppUser
    public string AppUserId { get; private set; }

    // 2. Thêm thuộc tính điều hướng (navigation property)
    public AppUser AppUser { get; private set; } = null!;

    // 3. Quan hệ nhiều-nhiều với Specialty (giữ nguyên)
    private readonly List<Specialty> _specialties = new();
    public IReadOnlyCollection<Specialty> Specialties => _specialties;

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
    private readonly List<Service.Service> _services = new();
    public IReadOnlyCollection<Service.Service> Services => _services;

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
}