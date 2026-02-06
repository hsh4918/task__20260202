namespace emergency_contact_system.Application.Models;

public sealed record EmployeeDto(
    string Name,
    string Email,
    string Tel,
    string Joined);
