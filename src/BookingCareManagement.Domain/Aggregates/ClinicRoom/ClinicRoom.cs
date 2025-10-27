using System;

namespace BookingCareManagement.Domain.Aggregates.ClinicRoom;

public class ClinicRoom
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; }
    public int Capacity { get; private set; } = 1;

    private ClinicRoom() { }
    public ClinicRoom(string code, int capacity = 1) { Code = code; Capacity = capacity; }
}
