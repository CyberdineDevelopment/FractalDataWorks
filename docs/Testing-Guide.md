# FractalDataWorks Developer Kit - Testing Guide

**Comprehensive testing strategy for xUnit v3 and .NET 10**

## Table of Contents

- [Testing Philosophy](#testing-philosophy)
- [Test Project Structure](#test-project-structure)
- [xUnit v3 Setup](#xunit-v3-setup)
- [Testing Patterns](#testing-patterns)
- [Source Generator Testing](#source-generator-testing)
- [Coverage Requirements](#coverage-requirements)
- [Best Practices](#best-practices)

## Testing Philosophy

The FractalDataWorks Developer Kit follows these testing principles:

1. **Test Public APIs**: Focus on public interfaces and contracts
2. **Test Behavior, Not Implementation**: Verify outcomes, not internal details
3. **Arrange-Act-Assert**: Clear test structure
4. **Fast Tests**: Unit tests run in <100ms, integration tests in <1s
5. **Isolated Tests**: No shared state between tests
6. **Meaningful Names**: Test names describe scenarios and expected outcomes

## Test Project Structure

### Naming Convention

Test projects follow this pattern:
```
[ProjectName].Tests
```

Examples:
- `FractalDataWorks.Collections.Tests` tests `FractalDataWorks.Collections`
- `FractalDataWorks.Services.Tests` tests `FractalDataWorks.Services`
- `FractalDataWorks.MCP.Tests` tests MCP tool projects

### Project Configuration

**.NET 10 Test Project Structure**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit v3 -->
    <PackageReference Include="xunit" Version="3.0.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>

    <!-- Assertion Libraries -->
    <PackageReference Include="Shouldly" Version="4.2.1" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />

    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.70" />

    <!-- Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>

    <!-- Microsoft Testing -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Project Under Test -->
    <ProjectReference Include="..\FractalDataWorks.Collections\FractalDataWorks.Collections.csproj" />
  </ItemGroup>
</Project>
```

### Folder Organization

```
FractalDataWorks.Collections.Tests/
├── Collections/              # Tests for collection generation
│   ├── TypeCollectionTests.cs
│   ├── EnumCollectionTests.cs
│   └── GlobalCollectionTests.cs
├── Attributes/              # Tests for attributes
│   ├── TypeOptionAttributeTests.cs
│   └── TypeLookupAttributeTests.cs
├── Performance/            # Performance tests
│   └── LookupPerformanceTests.cs
├── Integration/           # Integration tests
│   └── CrossAssemblyTests.cs
├── Helpers/              # Test utilities
│   └── TestCollectionBuilder.cs
└── README.md
```

## xUnit v3 Setup

### Key Changes from xUnit v2

xUnit v3 introduces several improvements:

1. **Better async support**: Native async test execution
2. **Improved parallelization**: Better test isolation
3. **Enhanced attributes**: More powerful test configuration
4. **Nullable annotations**: Full nullable reference type support

### Basic Test Class

```csharp
using Xunit;
using Shouldly;

namespace FractalDataWorks.Collections.Tests;

public class TypeCollectionTests
{
    [Fact]
    public void TypeCollection_ShouldContainAllDiscoveredTypes()
    {
        // Arrange
        var collection = SecurityMethods.All();

        // Act
        var count = collection.Count;

        // Assert
        count.ShouldBeGreaterThan(0);
        collection.ShouldContain(x => x.Name == "JWT");
        collection.ShouldContain(x => x.Name == "None");
    }

    [Theory]
    [InlineData("JWT", true)]
    [InlineData("None", true)]
    [InlineData("Invalid", false)]
    public void GetByName_WithVariousInputs_ReturnsExpectedResults(string name, bool shouldExist)
    {
        // Act
        var result = SecurityMethods.GetByName(name);

        // Assert
        if (shouldExist)
        {
            result.ShouldNotBeNull();
            result.Name.ShouldBe(name);
        }
        else
        {
            result.ShouldBe(SecurityMethods.Empty());
        }
    }
}
```

### Async Test Support

```csharp
public class ServiceExecutionTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var service = new EmailService(logger, configuration);
        var command = new SendEmailCommand { To = "test@example.com" };

        // Act
        var result = await service.Execute(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldContain("sent successfully");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var service = new EmailService(logger, configuration);
        var command = new SendEmailCommand { To = "test@example.com" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await service.Execute(command, cts.Token));
    }
}
```

### Test Fixtures for Shared Setup

```csharp
using Xunit;

namespace FractalDataWorks.Services.Tests;

// Shared fixture class
public class ServiceTestFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Setup shared resources
        var services = new ServiceCollection();

        // Register test services
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Services:Email:SmtpServer"] = "localhost",
                ["Services:Email:Port"] = "587"
            })
            .Build());

        ServiceProvider = services.BuildServiceProvider();
        Configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

// Test class using fixture
public class EmailServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public EmailServiceTests(ServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Configuration_ShouldBeLoaded()
    {
        // Arrange
        var config = _fixture.Configuration;

        // Act
        var smtpServer = config["Services:Email:SmtpServer"];

        // Assert
        smtpServer.ShouldBe("localhost");
    }
}
```

### Collection Fixtures for Shared Context

```csharp
using Xunit;

namespace FractalDataWorks.Services.Tests;

// Collection definition
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

// Fixture class
public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Setup test database
        ConnectionString = "Server=localhost;Database=TestDb;";
        await InitializeDatabase();
    }

    public async Task DisposeAsync()
    {
        // Cleanup test database
        await CleanupDatabase();
    }

    private async Task InitializeDatabase() { /* Setup logic */ }
    private async Task CleanupDatabase() { /* Cleanup logic */ }
}

// Test class using collection
[Collection("Database collection")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        // Use _fixture.ConnectionString
    }
}
```

## Testing Patterns

### Testing Enhanced Enums

```csharp
public class PriorityEnumTests
{
    [Fact]
    public void All_ShouldContainAllPriorities()
    {
        // Arrange & Act
        var all = PriorityCollection.All();

        // Assert
        all.ShouldNotBeEmpty();
        all.Length.ShouldBe(3); // Low, Medium, High
    }

