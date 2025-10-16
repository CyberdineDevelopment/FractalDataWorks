# FractalDataWorks.Services.Execution.Tests

Unit tests for the FractalDataWorks.Services.Execution project.

## Test Strategy

- **Framework**: xUnit v3
- **Assertions**: Shouldly
- **Mocking**: Moq
- **Coverage Target**: 100% path coverage (measured by Coverlet)

## Coverage

All untestable code (external dependencies, infrastructure, etc.) should be marked with [ExcludeFromCodeCoverage] attribute.

## Running Tests

```bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View coverage report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```
