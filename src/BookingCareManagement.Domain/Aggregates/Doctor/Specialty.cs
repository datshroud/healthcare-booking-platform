using System;

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class Specialty
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public bool Active { get; private set; } = true;

    private Specialty() { }
    public Specialty(string name, string? slug = null)
    {
        Name = name;
        Slug = slug ?? name.Trim().ToLower().Replace(' ', '-');
    }
    public void Deactivate() => Active = false;
}
