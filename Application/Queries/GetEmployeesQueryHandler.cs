using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Mappings;
using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Queries;

public sealed class GetEmployeesQueryHandler(IEmployeeRepository repository)
    : IQueryHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> HandleAsync(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        var pagedResult = await repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        var items = pagedResult.Items.Select(EmployeeMapper.ToDto).ToList();
        return new PagedResult<EmployeeDto>(items, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }
}
