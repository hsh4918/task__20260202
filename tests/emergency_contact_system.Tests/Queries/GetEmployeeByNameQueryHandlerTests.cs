using System.Threading.Tasks;
using System.Threading;
using Moq;
using FluentAssertions;
using Xunit;
using emergency_contact_system.Application.Queries;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;
using System.Collections.Generic;
using System.Linq;

namespace emergency_contact_system.Tests.Queries;

public class GetEmployeeByNameQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAllMatching()
    {
        var repo = new Mock<IEmployeeRepository>();
        var list = new List<Employee>
        {
            new Employee("A","a@x.com","010", new DateOnly(2023,1,1)),
            new Employee("A","b@x.com","011", new DateOnly(2023,1,2))
        };
        repo.Setup(r => r.GetByNameAsync("A", It.IsAny<CancellationToken>())).ReturnsAsync((IReadOnlyList<Employee>)list);

        var handler = new GetEmployeeByNameQueryHandler(repo.Object);
        var result = await handler.HandleAsync(new GetEmployeeByNameQuery("A"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().AllBeEquivalentTo("A");
    }
}
