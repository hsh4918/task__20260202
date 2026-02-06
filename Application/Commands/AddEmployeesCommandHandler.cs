using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;

namespace emergency_contact_system.Application.Commands;

public sealed class AddEmployeesCommandHandler(IEmployeeRepository repository)
    : ICommandHandler<AddEmployeesCommand, AddEmployeesResult>
{
    public async Task<AddEmployeesResult> HandleAsync(AddEmployeesCommand command, CancellationToken cancellationToken)
    {
        if (command.Employees.Count == 0)
        {
            return new AddEmployeesResult(0);
        }

        var employees = command.Employees.Select(input => new Employee(
            input.Name,
            input.Email,
            input.Tel,
            input.Joined));

        await repository.AddRangeAsync(employees, cancellationToken);
        return new AddEmployeesResult(command.Employees.Count);
    }
}
