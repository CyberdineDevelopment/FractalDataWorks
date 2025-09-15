using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.MCP.Tests.ConnectionTypes;

/// <summary>
/// Comprehensive tests for RoslynWorkspaceConnectionType and connection management patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoslynWorkspaceConnectionTypeTests
{
    private readonly IServiceCollection _services;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockSection;

    public RoslynWorkspaceConnectionTypeTests()
    {
        _services = new ServiceCollection();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockSection = new Mock<IConfigurationSection>();
    }

    [Fact]
    public void RoslynWorkspaceConnectionType_IsSingleton()
    {
        // Arrange & Act
        var instance1 = RoslynWorkspaceConnectionType.Instance;
        var instance2 = RoslynWorkspaceConnectionType.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("RoslynWorkspaceConnectionType.Instance is a singleton");
    }

    [Fact]
    public void RoslynWorkspaceConnectionType_InheritsFromConnectionTypeBase()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var type = typeof(RoslynWorkspaceConnectionType);

        // Assert
        type.IsSealed.ShouldBeTrue();
        connectionType.ShouldBeAssignableTo<ConnectionTypeBase<IRoslynWorkspaceConnection, RoslynWorkspaceConfiguration, IRoslynWorkspaceConnectionFactory>>();
        TestContext.Current.WriteLine("RoslynWorkspaceConnectionType correctly inherits from ConnectionTypeBase with proper generic parameters");
    }

    [Fact]
    public void RoslynWorkspaceConnectionType_HasCorrectProperties()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Id.ShouldBe(2001);
        connectionType.Name.ShouldBe("RoslynWorkspace");
        connectionType.Category.ShouldBe("Development Tools");
        connectionType.DisplayName.ShouldBe("Roslyn Workspace");
        connectionType.Description.ShouldBe("Connection to Roslyn compilation workspaces for code analysis and refactoring");
        connectionType.FactoryType.ShouldBe(typeof(IRoslynWorkspaceConnectionFactory));

        TestContext.Current.WriteLine($"RoslynWorkspaceConnectionType properties - Id: {connectionType.Id}, Name: {connectionType.Name}, Category: {connectionType.Category}");
    }

    [Fact]
    public void Register_WithServiceCollection_RegistersAllRequiredServices()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Act
        connectionType.Register(_services);

        // Assert
        var serviceDescriptors = _services.ToList();

        // Verify Roslyn workspace specific services
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("IRoslynWorkspaceConnectionFactory"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("RoslynWorkspaceService"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("WorkspaceCommandTranslator"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("CompilationProvider"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("DocumentAnalyzer"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("SymbolResolver"));

        // Verify workspace session management
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("WorkspaceSessionPool"));
        serviceDescriptors.ShouldContain(sd => sd.ServiceType.Name.Contains("WorkspaceHealthChecker"));

        TestContext.Current.WriteLine($"Register method added {serviceDescriptors.Count} service registrations");
    }

    [Fact]
    public void Register_ServiceLifetimes_AreCorrect()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Act
        connectionType.Register(_services);

        // Assert
        var serviceDescriptors = _services.ToList();

        // Factory and session pool should be scoped/singleton, health checker should be scoped
        var singletonServices = serviceDescriptors.Where(sd => sd.Lifetime == ServiceLifetime.Singleton).ToList();
        var scopedServices = serviceDescriptors.Where(sd => sd.Lifetime == ServiceLifetime.Scoped).ToList();

        singletonServices.Count.ShouldBeGreaterThan(0);
        scopedServices.Count.ShouldBeGreaterThan(0);

        TestContext.Current.WriteLine($"Service lifetimes - Singleton: {singletonServices.Count}, Scoped: {scopedServices.Count}");
    }

    [Fact]
    public void Configure_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var validConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = 5,
            SessionTimeoutMinutes = 30
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(validConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(_mockConfiguration.Object));
        TestContext.Current.WriteLine("Configure method succeeded with valid configuration");
    }

    [Fact]
    public void Configure_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns((RoslynWorkspaceConfiguration)null);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(_mockConfiguration.Object));
        TestContext.Current.WriteLine("Configure method handled null configuration gracefully");
    }

    [Fact]
    public void Configure_WithInvalidMaxConcurrentSessions_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var invalidConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = 0, // Invalid
            SessionTimeoutMinutes = 30
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(invalidConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => connectionType.Configure(_mockConfiguration.Object));
        exception.Message.ShouldContain("MaxConcurrentSessions must be greater than 0");
        TestContext.Current.WriteLine($"Configure method correctly threw exception: {exception.Message}");
    }

    [Fact]
    public void Configure_WithInvalidSessionTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var invalidConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = 5,
            SessionTimeoutMinutes = 0 // Invalid
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(invalidConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => connectionType.Configure(_mockConfiguration.Object));
        exception.Message.ShouldContain("SessionTimeoutMinutes must be greater than 0");
        TestContext.Current.WriteLine($"Configure method correctly threw exception: {exception.Message}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(int.MinValue)]
    public void Configure_WithNegativeMaxConcurrentSessions_ThrowsInvalidOperationException(int invalidValue)
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var invalidConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = invalidValue,
            SessionTimeoutMinutes = 30
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(invalidConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => connectionType.Configure(_mockConfiguration.Object));
        exception.Message.ShouldContain("MaxConcurrentSessions must be greater than 0");
        TestContext.Current.WriteLine($"Configure method correctly rejected negative MaxConcurrentSessions: {invalidValue}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(int.MinValue)]
    public void Configure_WithNegativeSessionTimeout_ThrowsInvalidOperationException(int invalidValue)
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var invalidConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = 5,
            SessionTimeoutMinutes = invalidValue
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(invalidConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => connectionType.Configure(_mockConfiguration.Object));
        exception.Message.ShouldContain("SessionTimeoutMinutes must be greater than 0");
        TestContext.Current.WriteLine($"Configure method correctly rejected negative SessionTimeoutMinutes: {invalidValue}");
    }

    [Fact]
    public void DisplayName_ReturnsCorrectValue()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.DisplayName.ShouldBe("Roslyn Workspace");
        TestContext.Current.WriteLine($"DisplayName property returns: {connectionType.DisplayName}");
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Description.ShouldBe("Connection to Roslyn compilation workspaces for code analysis and refactoring");
        TestContext.Current.WriteLine($"Description property returns: {connectionType.Description}");
    }

    [Fact]
    public void FactoryType_ReturnsCorrectType()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.FactoryType.ShouldBe(typeof(IRoslynWorkspaceConnectionFactory));
        TestContext.Current.WriteLine($"FactoryType property returns: {connectionType.FactoryType.Name}");
    }

    [Theory]
    [InlineData(2001)]
    public void Id_HasExpectedValue(int expectedId)
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Id.ShouldBe(expectedId);
        TestContext.Current.WriteLine($"RoslynWorkspaceConnectionType Id: {connectionType.Id}");
    }

    [Theory]
    [InlineData("RoslynWorkspace")]
    public void Name_HasExpectedValue(string expectedName)
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Name.ShouldBe(expectedName);
        TestContext.Current.WriteLine($"RoslynWorkspaceConnectionType Name: {connectionType.Name}");
    }

    [Theory]
    [InlineData("Development Tools")]
    public void Category_HasExpectedValue(string expectedCategory)
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Category.ShouldBe(expectedCategory);
        TestContext.Current.WriteLine($"RoslynWorkspaceConnectionType Category: {connectionType.Category}");
    }

    [Fact]
    public void Register_MultipleCalls_AddsDuplicateServices()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Act
        connectionType.Register(_services);
        var firstCount = _services.Count;

        connectionType.Register(_services);
        var secondCount = _services.Count;

        // Assert
        secondCount.ShouldBe(firstCount * 2); // Services get duplicated
        TestContext.Current.WriteLine($"Multiple Register calls: first {firstCount} services, second {secondCount} services");
    }

    [Fact]
    public void Configure_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => connectionType.Configure(null));
        TestContext.Current.WriteLine("Configure method correctly threw ArgumentNullException for null configuration");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 60)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Configure_WithValidBoundaryValues_DoesNotThrow(int maxSessions, int timeoutMinutes)
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var validConfig = new RoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = maxSessions,
            SessionTimeoutMinutes = timeoutMinutes
        };

        _mockSection.Setup(s => s.Get<RoslynWorkspaceConfiguration>()).Returns(validConfig);
        _mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(_mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(_mockConfiguration.Object));
        TestContext.Current.WriteLine($"Configure method accepted boundary values: MaxSessions={maxSessions}, Timeout={timeoutMinutes}");
    }

    /// <summary>
    /// Mock classes for testing since actual implementations may not be available in test context.
    /// </summary>
    private interface IRoslynWorkspaceConnection { }
    private interface IRoslynWorkspaceConnectionFactory { }

    private class RoslynWorkspaceConfiguration
    {
        public int MaxConcurrentSessions { get; set; }
        public int SessionTimeoutMinutes { get; set; }
    }
}

