using System.Globalization;
using System.Text.Json;
using emergency_contact_system.Application.Models;

namespace emergency_contact_system.Application.Services;

public sealed class EmployeeImportParser : IEmployeeImportParser
{
    private static readonly string[] SupportedDateFormats = ["yyyy.MM.dd", "yyyy-MM-dd"];

    public IReadOnlyList<EmployeeInput> Parse(string content, EmployeeInputFormat format)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new EmployeeImportException("입력 데이터가 비어 있습니다.");
        }

        return format switch
        {
            EmployeeInputFormat.Csv => ParseCsv(content),
            EmployeeInputFormat.Json => ParseJson(content),
            _ => throw new EmployeeImportException("지원하지 않는 형식입니다.")
        };
    }

    private static IReadOnlyList<EmployeeInput> ParseCsv(string csv)
    {
        var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var results = new List<EmployeeInput>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 4)
            {
                throw new EmployeeImportException("CSV 형식이 올바르지 않습니다.");
            }

            var joined = ParseDate(parts[3]);
            results.Add(new EmployeeInput(parts[0], parts[1], parts[2], joined));
        }

        return results;
    }

    private static IReadOnlyList<EmployeeInput> ParseJson(string json)
    {
        try
        {
            var items = JsonSerializer.Deserialize<List<EmployeeJsonModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items is null || items.Count == 0)
            {
                throw new EmployeeImportException("JSON 데이터가 비어 있습니다.");
            }

            return items.Select(item => new EmployeeInput(
                item.Name,
                item.Email,
                item.Tel,
                ParseDate(item.Joined)))
                .ToList();
        }
        catch (JsonException ex)
        {
            throw new EmployeeImportException($"JSON 형식이 올바르지 않습니다: {ex.Message}");
        }
    }

    private static DateOnly ParseDate(string value)
    {
        if (DateOnly.TryParseExact(value.Trim(), SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new EmployeeImportException("날짜 형식이 올바르지 않습니다.");
    }

    private sealed class EmployeeJsonModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tel { get; set; } = string.Empty;
        public string Joined { get; set; } = string.Empty;
    }
}
