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

    public Task<IReadOnlyList<Employee>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var items = _employees
                .Where(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult((IReadOnlyList<Employee>)items);
        }
    }

    public Task<bool> ExistsAsync(Employee employee, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            static string NormalizeTel(string? tel) => string.IsNullOrWhiteSpace(tel) ? string.Empty : tel.Replace("-", "").Trim();

            var exists = _employees.Any(e =>
                string.Equals(e.Name, employee.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(e.Email, employee.Email, StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizeTel(e.Tel), NormalizeTel(employee.Tel), StringComparison.OrdinalIgnoreCase)
                && e.Joined == employee.Joined);

            return Task.FromResult(exists);
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
