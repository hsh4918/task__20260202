using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Services;

public interface IEmployeeImportParser
{
    IReadOnlyList<EmployeeInput> Parse(string content, EmployeeInputFormat format);
}

public enum EmployeeInputFormat
{
    Csv,
    Json
}