    [Theory]
    [InlineData(1, "Low")]
    [InlineData(2, "Medium")]
    [InlineData(3, "High")]
    public void GetById_ShouldReturnCorrectPriority(int id, string expectedName)
    {
        // Act
        var priority = PriorityCollection.GetById(id);

        // Assert
        priority.ShouldNotBeNull();
        priority.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void GetById_WithInvalidId_ShouldReturnEmpty()
    {
        // Act
        var priority = PriorityCollection.GetById(999);

        // Assert
        priority.ShouldBe(PriorityCollection.Empty());
    }
}
```

### Testing Type Collections

```csharp
public class SecurityMethodsTests
{
    [Fact]
    public void StaticProperties_ShouldBeInitialized()
    {
        // Assert
        SecurityMethods.None.ShouldNotBeNull();
        SecurityMethods.Jwt.ShouldNotBeNull();
        SecurityMethods.None.Name.ShouldBe("None");
    }

    [Fact]
    public void All_ShouldReturnFrozenSet()
    {
        // Act
        var all = SecurityMethods.All();

        // Assert
        all.ShouldBeOfType<FrozenSet<ISecurityMethod>>();
        all.Count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("JWT")]
    [InlineData("ApiKey")]
    public void GetByName_ShouldBeCaseInsensitive(string name)
    {
        // Act
        var lower = SecurityMethods.GetByName(name.ToLower());
        var upper = SecurityMethods.GetByName(name.ToUpper());
        var mixed = SecurityMethods.GetByName(name);

        // Assert
        lower.ShouldBe(mixed);
        upper.ShouldBe(mixed);
        lower.ShouldBe(upper);
    }
}
```

### Testing Services with Results

```csharp
public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailConfiguration _configuration;
    private readonly Mock<ISmtpClient> _smtpClientMock;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _configuration = new EmailConfiguration
        {
            SmtpServer = "smtp.test.com",
            Port = 587,
            EnableSsl = true
        };
        _smtpClientMock = new Mock<ISmtpClient>();
    }

    [Fact]
    public async Task Execute_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var service = new EmailService(_loggerMock.Object, _configuration, _smtpClientMock.Object);
        var command = new SendEmailCommand
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test body"
        };

        _smtpClientMock
            .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await service.Execute(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldContain("sent successfully");

        _smtpClientMock.Verify(x => x.SendAsync(
            command.To,
            command.Subject,
            command.Body), Times.Once);
    }

    [Fact]
    public async Task Execute_WithSmtpFailure_ShouldReturnFailure()
    {
        // Arrange
        var service = new EmailService(_loggerMock.Object, _configuration, _smtpClientMock.Object);
        var command = new SendEmailCommand { To = "test@example.com" };

        _smtpClientMock
            .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new SmtpException("Connection failed"));

        // Act
        var result = await service.Execute(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.Message.ShouldContain("Connection failed");
    }
}
```

### Testing Service Factories

```csharp
public class EmailServiceFactoryTests
{
    [Fact]
    public void Create_WithValidConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new EmailServiceFactory(
            Mock.Of<ILogger<EmailServiceFactory>>(),
            Mock.Of<ILogger<EmailService>>());

        var config = new EmailConfiguration
        {
            SmtpServer = "smtp.test.com",
            Port = 587
        };

