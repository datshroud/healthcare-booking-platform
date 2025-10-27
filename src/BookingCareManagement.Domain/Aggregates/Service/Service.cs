using System;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Domain.Aggregates.Service;

public class Service
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; } = 60;
    public int Capacity { get; private set; } = 1;
    public bool Active { get; private set; } = true;

    private readonly List<Specialty> _specialties = new();
    public IReadOnlyCollection<Specialty> Specialties => _specialties;

    private Service() { }
    public Service(string name, decimal price, int durationMinutes, int capacity = 1, string? desc = null)
    {
        Name = name; Price = price; DurationMinutes = durationMinutes; Capacity = capacity; Description = desc;
    }

    public void AddSpecialty(Specialty sp)
    {
        if (_specialties.All(x => x.Id != sp.Id)) _specialties.Add(sp);
    }
}
