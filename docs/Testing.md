# Testing Patterns and Best Practices

## Table of Contents
- [Overview](#overview)
- [Testing Philosophy](#testing-philosophy)
- [Unit Testing Patterns](#unit-testing-patterns)
- [Integration Testing](#integration-testing)
- [Enhanced Enum Testing](#enhanced-enum-testing)
- [Service Testing](#service-testing)
- [Configuration Testing](#configuration-testing)
- [Mock and Stub Patterns](#mock-and-stub-patterns)
- [Test Data Management](#test-data-management)
- [Performance Testing](#performance-testing)
- [Best Practices](#best-practices)

## Overview

The FractalDataWorks Framework follows testing best practices with a focus on:
- **xUnit v3** as the testing framework
- **Shouldly** for assertions with natural language
- **One assertion per test** for clear test failures
- **Comprehensive test output** with diagnostic information
- **Isolated tests** that don't depend on external resources

## Testing Philosophy

### Core Principles

1. **Arrange, Act, Assert (AAA)** - Clear test structure
2. **One Thing Per Test** - Single responsibility per test method
3. **Descriptive Test Names** - No underscores, clear intent
4. **Fast and Reliable** - Tests should run quickly and consistently
5. **Independent Tests** - No test order dependencies

### Test Categories

- **Unit Tests** - Test individual components in isolation
- **Integration Tests** - Test component interactions
- **End-to-End Tests** - Test complete workflows
- **Performance Tests** - Validate performance characteristics

## Unit Testing Patterns

### Basic Service Testing

```csharp
public class EmailServiceTests
{
    private readonly Mock<EmailExecutor> _mockExecutor;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailService _emailService;
    
    public EmailServiceTests()
    {
        _mockExecutor = new Mock<EmailExecutor>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_mockExecutor.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";
        
        _mockExecutor
            .Setup(x => x.SendEmailAsync(recipient, subject, body, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _emailService.SendEmailAsync(recipient, subject, body);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        _mockExecutor.Verify(x => x.SendEmailAsync(recipient, subject, body, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task SendEmailAsync_WhenExecutorThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";
        var expectedException = new InvalidOperationException("SMTP connection failed");
        
        _mockExecutor
            .Setup(x => x.SendEmailAsync(recipient, subject, body, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);
        
        // Act
        var result = await _emailService.SendEmailAsync(recipient, subject, body);
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("SMTP connection failed");
    }
}
```

### Testing with Enhanced Enum Messages

```csharp
public class NotificationServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly NotificationService _notificationService;
    
    public NotificationServiceTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger<NotificationService>>());
        _mockEmailService = new Mock<IEmailService>();
        services.AddSingleton(_mockEmailService.Object);
        
        _serviceProvider = services.BuildServiceProvider();
        _notificationService = new NotificationService(_mockEmailService.Object, 
            _serviceProvider.GetRequiredService<ILogger<NotificationService>>());
    }
    
    [Fact]
    public async Task SendNotificationAsync_WhenEmailServiceSucceeds_ShouldReturnSuccessMessage()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Recipient = "user@example.com",
            Subject = "Welcome",
            Body = "Welcome to our service"
        };
        
        _mockEmailService
            .Setup(x => x.SendEmailAsync(request.Recipient, request.Subject, request.Body, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Success());
        
        // Act
        var result = await _notificationService.SendNotificationAsync(request);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(NotificationMessages.NotificationSentSuccessfully());
    }
    
    [Fact]
    public async Task SendNotificationAsync_WhenEmailServiceFails_ShouldReturnFailureMessage()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Recipient = "invalid-email",
            Subject = "Test",
            Body = "Test"
        };
        
        _mockEmailService
            .Setup(x => x.SendEmailAsync(request.Recipient, request.Subject, request.Body, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Failure("Invalid email address"));
        
        // Act
        var result = await _notificationService.SendNotificationAsync(request);
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(NotificationMessages.NotificationDeliveryFailed("Invalid email address"));
    }
    
    public void Dispose() => _serviceProvider?.Dispose();
}
```

## Integration Testing

### Database Integration Testing

```csharp
[Collection("Database")]
public class UserRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly UserRepository _repository;
    
    public UserRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(_fixture.Context, Mock.Of<ILogger<UserRepository>>());
    }
    
    [Fact]
    public async Task CreateUserAsync_WithValidUser_ShouldPersistToDatabase()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        var result = await _repository.CreateUserAsync(user);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        // Verify persistence
        var savedUser = await _fixture.Context.Users.FindAsync(user.Id);
        savedUser.ShouldNotBeNull();
        savedUser.Email.ShouldBe(user.Email);
        savedUser.Name.ShouldBe(user.Name);
    }
    
    [Fact]
    public async Task GetUserByEmailAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            Name = "Existing User"
        };
        
        _fixture.Context.Users.Add(user);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetUserByEmailAsync(user.Email);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Email.ShouldBe(user.Email);
    }
}

public class DatabaseFixture : IDisposable
{
    public TestDbContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        Context = new TestDbContext(options);
        Context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        Context?.Dispose();
    }
}
```

### API Integration Testing

```csharp
public class NotificationControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public NotificationControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real email service with mock for testing
                services.RemoveAll<IEmailService>();
                services.AddSingleton(Mock.Of<IEmailService>(m =>
                    m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()) ==
                    Task.FromResult(FdwResult.Success())));
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task PostNotification_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Recipient = "test@example.com",
            Subject = "Test Notification",
            Body = "This is a test notification."
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/notifications", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<NotificationResponse>(responseContent);
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
    }
    
    [Fact]
    public async Task PostNotification_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Recipient = "", // Invalid empty email
            Subject = "Test",
            Body = "Test"
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/notifications", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
```

## Enhanced Enum Testing

### Testing Enhanced Enum Collections

```csharp
public class PriorityEnumTests
{
    [Fact]
    public void Priorities_ShouldContainExpectedValues()
    {
        // Act
        var allPriorities = Priorities.All();
        
        // Assert
        allPriorities.Length.ShouldBe(3);
        allPriorities.ShouldContain(p => p.Name == "High");
        allPriorities.ShouldContain(p => p.Name == "Medium");
        allPriorities.ShouldContain(p => p.Name == "Low");
    }
    
    [Fact]
    public void Priority_GetByName_WithValidName_ShouldReturnCorrectPriority()
    {
        // Act
        var priority = Priorities.GetByName("High");
        
        // Assert
        priority.ShouldNotBeNull();
        priority.Name.ShouldBe("High");
        priority.Level.ShouldBe(100);
    }
    
    [Fact]
    public void Priority_GetByName_WithInvalidName_ShouldReturnNull()
    {
        // Act
        var priority = Priorities.GetByName("Invalid");
        
        // Assert
        priority.ShouldBeNull();
    }
    
    [Theory]
    [InlineData("High", 100)]
    [InlineData("Medium", 50)]
    [InlineData("Low", 10)]
    public void Priority_ShouldHaveCorrectLevel(string name, int expectedLevel)
    {
        // Act
        var priority = Priorities.GetByName(name);
        
        // Assert
        priority.ShouldNotBeNull();
        priority.Level.ShouldBe(expectedLevel);
    }
}
```

### Testing Enhanced Enum Business Logic

```csharp
public class PaymentMethodTests
{
    [Fact]
    public void CreditCard_ShouldRequireVerification()
    {
        // Arrange & Act
        var creditCard = PaymentMethods.CreditCard();
        
        // Assert
        creditCard.RequiresVerification.ShouldBeTrue();
        creditCard.ProcessingFee.ShouldBe(2.9m);
        creditCard.ProcessorName.ShouldBe("Stripe");
    }
    
    [Fact]
    public void BankTransfer_ShouldNotRequireVerification()
    {
        // Arrange & Act
        var bankTransfer = PaymentMethods.BankTransfer();
        
        // Assert
        bankTransfer.RequiresVerification.ShouldBeFalse();
        bankTransfer.ProcessingFee.ShouldBe(0.5m);
        bankTransfer.ProcessorName.ShouldBe("ACH");
    }
    
    [Theory]
    [InlineData(100.00, 2.90)] // Credit card
    [InlineData(100.00, 0.50)] // Bank transfer
    public void PaymentMethod_CalculateProcessingFee_ShouldReturnCorrectFee(decimal amount, decimal expectedFee)
    {
        // Arrange
        var paymentMethods = new[] { PaymentMethods.CreditCard(), PaymentMethods.BankTransfer() };
        var paymentMethod = paymentMethods.First(pm => pm.ProcessingFee == expectedFee);
        
        // Act
        var fee = amount * (paymentMethod.ProcessingFee / 100);
        
        // Assert
        fee.ShouldBe(expectedFee);
    }
}
```

## Service Testing

### Testing Service Base Classes

```csharp
public class ServiceBaseTests
{
    private readonly Mock<TestExecutor> _mockExecutor;
    private readonly Mock<ILogger<TestService>> _mockLogger;
    private readonly TestService _service;
    
    public ServiceBaseTests()
    {
        _mockExecutor = new Mock<TestExecutor>();
        _mockLogger = new Mock<ILogger<TestService>>();
        _service = new TestService(_mockExecutor.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task ProcessAsync_ShouldStartAndDisposeActivity()
    {
        // Arrange
        _mockExecutor
            .Setup(x => x.ExecuteAsync(It.IsAny<TestConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult { Success = true });
        
        // Act
        var result = await _service.ProcessAsync(new TestConfiguration());
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        // Verify activity was started (indirectly through logging)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task ProcessAsync_WhenExecutorThrowsException_ShouldLogErrorAndReturnFailure()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _mockExecutor
            .Setup(x => x.ExecuteAsync(It.IsAny<TestConfiguration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);
        
        // Act
        var result = await _service.ProcessAsync(new TestConfiguration());
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Test exception");
        
        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
```

### Testing Service Factories

```csharp
public class EmailServiceFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<EmailServiceFactory>> _mockLogger;
    private readonly EmailServiceFactory _factory;
    
    public EmailServiceFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<EmailServiceFactory>>();
        _factory = new EmailServiceFactory(_mockServiceProvider.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateServiceAsync_WithValidConfiguration_ShouldReturnService()
    {
        // Arrange
        var configuration = new EmailServiceConfiguration
        {
            SmtpHost = "smtp.test.com",
            Port = 587,
            Username = "test@test.com",
            Password = "password"
        };
        
        var mockExecutor = Mock.Of<EmailExecutor>();
        var mockServiceLogger = Mock.Of<ILogger<EmailService>>();
        
        _mockServiceProvider
            .Setup(x => x.GetRequiredService<EmailExecutor>())
            .Returns(mockExecutor);
        _mockServiceProvider
            .Setup(x => x.GetRequiredService<ILogger<EmailService>>())
            .Returns(mockServiceLogger);
        
        // Act
        var result = await _factory.CreateServiceAsync(configuration);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<EmailService>();
    }
}
```

## Configuration Testing

### Testing Configuration Validation

```csharp
public class EmailServiceConfigurationTests
{
    private readonly EmailServiceConfigurationValidator _validator;
    
    public EmailServiceConfigurationTests()
    {
        _validator = new EmailServiceConfigurationValidator();
    }
    
    [Fact]
    public async Task Validate_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var configuration = new EmailServiceConfiguration
        {
            SmtpHost = "smtp.gmail.com",
            Port = 587,
            UseSsl = true,
            Username = "test@gmail.com",
            Password = "password"
        };
        
        // Act
        var result = await _validator.ValidateAsync(configuration);
        
        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    
    [Theory]
    [InlineData("", 587, true, "user", "pass", "SMTP host is required")]
    [InlineData("smtp.gmail.com", 0, true, "user", "pass", "Port must be valid")]
    [InlineData("smtp.gmail.com", 587, true, "", "pass", "Username is required")]
    [InlineData("smtp.gmail.com", 587, true, "user", "", "Password is required")]
    public async Task Validate_WithInvalidConfiguration_ShouldReturnValidationErrors(
        string smtpHost, int port, bool useSsl, string username, string password, string expectedError)
    {
        // Arrange
        var configuration = new EmailServiceConfiguration
        {
            SmtpHost = smtpHost,
            Port = port,
            UseSsl = useSsl,
            Username = username,
            Password = password
        };
        
        // Act
        var result = await _validator.ValidateAsync(configuration);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains(expectedError));
    }
}
```

## Mock and Stub Patterns

### Mock Setup Patterns

```csharp
public class MockSetupExamples
{
    [Fact]
    public void MockSetup_BasicReturn()
    {
        // Arrange
        var mock = new Mock<IEmailService>();
        mock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Success());
        
        // Use mock
        var service = mock.Object;
        // ... test logic
    }
    
    [Fact]
    public void MockSetup_ConditionalReturn()
    {
        // Arrange
        var mock = new Mock<IEmailService>();
        mock.Setup(x => x.SendEmailAsync(
                It.Is<string>(email => email.Contains("@valid.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Success());
        
        mock.Setup(x => x.SendEmailAsync(
                It.Is<string>(email => !email.Contains("@valid.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Failure("Invalid email domain"));
        
        // Use mock with conditional behavior
    }
    
    [Fact]
    public void MockSetup_SequenceReturns()
    {
        // Arrange
        var mock = new Mock<IEmailService>();
        mock.SetupSequence(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult.Success())           // First call succeeds
            .ReturnsAsync(FdwResult.Failure("Timeout"))  // Second call fails
            .ReturnsAsync(FdwResult.Success());          // Third call succeeds
        
        // Use mock with sequence behavior
    }
}
```

### Test Double Patterns

```csharp
// Stub - Returns predetermined values
public class StubEmailService : IEmailService
{
    public Task<IFdwResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult.Success());
    }
    
    public Task<IFdwResult<ValidationResult>> ValidateConfigurationAsync(EmailServiceConfiguration config, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult<ValidationResult>.Success(new ValidationResult()));
    }
}

// Fake - Working implementation with simplified logic
public class FakeEmailService : IEmailService
{
    private readonly List<SentEmail> _sentEmails = new();
    
    public IReadOnlyList<SentEmail> SentEmails => _sentEmails.AsReadOnly();
    
    public Task<IFdwResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(to) || !to.Contains("@"))
        {
            return Task.FromResult(FdwResult.Failure("Invalid email address"));
        }
        
        _sentEmails.Add(new SentEmail(to, subject, body, DateTime.UtcNow));
        return Task.FromResult(FdwResult.Success());
    }
}

public record SentEmail(string To, string Subject, string Body, DateTime SentAt);
```

## Test Data Management

### Test Data Builders

```csharp
public class UserTestDataBuilder
{
    private User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        Name = "Test User",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
    
    public UserTestDataBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }
    
    public UserTestDataBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }
    
    public UserTestDataBuilder Inactive()
    {
        _user.IsActive = false;
        return this;
    }
    
    public UserTestDataBuilder CreatedAt(DateTime createdAt)
    {
        _user.CreatedAt = createdAt;
        return this;
    }
    
    public User Build() => _user;
    
    public static implicit operator User(UserTestDataBuilder builder) => builder.Build();
}

// Usage
[Fact]
public void TestWithBuilder()
{
    // Arrange
    User user = new UserTestDataBuilder()
        .WithEmail("john@example.com")
        .WithName("John Doe")
        .CreatedAt(DateTime.UtcNow.AddDays(-7));
    
    // Act & Assert
    user.Email.ShouldBe("john@example.com");
    user.Name.ShouldBe("John Doe");
    user.CreatedAt.ShouldBeLessThan(DateTime.UtcNow);
}
```

### Object Mother Pattern

```csharp
public static class TestUsers
{
    public static User ValidUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "valid@example.com",
        Name = "Valid User",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
    
    public static User AdminUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "admin@example.com",
        Name = "Admin User",
        CreatedAt = DateTime.UtcNow,
        IsActive = true,
        Role = UserRoles.Administrator()
    };
    
    public static User InactiveUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "inactive@example.com",
        Name = "Inactive User",
        CreatedAt = DateTime.UtcNow.AddMonths(-6),
        IsActive = false
    };
}
```

## Performance Testing

### Benchmark Testing

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class EnhancedEnumPerformanceBenchmarks
{
    private readonly Priority[] _priorities = Priorities.All();
    
    [Benchmark]
    public Priority GetByNameDictionary()
    {
        return Priorities.GetByName("High");
    }
    
    [Benchmark]
    public Priority GetByIdDictionary()
    {
        return Priorities.GetById(1);
    }
    
    [Benchmark]
    public Priority[] GetAllValues()
    {
        return Priorities.All();
    }
    
    [Benchmark]
    [Arguments("High")]
    [Arguments("Medium")]
    [Arguments("Low")]
    public Priority GetByNameParameterized(string name)
    {
        return Priorities.GetByName(name);
    }
}
```

### Load Testing

```csharp
[Fact]
public async Task EmailService_ShouldHandleConcurrentRequests()
{
    // Arrange
    var service = CreateEmailService();
    var tasks = new List<Task<IFdwResult>>();
    const int concurrentRequests = 100;
    
    // Act
    for (int i = 0; i < concurrentRequests; i++)
    {
        var task = service.SendEmailAsync($"user{i}@example.com", "Test", "Body");
        tasks.Add(task);
    }
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    results.All(r => r.IsSuccess).ShouldBeTrue();
    results.Length.ShouldBe(concurrentRequests);
}
```

## Best Practices

### 1. Test Organization

```csharp
// Group related tests in nested classes
public class EmailServiceTests
{
    public class SendEmailAsync
    {
        [Fact]
        public async Task WithValidParameters_ShouldReturnSuccess() { }
        
        [Fact]
        public async Task WithInvalidEmail_ShouldReturnFailure() { }
        
        [Fact]
        public async Task WhenCancelled_ShouldThrowOperationCancelledException() { }
    }
    
    public class ValidateConfigurationAsync
    {
        [Fact]
        public async Task WithValidConfiguration_ShouldReturnSuccess() { }
        
        [Fact]
        public async Task WithMissingSmtpHost_ShouldReturnValidationError() { }
    }
}
```

### 2. Assertion Quality

```csharp
// Good assertions with descriptive messages
result.IsSuccess.ShouldBeTrue("Email service should successfully send valid emails");
result.Value.Count.ShouldBe(3, "Should return exactly 3 priority levels");
user.Email.ShouldBe(expectedEmail, "User email should match the expected value");

// Use collection assertions
users.ShouldContain(u => u.Email == "test@example.com", "Users should contain the test user");
errors.ShouldAllBe(e => !string.IsNullOrEmpty(e.Message), "All errors should have messages");
```

### 3. Test Data Isolation

```csharp
// Each test should create its own data
[Fact]
public async Task CreateUser_ShouldPersistCorrectly()
{
    // Create unique test data for this test
    var uniqueId = Guid.NewGuid();
    var user = new UserTestDataBuilder()
        .WithEmail($"test-{uniqueId}@example.com")
        .WithName($"Test User {uniqueId}")
        .Build();
    
    // Test logic...
}
```

### 4. Test Output Diagnostics

```csharp
[Fact]
public async Task ComplexBusinessLogic_ShouldProduceExpectedResults()
{
    // Arrange
    var input = CreateComplexTestData();
    
    // Act
    var result = await _service.ProcessComplexData(input);
    
    // Assert
    result.IsSuccess.ShouldBeTrue();
    
    // Diagnostic output for debugging failures
    _testOutputHelper.WriteLine($"Input data: {JsonSerializer.Serialize(input, new JsonSerializerOptions { WriteIndented = true })}");
    _testOutputHelper.WriteLine($"Result: {JsonSerializer.Serialize(result.Value, new JsonSerializerOptions { WriteIndented = true })}");
    
    result.Value.ProcessedItems.Count.ShouldBe(input.Items.Count, 
        $"Expected {input.Items.Count} processed items, but got {result.Value.ProcessedItems.Count}");
}
```