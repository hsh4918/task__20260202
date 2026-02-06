using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Commands;

public sealed record AddEmployeesCommand(IReadOnlyCollection<EmployeeInput> Employees);
