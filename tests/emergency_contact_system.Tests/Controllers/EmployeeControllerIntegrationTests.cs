using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace emergency_contact_system.Tests.Controllers;

public class EmployeeControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public EmployeeControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_AddEmployeeJson_Returns201()
    {
        var json = "[{\"name\":\"ȫ�浿\",\"email\":\"a@b.com\",\"tel\":\"010-1234-5678\",\"joined\":\"2023-01-01\"}]";
        var resp = await _client.PostAsync("/api/employee", new StringContent(json, Encoding.UTF8, "application/json"));
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("addedCount");
    }
}
