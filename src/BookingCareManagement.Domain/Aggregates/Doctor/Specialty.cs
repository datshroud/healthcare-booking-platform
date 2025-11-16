using System;
using System.Collections.Generic;

namespace BookingCareManagement.Domain.Aggregates.Doctor;

// File này đại diện cho "Danh mục"
public class Specialty
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public bool Active { get; private set; } = true;

    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Color { get; private set; } = "#1a73e8";
    public decimal Price { get; private set; }

    private readonly List<Doctor> _doctors = new();
    public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();

    // Giữ quan hệ với Service (Chuyên khoa)
    // private readonly List<Service> _services = new();
    // public IReadOnlyCollection<Service> Services => _services;

    private Specialty() { }

    public Specialty(
        string name,
        string? slug = null,
        string? description = null,
        string? imageUrl = null,
        string? color = null,
        decimal price = 0)
    {
        Name = name;
        Slug = slug ?? name.Trim().ToLower().Replace(' ', '-');
        Description = description;
        ImageUrl = imageUrl;
        SetColor(color);
        SetPrice(price);
    }

    // Thêm hàm Update cho API sau này
    public void Update(
        string name,
        string? slug = null,
        string? description = null,
        string? imageUrl = null,
        string? color = null,
        decimal price = 0)
    {
        Name = name;
        Slug = slug ?? name.Trim().ToLower().Replace(' ', '-');
        Description = description;
        ImageUrl = imageUrl;
        SetColor(color);
        SetPrice(price);
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;

    public void ReplaceDoctors(IEnumerable<Doctor>? doctors)
    {
        foreach (var doctor in _doctors.ToArray())
        {
            doctor?.RemoveSpecialty(Id);
        }

        _doctors.Clear();

        if (doctors is null)
        {
            return;
        }

        foreach (var doctor in doctors)
        {
            if (doctor is not null)
            {
                doctor.AddSpecialty(this);
                _doctors.Add(doctor);
            }
        }
    }

    public void SetColor(string? color)
    {
        Color = string.IsNullOrWhiteSpace(color) ? "#1a73e8" : color.Trim();
    }

    public void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Price = decimal.Round(price, 0, MidpointRounding.AwayFromZero);
    }
}