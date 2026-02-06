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

        // Remove duplicates: - within the incoming batch (all fields identical) and - those already stored (all fields identical)
        var toAdd = new List<Employee>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static string NormalizeTel(string? tel) => string.IsNullOrWhiteSpace(tel) ? string.Empty : tel.Replace("-", "").Trim();

        foreach (var input in command.Employees)
        {
            var normalizedTel = NormalizeTel(input.Tel);
            var key = $"{input.Name}|{input.Email}|{normalizedTel}|{input.Joined:yyyy-MM-dd}";

            // skip duplicate entries within the batch (all fields identical after normalization)
            if (!seenKeys.Add(key))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var candidate = new Employee(
                input.Name,
                input.Email,
                normalizedTel,
                input.Joined);

            // skip if an employee with identical data already exists in repository
            var exists = await repository.ExistsAsync(candidate, cancellationToken);
            if (!exists)
            {
                toAdd.Add(candidate);
            }
        }

        if (toAdd.Count > 0)
        {
            await repository.AddRangeAsync(toAdd, cancellationToken);
        }

        return new AddEmployeesResult(toAdd.Count);
    }
}
