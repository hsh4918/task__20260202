using System.Threading.Tasks;
using emergency_contact_system.Infrastructure.Repositories;
using emergency_contact_system.Domain;
using FluentAssertions;
using Xunit;

namespace emergency_contact_system.Tests.Repositories;

public class InMemoryEmployeeRepositoryTests
{
    [Fact]
    public async Task AddAndExists_Workflow()
    {
        var repo = new InMemoryEmployeeRepository();
        var emp = new Employee("A","a@x.com","01012345678", new DateOnly(2023,1,1));
        await repo.AddRangeAsync(new[] { emp }, default);
        var exists = await repo.ExistsAsync(emp, default);
        exists.Should().BeTrue();
    }
}
