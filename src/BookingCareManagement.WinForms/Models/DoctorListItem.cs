using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Models;

/// <summary>
/// DTO representing the data returned by the doctors API list endpoint.
/// </summary>
/// 
public sealed record DoctorListItem(Guid Id, string FullName, IReadOnlyCollection<string> Specialties);
