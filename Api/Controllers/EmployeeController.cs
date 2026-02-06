using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Commands;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Application.Queries;
using emergency_contact_system.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace emergency_contact_system.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeeController(
    ICommandHandler<AddEmployeesCommand, AddEmployeesResult> addEmployeesHandler,
    IQueryHandler<GetEmployeesQuery, PagedResult<EmployeeDto>> getEmployeesHandler,
    IQueryHandler<GetEmployeeByNameQuery, IReadOnlyList<EmployeeDto>> getEmployeeByNameHandler,
    IEmployeeImportParser importParser,
    ILogger<EmployeeController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeInfo([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("page와 pageSize는 1 이상이어야 합니다.");
        }

        var result = await getEmployeesHandler.HandleAsync(new GetEmployeesQuery(page, pageSize), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("{name}")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await getEmployeeByNameHandler.HandleAsync(new GetEmployeeByNameQuery(name), HttpContext.RequestAborted);
        return result == null || result.Count == 0 ? NotFound("조회 대상이 없습니다.") : Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data", "application/json", "text/csv", "text/plain")]
    [ProducesResponseType(typeof(AddEmployeesResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAddEmployeeInfo([FromForm] IFormFile? file)
    {
        try
        {
            var (content, format) = await ReadContentAsync(file);
            var employees = importParser.Parse(content, format);

            var result = await addEmployeesHandler.HandleAsync(
                new AddEmployeesCommand(employees),
                HttpContext.RequestAborted);

            return CreatedAtAction(nameof(GetEmployeeInfo), new { page = 1, pageSize = 20 }, result);
        }
        catch (EmployeeImportException ex)
        {
            logger.LogWarning(ex, "직원 데이터 파싱 실패");
            return BadRequest(ex.Message);
        }
    }

    private async Task<(string Content, EmployeeInputFormat Format)> ReadContentAsync(IFormFile? file)
    {
        //multipart/form-data 요청에서 파일이 file 파라미터로 바인딩되지 않아 Request.Body를 읽는 경우가 있음
        //그래서 boundary/header가 CSV 내용에 섞여서 파싱 실패하는 오류가 발생하여 아래의 예외를 추가
        if (file is null && Request.HasFormContentType)
        {
            var formFile = Request.Form.Files.FirstOrDefault();
            if (formFile is not null)
            {
                file = formFile;
            }
        }

        if (file is not null)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            return (content, GetFormatFromFileName(file.FileName));
        }

        using var bodyReader = new StreamReader(Request.Body);
        var bodyContent = await bodyReader.ReadToEndAsync();
        return (bodyContent, GetFormatFromContent(bodyContent, Request.ContentType));
    }

    private static EmployeeInputFormat GetFormatFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => EmployeeInputFormat.Csv,
            ".json" => EmployeeInputFormat.Json,
            _ => throw new EmployeeImportException("지원하지 않는 파일 형식입니다.")
        };
    }

    private static EmployeeInputFormat GetFormatFromContent(string content, string? contentType)
    {
        if (contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
        {
            return EmployeeInputFormat.Json;
        }

        if (contentType?.Contains("csv", StringComparison.OrdinalIgnoreCase) == true)
        {
            return EmployeeInputFormat.Csv;
        }

        var trimmed = content.TrimStart();
        if (trimmed.StartsWith("[") || trimmed.StartsWith("{"))
        {
            return EmployeeInputFormat.Json;
        }

        return EmployeeInputFormat.Csv;
    }
}
