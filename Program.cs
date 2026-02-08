using emergency_contact_system.Application.Abstractions.Messaging;
using emergency_contact_system.Application.Abstractions.Repositories;
using emergency_contact_system.Application.Commands;
using emergency_contact_system.Application.Models;
using emergency_contact_system.Application.Queries;
using emergency_contact_system.Application.Services;
using emergency_contact_system.Infrastructure.Repositories;
using emergency_contact_system.Middleware;
using Serilog;
using Serilog.Events;
using System.Collections.Generic;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    {
        //각 레벨마다의 log를 구성
        //Info -> 각 Api의 호출 실행 시간
        //Debug -> DTO 정보 등 로깅
        //Error -> Exception 로그
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent => logEvent.Level == LogEventLevel.Debug)
                .WriteTo.File("Logs/debug/log-.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent => logEvent.Level == LogEventLevel.Information)
                .WriteTo.File("Logs/info/log-.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent => logEvent.Level == LogEventLevel.Error)
                .WriteTo.File("Logs/error/log-.txt", rollingInterval: RollingInterval.Day));
    });

    builder.Services.AddControllers();
    //외부 진입 방지를 위하여 개발 환경에서만 Swagger 활성화
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepository>();
    builder.Services.AddSingleton<IEmployeeImportParser, EmployeeImportParser>();
    builder.Services.AddScoped<ICommandHandler<AddEmployeesCommand, AddEmployeesResult>, AddEmployeesCommandHandler>();
    builder.Services.AddScoped<IQueryHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>, GetEmployeesQueryHandler>();
    builder.Services.AddScoped<IQueryHandler<GetEmployeeByNameQuery, IReadOnlyList<EmployeeDto>>, GetEmployeeByNameQueryHandler>();

    var app = builder.Build();

    app.UseHttpsRedirection();

    app.UseRouting();

    //Logging Middleware를 추가하여 모든 요청과 응답을 로깅(Azure의 Application Insights와 Serilog를 통합 사용)
    //Application Insights는 주로 성능 모니터링과 장애 추적에 사용되고, Serilog는 파일로그만 사용하도록 구성
    app.UseMiddleware<LoggingMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
