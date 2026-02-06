namespace emergency_contact_system.Application.Models;

public sealed record EmployeeInput(
    string Name,
    string Email,
    string Tel,
    DateOnly Joined);