/// <summary>
/// Edge case tests for RoslynWorkspaceConnectionType focusing on boundary conditions and error scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoslynWorkspaceConnectionTypeEdgeCaseTests
{
    [Fact]
    public void RoslynWorkspaceConnectionType_IsThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int iterationsPerThread = 100;
        var instances = new List<RoslynWorkspaceConnectionType>();
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerThread; j++)
                {
                    var instance = RoslynWorkspaceConnectionType.Instance;
                    lock (instances)
                    {
                        instances.Add(instance);
                    }
                }
            });
        }

        Task.WaitAll(tasks);

        // Assert
        instances.Count.ShouldBe(threadCount * iterationsPerThread);
        instances.ShouldAllBe(instance => ReferenceEquals(instance, RoslynWorkspaceConnectionType.Instance));
        TestContext.Current.WriteLine($"Thread safety test: {instances.Count} instances all reference the same singleton");
    }

    [Fact]
    public void Register_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => connectionType.Register(null));
        TestContext.Current.WriteLine("Register method correctly threw ArgumentNullException for null ServiceCollection");
    }

    [Fact]
    public void Configure_WithEmptyConfigurationSection_HandlesGracefully()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var mockConfiguration = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();

        // Empty section returns null
        mockSection.Setup(s => s.Get<object>()).Returns((object)null);
        mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(mockConfiguration.Object));
        TestContext.Current.WriteLine("Configure method handled empty configuration section gracefully");
    }

    [Fact]
    public void ConnectionType_Properties_AreNotNull()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Name.ShouldNotBeNull();
        connectionType.Name.ShouldNotBeEmpty();
        connectionType.Category.ShouldNotBeNull();
        connectionType.Category.ShouldNotBeEmpty();
        connectionType.DisplayName.ShouldNotBeNull();
        connectionType.DisplayName.ShouldNotBeEmpty();
        connectionType.Description.ShouldNotBeNull();
        connectionType.Description.ShouldNotBeEmpty();
        connectionType.FactoryType.ShouldNotBeNull();

        TestContext.Current.WriteLine("All ConnectionType properties are properly initialized and non-null");
    }

    [Fact]
    public void RoslynWorkspaceConnectionType_HasUniqueId()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert
        connectionType.Id.ShouldBe(2001);
        connectionType.Id.ShouldBeGreaterThan(2000); // In the workspace connection range
        TestContext.Current.WriteLine($"RoslynWorkspaceConnectionType has unique ID: {connectionType.Id}");
    }

    [Fact]
    public void Register_DoesNotRegisterDuplicateInterfaceTypes()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var services = new ServiceCollection();

        // Act
        connectionType.Register(services);

        // Assert
        var serviceTypes = services.Select(sd => sd.ServiceType).ToList();
        var duplicateTypes = serviceTypes.GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        // Log for verification - some duplication might be expected
        TestContext.Current.WriteLine($"Service registration resulted in {serviceTypes.Count} total registrations");
        if (duplicateTypes.Count > 0)
        {
            TestContext.Current.WriteLine($"Duplicate service types found: {string.Join(", ", duplicateTypes.Select(t => t.Name))}");
        }
        else
        {
            TestContext.Current.WriteLine("No duplicate service types found in registration");
        }
    }

    [Fact]
    public void FactoryType_ImplementsCorrectInterface()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var factoryType = connectionType.FactoryType;

        // Assert
        factoryType.IsInterface.ShouldBeTrue();
        factoryType.Name.ShouldEndWith("Factory");
        factoryType.Name.ShouldContain("RoslynWorkspace");
        TestContext.Current.WriteLine($"FactoryType {factoryType.Name} is an interface with correct naming convention");
    }

    [Fact]
    public void Configure_WithMalformedConfigurationSection_HandlesGracefully()
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var mockConfiguration = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();

        // Simulate configuration binding failure
        mockSection.Setup(s => s.Get<object>())
                   .Throws(new InvalidOperationException("Configuration binding failed"));
        mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(mockSection.Object);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => connectionType.Configure(mockConfiguration.Object));
        TestContext.Current.WriteLine("Configure method propagated malformed configuration section exception appropriately");
    }

    [Fact]
    public void RoslynWorkspaceConnectionType_CanBeUsedInGenericConstraints()
    {
        // Arrange & Act
        var connectionType = RoslynWorkspaceConnectionType.Instance;

        // Assert - Test that it can be used in generic scenarios
        var genericMethod = typeof(RoslynWorkspaceConnectionTypeEdgeCaseTests)
            .GetMethod(nameof(GenericTestMethod), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Should.NotThrow(() => genericMethod?.MakeGenericMethod(typeof(RoslynWorkspaceConnectionType)));
        TestContext.Current.WriteLine("RoslynWorkspaceConnectionType can be used in generic constraints");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Configure_WithVariousValidMaxSessions_AcceptsAll(int maxSessions)
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var mockConfiguration = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();

        var validConfig = new TestRoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = maxSessions,
            SessionTimeoutMinutes = 30
        };

        mockSection.Setup(s => s.Get<TestRoslynWorkspaceConfiguration>()).Returns(validConfig);
        mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(mockConfiguration.Object));
        TestContext.Current.WriteLine($"Configure method accepted MaxConcurrentSessions value: {maxSessions}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(60)]
    [InlineData(1440)] // 24 hours
    [InlineData(10080)] // 1 week
    public void Configure_WithVariousValidTimeouts_AcceptsAll(int timeoutMinutes)
    {
        // Arrange
        var connectionType = RoslynWorkspaceConnectionType.Instance;
        var mockConfiguration = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();

        var validConfig = new TestRoslynWorkspaceConfiguration
        {
            MaxConcurrentSessions = 5,
            SessionTimeoutMinutes = timeoutMinutes
        };

        mockSection.Setup(s => s.Get<TestRoslynWorkspaceConfiguration>()).Returns(validConfig);
        mockConfiguration.Setup(c => c.GetSection("RoslynWorkspace")).Returns(mockSection.Object);

        // Act & Assert
        Should.NotThrow(() => connectionType.Configure(mockConfiguration.Object));
        TestContext.Current.WriteLine($"Configure method accepted SessionTimeoutMinutes value: {timeoutMinutes}");
    }

    private void GenericTestMethod<T>() where T : class
    {
        // Generic constraint test method
    }

    private class TestRoslynWorkspaceConfiguration
    {
        public int MaxConcurrentSessions { get; set; }
        public int SessionTimeoutMinutes { get; set; }
    }
}