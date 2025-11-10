using System;

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class Doctor
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FullName { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;

    // many-to-many
    private readonly List<Specialty> _specialties = new();
    public IReadOnlyCollection<Specialty> Specialties => _specialties;

    private Doctor() { }
    public Doctor(string fullName) => FullName = fullName;

    public void AddSpecialty(Specialty s)
    {
        if (_specialties.All(x => x.Id != s.Id)) _specialties.Add(s);
    }
}
