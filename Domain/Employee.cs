namespace emergency_contact_system.Domain;

public sealed record Employee(
    string Name,
    string Email,
    string Tel,
    DateOnly Joined);