        // Act
        var result = factory.Create(config);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<EmailService>();
    }

    [Fact]
    public void Create_WithInvalidConfiguration_ShouldReturnFailure()
    {
        // Arrange
        var factory = new EmailServiceFactory(
            Mock.Of<ILogger<EmailServiceFactory>>(),
            Mock.Of<ILogger<EmailService>>());

        var config = new EmailConfiguration
        {
            SmtpServer = "", // Invalid
            Port = 0         // Invalid
        };

        // Act
        var result = factory.Create(config);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
    }
}
```

### Testing Configuration Validation

```csharp
public class EmailConfigurationTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var config = new EmailConfiguration
        {
            SmtpServer = "smtp.test.com",
            Port = 587,
            EnableSsl = true
        };

        // Act
        var result = config.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("", 587, false, "SmtpServer")]
    [InlineData("smtp.test.com", 0, false, "Port")]
    [InlineData("smtp.test.com", 70000, false, "Port")]
    public void Validate_WithInvalidConfiguration_ShouldFail(
        string smtpServer, int port, bool shouldBeValid, string expectedErrorProperty)
    {
        // Arrange
        var config = new EmailConfiguration
        {
            SmtpServer = smtpServer,
            Port = port
        };

        // Act
        var result = config.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Validation returns success with validation result
        result.Value.IsValid.ShouldBe(shouldBeValid);

        if (!shouldBeValid)
        {
            result.Value.Errors.ShouldContain(e => e.PropertyName == expectedErrorProperty);
        }
    }
}
```

## Source Generator Testing

### Testing Strategy

Source generators require special testing approaches:

1. **Compilation Tests**: Verify generated code compiles
2. **Output Tests**: Verify generated code structure
3. **Integration Tests**: Test generated code functionality

### Compilation Tests

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class EnumCollectionGeneratorTests
{
    [Fact]
    public void Generator_WithValidEnum_ShouldGenerateCollectionClass()
    {
        // Arrange
        var source = @"
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, ""Low"");
    public static readonly Priority High = new(2, ""High"");

    private Priority(int id, string name) : base(id, name) { }
}

[EnumCollection(""PriorityCollection"")]
public sealed partial class PriorityCollection : EnumCollectionBase<Priority>
{
}";

        var compilation = CreateCompilation(source);
        var generator = new EnumCollectionGenerator();

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Assert
        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Length.ShouldBeGreaterThan(0);
    }

    private static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
```

### Output Verification Tests

```csharp
public class GeneratedCodeTests
{
    [Fact]
    public void GeneratedCollection_ShouldContainStaticConstructor()
    {
        // Arrange
        var source = "/* source code */";
        var compilation = CreateCompilation(source);
        var generator = new EnumCollectionGenerator();

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var generatedCode = driver.GetRunResult()
            .GeneratedTrees
            .First()
            .GetText()
            .ToString();

        // Assert
        generatedCode.ShouldContain("static PriorityCollection()");
        generatedCode.ShouldContain("_all = ImmutableArray.Create");
        generatedCode.ShouldContain("Priority.Low");
        generatedCode.ShouldContain("Priority.High");
    }
}
```

### Integration Tests for Generated Code

```csharp
// Test project references project with source generator
public class GeneratedCollectionIntegrationTests
{
    [Fact]
    public void GeneratedCollection_ShouldWorkCorrectly()
    {
        // This test uses the actual generated code from the test project
        var all = TestPriorityCollection.All();

        all.ShouldNotBeEmpty();
        all.ShouldContain(x => x.Name == "Low");
        all.ShouldContain(x => x.Name == "High");
    }

    [Fact]
    public void GeneratedLookup_ShouldBeOptimized()
    {
        // Verify FrozenDictionary is used on .NET 8+
        var type = typeof(TestPriorityCollection);
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

#if NET8_0_OR_GREATER
        fields.ShouldContain(f => f.FieldType.Name.Contains("FrozenDictionary"));
#else
        fields.ShouldNotContain(f => f.FieldType.Name.Contains("FrozenDictionary"));
#endif
    }
}
```

## Coverage Requirements

### Code Coverage Targets

| Project Type | Minimum Coverage | Target Coverage |
|--------------|------------------|-----------------|
| Core Infrastructure | 80% | 90% |
| Service Abstractions | 70% | 85% |
| Service Implementations | 75% | 85% |
| Source Generators | 60% | 75% |
| MCP Tools | 70% | 80% |

### Exclusions from Coverage

#### 1. Source-Generated Code

```csharp
// Generated files automatically excluded via:
[ExcludeFromCodeCoverage]
[GeneratedCode("SourceGenerator", "1.0.0.0")]
public partial class PriorityCollection
{
    // Generated code
}
```

