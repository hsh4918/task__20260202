# emergency-contact-system

## 개요

`emergency-contact-system`은 직원 비상 연락망 정보(이름, 연락처 등)를 관리하고 외부에서 CSV/JSON 형식으로 직원 목록을 업로드하여 저장 및 조회할 수 있는 간단한 ASP.NET Core 웹 API 프로젝트입니다. .NET 8을 사용하여 개발되어 있습니다.

프로젝트는 CQRS 패턴을 사용하였고 다음 계층으로 구성되어 있습니다.

- `Api` : 컨트롤러 및 진입점(HTTP)을 담당합니다. (예: `Api/Controllers/EmployeeController.cs`)
- `Application` : 명령/쿼리, DTO, 서비스 인터페이스 등 비즈니스 로직의 핵심을 포함합니다.
- `Infrastructure` : 영속성 구현(예: 인메모리 저장소)과 외부 의존성 구현체를 포함합니다.
- `Middleware` : 요청/응답 로깅 등 파이프라인 미들웨어를 포함합니다.

## 주요 기능

- 직원 정보 추가(파일 업로드 또는 본문): CSV 또는 JSON 포맷을 파싱하여 저장
- 페이징된 직원 목록 조회
- 이름으로 직원 조회
- Serilog 기반 파일 로깅(레벨별 분리)
- 요청/응답 바디를 로깅하는 커스텀 미들웨어

## 엔드포인트

- `GET /api/employee?page={page}&pageSize={pageSize}`
  - 설명: 페이징된 직원 목록 조회
  - 기본값: `page=1`, `pageSize=20`

- `GET /api/employee/{name}`
  - 설명: 이름으로 직원 조회. 없으면 `404 NotFound` 반환

- `POST /api/employee`
  - 설명: 직원 정보를 추가. `multipart/form-data`로 파일 업로드하거나 `application/json`, `text/csv` 본문으로 전달 가능
  - 반환: `201 Created` 및 반영 된 Count(성공 시)

`EmployeeController`는 업로드된 파일의 확장자 또는 요청 콘텐츠를 기반으로 입력 포맷(`Csv` 또는 `Json`)을 판단하고, `IEmployeeImportParser`를 통해 파싱한 결과를 `AddEmployeesCommand`로 전달해 처리합니다.

## 주요 구성 요소

- `Program.cs`
  - Serilog 설정을 통해 로그를 `Logs/debug`, `Logs/info`, `Logs/error` 폴더로 레벨별 분리하여 기록합니다.
  - DI(의존성 주입):
    - `IEmployeeRepository` -> `InMemoryEmployeeRepository` (Singleton)
    - `IEmployeeImportParser` -> `EmployeeImportParser` (Singleton)
    - 명령/쿼리 핸들러들은 Scoped로 등록

- `Middleware/LoggingMiddleware.cs`
  - 요청(Request) 바디와 응답(Response) 바디를 읽어서 `Debug` 레벨로 로깅합니다. ASP.NET Core 파이프라인에 등록되어 모든 요청/응답을 감시합니다.

- 명령/쿼리 패턴
  - `AddEmployeesCommand` / `AddEmployeesResult`
  - `GetEmployeesQuery` -> `PagedResult<EmployeeDto>`
  - `GetEmployeeByNameQuery` -> `EmployeeDto?`

## 개발 및 실행 방법

사전 요구사항:
- .NET 8 SDK

개발 환경에서는 Swagger가 자동 활성화되어 `https://localhost:{port}/swagger`에서 API를 테스트할 수 있습니다.

## 로깅

프로젝트는 `Serilog`를 사용합니다. 로그는 레벨별로 파일에 기록됩니다:

- `Logs/debug/log-.txt`
- `Logs/info/log-.txt`
- `Logs/error/log-.txt`

또한 `LoggingMiddleware`에서 요청/응답 바디를 캡처하여 `Debug` 레벨 로그에 포함시킵니다.

