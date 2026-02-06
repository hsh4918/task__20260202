using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Mappings;
using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Queries;

public sealed class GetEmployeeByNameQueryHandler(IEmployeeRepository repository)
    : IQueryHandler<GetEmployeeByNameQuery, EmployeeDto?>
{
    public async Task<EmployeeDto?> HandleAsync(GetEmployeeByNameQuery query, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByNameAsync(query.Name, cancellationToken);
        return employee is null ? null : EmployeeMapper.ToDto(employee);
    }
}
