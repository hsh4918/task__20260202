using System.Threading.Tasks;
using System.Threading;
using Moq;
using FluentAssertions;
using Xunit;
using emergency_contact_system.Application.Commands;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;
using System.Collections.Generic;

namespace emergency_contact_system.Tests.Commands;

public class AddEmployeesCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_DuplicateInRepo_NotAdded()
    {
        var repo = new Mock<IEmployeeRepository>();
        repo.Setup(r => r.ExistsAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new AddEmployeesCommandHandler(repo.Object);
        var input = new EmployeeInput("A","a@x.com","010-1234-5678", new DateOnly(2023,1,1));

        var result = await handler.HandleAsync(new AddEmployeesCommand(new List<EmployeeInput> { input }), CancellationToken.None);

        result.AddedCount.Should().Be(0);
        repo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