#### 2. Logging Methods

```csharp
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class")]
public static partial class ServiceLog
{
    // LoggerMessage-generated methods
}
```

#### 3. Simple Unit Types

```csharp
/// <ExcludeFromTest>Simple unit type with no business logic</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public struct NonResult : IEquatable<NonResult>
{
    // Trivial equality operations
}
```

### Running Coverage

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# View report
start coveragereport/index.html
```

### Coverage Configuration

**coverlet.runsettings**:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura,opencover</Format>
          <Exclude>[*.Tests]*,[*]*.g.cs,[*]*Generated*</Exclude>
          <ExcludeByAttribute>ExcludeFromCodeCoverage,GeneratedCode</ExcludeByAttribute>
          <UseSourceLink>true</UseSourceLink>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Usage:
```bash
dotnet test --settings coverlet.runsettings
```

## Best Practices

### Test Naming

Use descriptive test names that explain the scenario:

```csharp
// ✅ Good - Clear scenario and expected outcome
[Fact]
public void Execute_WithValidCommand_ShouldReturnSuccess() { }

[Fact]
public void GetById_WithNonExistentId_ShouldReturnEmpty() { }

// ❌ Bad - Unclear what is being tested
[Fact]
public void Test1() { }

[Fact]
public void TestGetById() { }
```

### Arrange-Act-Assert

Always structure tests clearly:

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - Setup test data and dependencies
    var service = new EmailService(logger, config);
    var command = new SendEmailCommand { To = "test@example.com" };

    // Act - Execute the method under test
    var result = await service.Execute(command);

    // Assert - Verify the outcome
    result.IsSuccess.ShouldBeTrue();
    result.Message.ShouldContain("sent");
}
```

### Test Independence

Tests should not depend on each other:

```csharp
// ✅ Good - Each test is independent
public class UserServiceTests
{
    [Fact]
    public void Test1()
    {
        var user = CreateTestUser();
        // Test using user
    }

    [Fact]
    public void Test2()
    {
        var user = CreateTestUser();
        // Test using user
    }

    private User CreateTestUser() => new() { Id = 1, Name = "Test" };
}

// ❌ Bad - Tests share state
public class UserServiceTests
{
    private User _sharedUser = new() { Id = 1 };

    [Fact]
    public void Test1()
    {
        _sharedUser.Name = "Changed"; // Affects Test2!
    }

    [Fact]
    public void Test2()
    {
        Assert.Equal("Test", _sharedUser.Name); // Fails if Test1 runs first!
    }
}
```

### Assertion Libraries

Use expressive assertions:

```csharp
// ✅ Shouldly - Readable and expressive
result.IsSuccess.ShouldBeTrue();
collection.ShouldContain(x => x.Name == "Test");
user.Email.ShouldBe("test@example.com");

// ✅ FluentAssertions - Also good
result.IsSuccess.Should().BeTrue();
collection.Should().Contain(x => x.Name == "Test");
user.Email.Should().Be("test@example.com");

// ❌ Basic Assert - Less readable error messages
Assert.True(result.IsSuccess);
Assert.Contains(collection, x => x.Name == "Test");
Assert.Equal("test@example.com", user.Email);
```

### Mocking Strategy

Mock external dependencies, not your own abstractions:

```csharp
// ✅ Good - Mock external dependency
var smtpClientMock = new Mock<ISmtpClient>();
smtpClientMock.Setup(x => x.SendAsync(...)).ReturnsAsync(true);

// ❌ Bad - Mocking your own service (test the real implementation instead)
var serviceMock = new Mock<IEmailService>();
```

### Performance Tests

Mark performance tests appropriately:

```csharp
[Trait("Category", "Performance")]
public class LookupPerformanceTests
{
    [Fact]
    public void GetById_ShouldCompleteIn_LessThan100Microseconds()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var result = SecurityMethods.GetById(1);
        }

        stopwatch.Stop();

        // Assert
        var averageMicroseconds = stopwatch.ElapsedTicks / 1000.0 / (Stopwatch.Frequency / 1_000_000.0);
        averageMicroseconds.ShouldBeLessThan(100);
    }
}
```

Run performance tests separately:
```bash
dotnet test --filter "Category=Performance"
```

---

## Summary

The FractalDataWorks Developer Kit testing strategy emphasizes:

1. **xUnit v3**: Modern test framework with async support
2. **High Coverage**: 70-90% code coverage across projects
3. **Clear Structure**: Arrange-Act-Assert pattern
4. **Independence**: No shared state between tests
5. **Generator Testing**: Special approaches for source generators
6. **Fast Execution**: Unit tests <100ms, integration tests <1s

Follow these guidelines to maintain high-quality, maintainable test suites that provide confidence in the framework's reliability.
