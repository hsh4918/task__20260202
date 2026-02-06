using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;

namespace emergency_contact_system.Application.Abstractions.Repositories;

public interface IEmployeeRepository
{
    Task AddRangeAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken);
    Task<IReadOnlyList<Employee>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Employee employee, CancellationToken cancellationToken);
    Task<PagedResult<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
}
