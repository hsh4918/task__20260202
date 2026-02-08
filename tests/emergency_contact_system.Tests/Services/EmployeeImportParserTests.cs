using System;
using System.Text;
using emergency_contact_system.Application.Services;
using emergency_contact_system.Application.Models;
using FluentAssertions;
using Xunit;

namespace emergency_contact_system.Tests.Services;

public class EmployeeImportParserTests
{
    [Fact]
    public void ParseCsv_ValidLine_ReturnsEmployeeInput()
    {
        var parser = new EmployeeImportParser();
        var csv = "ȫ�浿,email@example.com,010-1234-5678,2023-01-01";
        var list = parser.Parse(csv, EmployeeInputFormat.Csv);
        list.Should().HaveCount(1);
        var e = list[0];
        e.Name.Should().Be("ȫ�浿");
        e.Email.Should().Be("email@example.com");
        e.Tel.Should().Be("010-1234-5678");
        e.Joined.Should().Be(new DateOnly(2023, 1, 1));
    }

    [Fact]
    public void ParseJson_InvalidJson_Throws()
    {
        var parser = new EmployeeImportParser();
        Action act = () => parser.Parse("not-json", EmployeeInputFormat.Json);
        act.Should().Throw<Exception>();
    }
}
