using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Mappings;
using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Queries;

public sealed class GetEmployeeByNameQueryHandler(IEmployeeRepository repository)
    : IQueryHandler<GetEmployeeByNameQuery, IReadOnlyList<EmployeeDto>>
{
    public async Task<IReadOnlyList<EmployeeDto>> HandleAsync(GetEmployeeByNameQuery query, CancellationToken cancellationToken)
    {
        var employees = await repository.GetByNameAsync(query.Name, cancellationToken);
        var items = employees.Select(EmployeeMapper.ToDto).ToList();
        return items;
    }
}
