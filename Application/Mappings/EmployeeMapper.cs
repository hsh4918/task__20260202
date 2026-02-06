using emergency_contact_system.Application.Models;
using emergency_contact_system.Domain;

namespace emergency_contact_system.Application.Mappings;

public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Employee employee) => new(
        employee.Name,
        employee.Email,
        employee.Tel,
        employee.Joined.ToString("yyyy-MM-dd"));
}
