using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;

namespace emergency_contact_system.Infrastructure.Repositories;

public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees = [];
    private readonly object _lock = new();

    public Task AddRangeAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            _employees.AddRange(employees);
        }

        return Task.CompletedTask;
    }

    public Task<Employee?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var employee = _employees.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(employee);
        }
    }

    public Task<PagedResult<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var totalCount = _employees.Count;
            var items = _employees
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<Employee>(items, page, pageSize, totalCount);
            return Task.FromResult(result);
        }
    }
}
