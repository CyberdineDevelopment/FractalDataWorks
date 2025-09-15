using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.MCP.Tests.PluginSystem;

/// <summary>
/// Comprehensive tests for plugin registry functionality and plugin discovery mechanisms.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginRegistryTests
{
    private readonly Mock<IPluginRegistry> _mockRegistry;
    private readonly Mock<IToolPlugin> _mockPlugin1;
    private readonly Mock<IToolPlugin> _mockPlugin2;
    private readonly Mock<IToolPlugin> _mockPlugin3;
    private readonly CancellationToken _cancellationToken;

    public PluginRegistryTests()
    {
        _mockRegistry = new Mock<IPluginRegistry>();
        _mockPlugin1 = CreateMockPlugin("Plugin1", "Test Plugin 1", SessionManagement.Instance, 1, true);
        _mockPlugin2 = CreateMockPlugin("Plugin2", "Test Plugin 2", CodeAnalysis.Instance, 2, true);
        _mockPlugin3 = CreateMockPlugin("Plugin3", "Test Plugin 3", VirtualEditing.Instance, 3, false);
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    private static Mock<IToolPlugin> CreateMockPlugin(string id, string name, ToolCategoryBase category, int priority, bool isEnabled)
    {
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(id);
        mockPlugin.Setup(p => p.Name).Returns(name);
        mockPlugin.Setup(p => p.Description).Returns($"Description for {name}");
        mockPlugin.Setup(p => p.Category).Returns(category);
        mockPlugin.Setup(p => p.Priority).Returns(priority);
        mockPlugin.Setup(p => p.IsEnabled).Returns(isEnabled);
        mockPlugin.Setup(p => p.GetTools()).Returns(new List<IMcpTool>());
        return mockPlugin;
    }

    [Fact]
    public async Task RegisterPluginAsync_WithValidPlugin_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockRegistry.Setup(r => r.RegisterPluginAsync(_mockPlugin1.Object, _cancellationToken))
                    .ReturnsAsync(successResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(_mockPlugin1.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"Successfully registered plugin: {_mockPlugin1.Object.Name}");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithNullPlugin_ReturnsFailure()
    {
        // Arrange
        var failureResult = FdwResult.Failure("Plugin cannot be null");
        _mockRegistry.Setup(r => r.RegisterPluginAsync(null, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(null, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Plugin cannot be null");
        TestContext.Current.WriteLine("Registry correctly rejected null plugin");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithDuplicatePlugin_ReturnsFailure()
    {
        // Arrange
        var failureResult = FdwResult.Failure($"Plugin with ID '{_mockPlugin1.Object.Id}' is already registered");
        _mockRegistry.Setup(r => r.RegisterPluginAsync(_mockPlugin1.Object, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(_mockPlugin1.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("already registered");
        TestContext.Current.WriteLine("Registry correctly rejected duplicate plugin registration");
    }

    [Fact]
    public async Task UnregisterPluginAsync_WithValidPluginId_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockRegistry.Setup(r => r.UnregisterPluginAsync(_mockPlugin1.Object.Id, _cancellationToken))
                    .ReturnsAsync(successResult);

        // Act
        var result = await _mockRegistry.Object.UnregisterPluginAsync(_mockPlugin1.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"Successfully unregistered plugin: {_mockPlugin1.Object.Id}");
    }

    [Fact]
    public async Task UnregisterPluginAsync_WithNonExistentPluginId_ReturnsFailure()
    {
        // Arrange
        var nonExistentId = "NonExistentPlugin";
        var failureResult = FdwResult.Failure($"Plugin with ID '{nonExistentId}' not found");
        _mockRegistry.Setup(r => r.UnregisterPluginAsync(nonExistentId, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.UnregisterPluginAsync(nonExistentId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("not found");
        TestContext.Current.WriteLine("Registry correctly handled non-existent plugin ID");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task UnregisterPluginAsync_WithInvalidPluginId_ReturnsFailure(string invalidId)
    {
        // Arrange
        var failureResult = FdwResult.Failure("Plugin ID cannot be null or empty");
        _mockRegistry.Setup(r => r.UnregisterPluginAsync(invalidId, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.UnregisterPluginAsync(invalidId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("cannot be null or empty");
        TestContext.Current.WriteLine($"Registry correctly rejected invalid plugin ID: '{invalidId}'");
    }

    [Fact]
    public void GetAllPlugins_ReturnsAllRegisteredPlugins()
    {
        // Arrange
        var allPlugins = new List<IToolPlugin> { _mockPlugin1.Object, _mockPlugin2.Object, _mockPlugin3.Object };
        _mockRegistry.Setup(r => r.GetAllPlugins()).Returns(allPlugins);

        // Act
        var result = _mockRegistry.Object.GetAllPlugins();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.ShouldContain(_mockPlugin1.Object);
        result.ShouldContain(_mockPlugin2.Object);
        result.ShouldContain(_mockPlugin3.Object);
        TestContext.Current.WriteLine($"GetAllPlugins returned {result.Count()} plugins");
    }

    [Fact]
    public void GetAllPlugins_WithNoRegisteredPlugins_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyPlugins = new List<IToolPlugin>();
        _mockRegistry.Setup(r => r.GetAllPlugins()).Returns(emptyPlugins);

        // Act
        var result = _mockRegistry.Object.GetAllPlugins();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
        TestContext.Current.WriteLine("GetAllPlugins returned empty collection when no plugins registered");
    }

    [Fact]
    public void GetEnabledPlugins_ReturnsOnlyEnabledPlugins()
    {
        // Arrange
        var enabledPlugins = new List<IToolPlugin> { _mockPlugin1.Object, _mockPlugin2.Object }; // Plugin3 is disabled
        _mockRegistry.Setup(r => r.GetEnabledPlugins()).Returns(enabledPlugins);

        // Act
        var result = _mockRegistry.Object.GetEnabledPlugins();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.ShouldContain(_mockPlugin1.Object);
        result.ShouldContain(_mockPlugin2.Object);
        result.ShouldNotContain(_mockPlugin3.Object); // Disabled plugin not included
        result.ShouldAllBe(p => p.IsEnabled);
        TestContext.Current.WriteLine($"GetEnabledPlugins returned {result.Count()} enabled plugins");
    }

    [Fact]
    public void GetPluginsByCategory_WithValidCategory_ReturnsMatchingPlugins()
    {
        // Arrange
        var categoryPlugins = new List<IToolPlugin> { _mockPlugin1.Object }; // Only Plugin1 is SessionManagement
        _mockRegistry.Setup(r => r.GetPluginsByCategory(SessionManagement.Instance)).Returns(categoryPlugins);

        // Act
        var result = _mockRegistry.Object.GetPluginsByCategory(SessionManagement.Instance);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.ShouldContain(_mockPlugin1.Object);
        result.ShouldAllBe(p => p.Category == SessionManagement.Instance);
        TestContext.Current.WriteLine($"GetPluginsByCategory returned {result.Count()} plugins for {SessionManagement.Instance.Name}");
    }

    [Fact]
    public void GetPluginsByCategory_WithCategoryWithNoPlugins_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyPlugins = new List<IToolPlugin>();
        _mockRegistry.Setup(r => r.GetPluginsByCategory(ServerManagement.Instance)).Returns(emptyPlugins);

        // Act
        var result = _mockRegistry.Object.GetPluginsByCategory(ServerManagement.Instance);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
        TestContext.Current.WriteLine($"GetPluginsByCategory returned empty collection for {ServerManagement.Instance.Name}");
    }

    [Fact]
    public void GetPluginById_WithValidId_ReturnsPlugin()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetPluginById(_mockPlugin1.Object.Id)).Returns(_mockPlugin1.Object);

        // Act
        var result = _mockRegistry.Object.GetPluginById(_mockPlugin1.Object.Id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(_mockPlugin1.Object);
        result.Id.ShouldBe(_mockPlugin1.Object.Id);
        TestContext.Current.WriteLine($"GetPluginById found plugin: {result.Name}");
    }

    [Fact]
    public void GetPluginById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = "NonExistentPlugin";
        _mockRegistry.Setup(r => r.GetPluginById(nonExistentId)).Returns((IToolPlugin)null);

        // Act
        var result = _mockRegistry.Object.GetPluginById(nonExistentId);

        // Assert
        result.ShouldBeNull();
        TestContext.Current.WriteLine($"GetPluginById correctly returned null for non-existent ID: {nonExistentId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void GetPluginById_WithInvalidId_ReturnsNull(string invalidId)
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetPluginById(invalidId)).Returns((IToolPlugin)null);

        // Act
        var result = _mockRegistry.Object.GetPluginById(invalidId);

        // Assert
        result.ShouldBeNull();
        TestContext.Current.WriteLine($"GetPluginById correctly returned null for invalid ID: '{invalidId}'");
    }

    [Fact]
    public void GetPluginsByPriority_ReturnsSortedPlugins()
    {
        // Arrange
        var sortedPlugins = new List<IToolPlugin> { _mockPlugin3.Object, _mockPlugin2.Object, _mockPlugin1.Object }; // Sorted by priority descending
        _mockRegistry.Setup(r => r.GetPluginsByPriority()).Returns(sortedPlugins);

        // Act
        var result = _mockRegistry.Object.GetPluginsByPriority().ToList();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].Priority.ShouldBeGreaterThanOrEqualTo(result[1].Priority);
        result[1].Priority.ShouldBeGreaterThanOrEqualTo(result[2].Priority);
        TestContext.Current.WriteLine($"GetPluginsByPriority returned plugins sorted: {string.Join(", ", result.Select(p => $"{p.Name}({p.Priority})"))}");
    }

    [Fact]
    public async Task IsPluginRegistered_WithRegisteredPlugin_ReturnsTrue()
    {
        // Arrange
        _mockRegistry.Setup(r => r.IsPluginRegistered(_mockPlugin1.Object.Id)).Returns(true);

        // Act
        var result = _mockRegistry.Object.IsPluginRegistered(_mockPlugin1.Object.Id);

        // Assert
        result.ShouldBeTrue();
        TestContext.Current.WriteLine($"IsPluginRegistered correctly returned true for registered plugin: {_mockPlugin1.Object.Id}");
    }

    [Fact]
    public async Task IsPluginRegistered_WithUnregisteredPlugin_ReturnsFalse()
    {
        // Arrange
        var unregisteredId = "UnregisteredPlugin";
        _mockRegistry.Setup(r => r.IsPluginRegistered(unregisteredId)).Returns(false);

        // Act
        var result = _mockRegistry.Object.IsPluginRegistered(unregisteredId);

        // Assert
        result.ShouldBeFalse();
        TestContext.Current.WriteLine($"IsPluginRegistered correctly returned false for unregistered plugin: {unregisteredId}");
    }

    [Fact]
    public async Task GetPluginCount_ReturnsCorrectCount()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetPluginCount()).Returns(3);

        // Act
        var result = _mockRegistry.Object.GetPluginCount();

        // Assert
        result.ShouldBe(3);
        TestContext.Current.WriteLine($"GetPluginCount returned: {result}");
    }

    [Fact]
    public async Task GetPluginCount_WithNoPlugins_ReturnsZero()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetPluginCount()).Returns(0);

        // Act
        var result = _mockRegistry.Object.GetPluginCount();

        // Assert
        result.ShouldBe(0);
        TestContext.Current.WriteLine("GetPluginCount correctly returned 0 when no plugins registered");
    }

    [Fact]
    public async Task ClearPluginsAsync_RemovesAllPlugins_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockRegistry.Setup(r => r.ClearPluginsAsync(_cancellationToken)).ReturnsAsync(successResult);

        // Act
        var result = await _mockRegistry.Object.ClearPluginsAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine("ClearPluginsAsync successfully removed all plugins");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var cancelledTokenSource = new CancellationTokenSource();
        cancelledTokenSource.Cancel();

        _mockRegistry.Setup(r => r.RegisterPluginAsync(It.IsAny<IToolPlugin>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new OperationCancelledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockRegistry.Object.RegisterPluginAsync(_mockPlugin1.Object, cancelledTokenSource.Token));

        TestContext.Current.WriteLine("RegisterPluginAsync correctly threw OperationCancelledException for cancelled token");
    }

    [Fact]
    public void GetPluginsByCategory_WithNullCategory_ThrowsArgumentNullException()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetPluginsByCategory(null))
                    .Throws(new ArgumentNullException(nameof(ToolCategoryBase)));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _mockRegistry.Object.GetPluginsByCategory(null));
        TestContext.Current.WriteLine("GetPluginsByCategory correctly threw ArgumentNullException for null category");
    }

    /// <summary>
    /// Mock interface for IPluginRegistry for testing purposes.
    /// </summary>
    private interface IPluginRegistry
    {
        Task<IFdwResult> RegisterPluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default);
        Task<IFdwResult> UnregisterPluginAsync(string pluginId, CancellationToken cancellationToken = default);
        IEnumerable<IToolPlugin> GetAllPlugins();
        IEnumerable<IToolPlugin> GetEnabledPlugins();
        IEnumerable<IToolPlugin> GetPluginsByCategory(ToolCategoryBase category);
        IEnumerable<IToolPlugin> GetPluginsByPriority();
        IToolPlugin GetPluginById(string pluginId);
        bool IsPluginRegistered(string pluginId);
        int GetPluginCount();
        Task<IFdwResult> ClearPluginsAsync(CancellationToken cancellationToken = default);
    }
}

/// <summary>
/// Edge case tests for plugin registry focusing on boundary conditions and error scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginRegistryEdgeCaseTests
{
    private readonly Mock<IPluginRegistry> _mockRegistry;
    private readonly CancellationToken _cancellationToken;

    public PluginRegistryEdgeCaseTests()
    {
        _mockRegistry = new Mock<IPluginRegistry>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task RegisterPluginAsync_WithPluginInitializationFailure_ReturnsFailure()
    {
        // Arrange
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns("FailingPlugin");
        mockPlugin.Setup(p => p.InitializeAsync(It.IsAny<IToolPluginConfiguration>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(FdwResult.Failure("Plugin initialization failed"));

        var failureResult = FdwResult.Failure("Plugin registration failed due to initialization error");
        _mockRegistry.Setup(r => r.RegisterPluginAsync(mockPlugin.Object, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(mockPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("initialization error");
        TestContext.Current.WriteLine("Registry correctly handled plugin initialization failure");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithExtremelyLongPluginId_HandlesGracefully()
    {
        // Arrange
        var longId = new string('x', 10000); // 10k character ID
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(longId);

        var failureResult = FdwResult.Failure("Plugin ID exceeds maximum length");
        _mockRegistry.Setup(r => r.RegisterPluginAsync(mockPlugin.Object, _cancellationToken))
                    .ReturnsAsync(failureResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(mockPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("exceeds maximum length");
        TestContext.Current.WriteLine($"Registry correctly handled extremely long plugin ID (length: {longId.Length})");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithSpecialCharactersInId_HandlesCorrectly()
    {
        // Arrange
        var specialId = "plugin@#$%^&*()[]{}|\\:;\"'<>,.?/~`!+=";
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(specialId);

        var successResult = FdwResult.Success(); // Assuming special characters are allowed
        _mockRegistry.Setup(r => r.RegisterPluginAsync(mockPlugin.Object, _cancellationToken))
                    .ReturnsAsync(successResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(mockPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"Registry handled special characters in plugin ID: {specialId}");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithUnicodePluginId_HandlesCorrectly()
    {
        // Arrange
        var unicodeId = "Plugin_🚀_测试_العربية_Ω";
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(unicodeId);

        var successResult = FdwResult.Success();
        _mockRegistry.Setup(r => r.RegisterPluginAsync(mockPlugin.Object, _cancellationToken))
                    .ReturnsAsync(successResult);

        // Act
        var result = await _mockRegistry.Object.RegisterPluginAsync(mockPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"Registry handled Unicode plugin ID: {unicodeId}");
    }

    [Fact]
    public void GetAllPlugins_WithLargeNumberOfPlugins_ReturnsAllEfficiently()
    {
        // Arrange
        var largePluginList = Enumerable.Range(1, 10000)
            .Select(i =>
            {
                var mockPlugin = new Mock<IToolPlugin>();
                mockPlugin.Setup(p => p.Id).Returns($"Plugin_{i}");
                mockPlugin.Setup(p => p.Name).Returns($"Plugin {i}");
                mockPlugin.Setup(p => p.Priority).Returns(i);
                mockPlugin.Setup(p => p.IsEnabled).Returns(i % 2 == 0); // Half enabled
                return mockPlugin.Object;
            })
            .ToList();

        _mockRegistry.Setup(r => r.GetAllPlugins()).Returns(largePluginList);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _mockRegistry.Object.GetAllPlugins().ToList();
        stopwatch.Stop();

        // Assert
        result.Count.ShouldBe(10000);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000); // Should complete within 1 second
        TestContext.Current.WriteLine($"GetAllPlugins returned {result.Count} plugins in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GetPluginsByPriority_WithDuplicatePriorities_HandlesStably()
    {
        // Arrange
        var duplicatePriorityPlugins = new List<IToolPlugin>();
        for (int i = 0; i < 5; i++)
        {
            var mockPlugin = new Mock<IToolPlugin>();
            mockPlugin.Setup(p => p.Id).Returns($"Plugin_{i}");
            mockPlugin.Setup(p => p.Priority).Returns(10); // All have same priority
            duplicatePriorityPlugins.Add(mockPlugin.Object);
        }

        _mockRegistry.Setup(r => r.GetPluginsByPriority()).Returns(duplicatePriorityPlugins);

        // Act
        var result = _mockRegistry.Object.GetPluginsByPriority().ToList();

        // Assert
        result.Count.ShouldBe(5);
        result.ShouldAllBe(p => p.Priority == 10);
        TestContext.Current.WriteLine($"GetPluginsByPriority handled {result.Count} plugins with duplicate priorities");
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetPluginsByPriority_WithExtremePriorityValues_HandlesCorrectly(int extremePriority)
    {
        // Arrange
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns($"ExtremePlugin_{extremePriority}");
        mockPlugin.Setup(p => p.Priority).Returns(extremePriority);

        var extremePlugins = new List<IToolPlugin> { mockPlugin.Object };
        _mockRegistry.Setup(r => r.GetPluginsByPriority()).Returns(extremePlugins);

        // Act
        var result = _mockRegistry.Object.GetPluginsByPriority().ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Priority.ShouldBe(extremePriority);
        TestContext.Current.WriteLine($"GetPluginsByPriority handled extreme priority value: {extremePriority}");
    }

    [Fact]
    public async Task UnregisterPluginAsync_WithPluginShutdownFailure_HandlesGracefully()
    {
        // Arrange
        var pluginId = "FailingShutdownPlugin";
        var partialFailureResult = FdwResult.Failure("Plugin unregistered but shutdown failed");
        _mockRegistry.Setup(r => r.UnregisterPluginAsync(pluginId, _cancellationToken))
                    .ReturnsAsync(partialFailureResult);

        // Act
        var result = await _mockRegistry.Object.UnregisterPluginAsync(pluginId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("shutdown failed");
        TestContext.Current.WriteLine("Registry handled plugin shutdown failure during unregistration");
    }

    [Fact]
    public void GetPluginsByCategory_WithCategoryHavingManyPlugins_PerformsEfficiently()
    {
        // Arrange
        var categoryPlugins = Enumerable.Range(1, 1000)
            .Select(i =>
            {
                var mockPlugin = new Mock<IToolPlugin>();
                mockPlugin.Setup(p => p.Id).Returns($"CategoryPlugin_{i}");
                mockPlugin.Setup(p => p.Category).Returns(CodeAnalysis.Instance);
                return mockPlugin.Object;
            })
            .ToList();

        _mockRegistry.Setup(r => r.GetPluginsByCategory(CodeAnalysis.Instance)).Returns(categoryPlugins);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _mockRegistry.Object.GetPluginsByCategory(CodeAnalysis.Instance).ToList();
        stopwatch.Stop();

        // Assert
        result.Count.ShouldBe(1000);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(100); // Should be very fast
        result.ShouldAllBe(p => p.Category == CodeAnalysis.Instance);
        TestContext.Current.WriteLine($"GetPluginsByCategory returned {result.Count} plugins in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ClearPluginsAsync_WithActivePlugins_ShutdownsAllPlugins()
    {
        // Arrange
        var shutdownResult = FdwResult.Success();
        _mockRegistry.Setup(r => r.ClearPluginsAsync(_cancellationToken))
                    .ReturnsAsync(shutdownResult);

        // Act
        var result = await _mockRegistry.Object.ClearPluginsAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine("ClearPluginsAsync successfully shutdown all active plugins");
    }

    [Fact]
    public async Task RegisterPluginAsync_WithTimeout_HandlesGracefully()
    {
        // Arrange
        var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns("TimeoutPlugin");

        _mockRegistry.Setup(r => r.RegisterPluginAsync(It.IsAny<IToolPlugin>(), It.IsAny<CancellationToken>()))
                    .Returns(async (IToolPlugin plugin, CancellationToken ct) =>
                    {
                        await Task.Delay(1000, ct); // Will timeout
                        return FdwResult.Success();
                    });

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockRegistry.Object.RegisterPluginAsync(mockPlugin.Object, timeoutTokenSource.Token));

        TestContext.Current.WriteLine("RegisterPluginAsync correctly handled timeout scenario");
    }

    /// <summary>
    /// Mock interface for IPluginRegistry for testing purposes.
    /// </summary>
    private interface IPluginRegistry
    {
        Task<IFdwResult> RegisterPluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default);
        Task<IFdwResult> UnregisterPluginAsync(string pluginId, CancellationToken cancellationToken = default);
        IEnumerable<IToolPlugin> GetAllPlugins();
        IEnumerable<IToolPlugin> GetEnabledPlugins();
        IEnumerable<IToolPlugin> GetPluginsByCategory(ToolCategoryBase category);
        IEnumerable<IToolPlugin> GetPluginsByPriority();
        IToolPlugin GetPluginById(string pluginId);
        bool IsPluginRegistered(string pluginId);
        int GetPluginCount();
        Task<IFdwResult> ClearPluginsAsync(CancellationToken cancellationToken = default);
    }
}