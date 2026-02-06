
# emergency-contact-system

## 개요

`emergency-contact-system`은 직원 비상 연락망 정보(이름, 연락처 등)를 관리하고 외부에서 CSV/JSON 형식으로 직원 목록을 업로드하여 저장 및 조회할 수 있는 간단한 ASP.NET Core 웹 API 프로젝트입니다. .NET 8을 사용하여 개발되어 있습니다.

프로젝트는 CQRS 패턴을 사용하며 다음 계층으로 구성되어 있습니다:

- `Api` : 컨트롤러 및 진입점(HTTP) (예: `Api/Controllers/EmployeeController.cs`)
- `Application` : 명령/쿼리, DTO, 서비스 인터페이스 등 비즈니스 로직
- `Infrastructure` : 영속성 구현(예: 인메모리 저장소) 및 외부 의존성 구현체
- `Middleware` : 요청/응답 로깅 등 파이프라인 미들웨어

## 최근 변경 및 중요한 동작

- `GET /api/employee/{name}`: 이름으로 조회 시 동일 이름을 가진 모든 직원(리스트)을 반환합니다. 결과가 없으면 `404 NotFound`를 반환합니다.
- 직원 목록 조회는 이름 기준 오름차순으로 정렬되어 반환됩니다.
- `POST /api/employee`(업로드): 최초 저장 이후 동일한 직원 정보는 추가되지 않습니다. "동일"은 `Name`, `Email`, `Tel`, `Joined` 네 필드가 모두 동일한 경우로 판단합니다.
- 전화번호 정규화: 비교 및 저장 전에 전화번호에서 하이픈(`-`)을 제거합니다. (추가 정규화가 필요하면 별도 적용 가능)

## 엔드포인트

- `GET /api/employee?page={page}&pageSize={pageSize}`
  - 설명: 페이징된 직원 목록 조회
  - 기본값: `page=1`, `pageSize=20`

- `GET /api/employee/{name}`
  - 설명: 이름으로 직원 조회. 동일 이름이 여러 명일 경우 모두 리스트로 반환. 없으면 `404 NotFound` 반환

- `POST /api/employee`
  - 설명: 직원 정보를 추가. `multipart/form-data`로 파일 업로드하거나 `application/json`, `text/csv` 본문으로 전달 가능
  - 반환: `201 Created` 및 반영된 추가 건수 (`AddEmployeesResult`)

`EmployeeController`는 업로드된 파일의 확장자 또는 요청 콘텐츠를 기반으로 입력 포맷(`Csv` 또는 `Json`)을 판단하고, `IEmployeeImportParser`를 통해 파싱한 결과를 `AddEmployeesCommand`로 전달해 처리합니다.

## 주요 구성 요소 및 변경된 인터페이스

- `Program.cs`
  - Serilog 설정을 통해 로그를 `Logs/debug`, `Logs/info`, `Logs/error` 폴더로 레벨별 분리하여 기록합니다.
  - DI(의존성 주입):
    - `IEmployeeRepository` -> `InMemoryEmployeeRepository` (Singleton)
    - `IEmployeeImportParser` -> `EmployeeImportParser` (Singleton)
    - 명령/쿼리 핸들러들은 Scoped로 등록

- `IEmployeeRepository` 변경사항:
  - `Task<IReadOnlyList<Employee>> GetByNameAsync(string name, CancellationToken cancellationToken);`
    - 동일 이름을 가진 모든 회원을 리스트로 반환하도록 변경되었습니다.
  - `Task<bool> ExistsAsync(Employee employee, CancellationToken cancellationToken);`
    - 업로드된 직원이 이미 저장소에 존재하는지(네 필드 전부 동일) 여부를 확인하기 위한 메서드가 추가되었습니다.

- `AddEmployeesCommand` 처리 동작:
  - 업로드 배치 내부에서 같은 사람이 중복으로 포함된 경우(네 필드가 동일, 전화는 하이픈 제거된 상태로 비교) 한 번만 저장합니다.
  - 저장소에 동일한 사람이 이미 존재하면 추가하지 않습니다.

- `GetEmployeeByNameQuery` 처리:
  - 이제 쿼리는 `IReadOnlyList<EmployeeDto>`를 반환하며, 컨트롤러는 결과 리스트가 비어있을 경우 `404`를 반환합니다.

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

