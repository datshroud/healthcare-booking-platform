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

    // THÊM 2 TRƯỜNG MỚI:
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    // Giữ quan hệ với Service (Chuyên khoa)
    // private readonly List<Service> _services = new();
    // public IReadOnlyCollection<Service> Services => _services;

    private Specialty() { }

    public Specialty(string name, string? slug = null, string? description = null, string? imageUrl = null)
    {
        Name = name;
        Slug = slug ?? name.Trim().ToLower().Replace(' ', '-');
        Description = description;
        ImageUrl = imageUrl;
    }

    // Thêm hàm Update cho API sau này
    public void Update(string name, string? slug = null, string? description = null, string? imageUrl = null)
    {
        Name = name;
        Slug = slug ?? name.Trim().ToLower().Replace(' ', '-');
        Description = description;
        ImageUrl = imageUrl;
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;
}