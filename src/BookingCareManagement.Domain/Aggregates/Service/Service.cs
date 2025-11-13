using BookingCareManagement.Domain.Aggregates.Doctor;
using System;
using System.Collections.Generic;

namespace BookingCareManagement.Domain.Aggregates.Service;

public class Service
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationInMinutes { get; private set; }
    public bool Active { get; private set; } = true;
    public string? Color { get; private set; }
    public string? ImageUrl { get; private set; }

    public Guid SpecialtyId { get; private set; }
    public Specialty Specialty { get; private set; } = null!;

    private readonly List<Doctor.Doctor> _doctors = new();
    public IReadOnlyCollection<Doctor.Doctor> Doctors => _doctors;

    private Service() { }

    // THÊM CONSTRUCTOR MỚI
    public Service(
        string name,
        decimal price,
        int durationInMinutes,
        Guid specialtyId,
        string? description,
        string? color,
        string? imageUrl)
    {
        Name = name;
        Price = price;
        DurationInMinutes = durationInMinutes;
        SpecialtyId = specialtyId;
        Description = description;
        Color = color;
        ImageUrl = imageUrl;
    }

    // THÊM HÀM UPDATE MỚI
    public void Update(
        string name,
        decimal price,
        int durationInMinutes,
        Guid specialtyId,
        string? description,
        string? color,
        string? imageUrl)
    {
        Name = name;
        Price = price;
        DurationInMinutes = durationInMinutes;
        SpecialtyId = specialtyId;
        Description = description;
        Color = color;
        ImageUrl = imageUrl;
    }

    // THÊM CÁC HÀM QUẢN LÝ DOCTOR
    public void AddDoctor(Doctor.Doctor doctor)
    {
        if (_doctors.All(d => d.Id != doctor.Id))
        {
            _doctors.Add(doctor);
        }
    }

    public void ClearDoctors()
    {
        _doctors.Clear();
    }

    // THÊM HÀM QUẢN LÝ ACTIVE
    public void Activate()
    {
        Active = true;
    }

    public void Deactivate()
    {
        Active = false;
    }
}