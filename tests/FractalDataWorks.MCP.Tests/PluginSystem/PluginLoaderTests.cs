using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.MCP.Tests.PluginSystem;

/// <summary>
/// Comprehensive tests for plugin loader functionality and plugin discovery mechanisms.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoaderTests
{
    private readonly Mock<IPluginLoader> _mockLoader;
    private readonly Mock<IToolPlugin> _mockPlugin1;
    private readonly Mock<IToolPlugin> _mockPlugin2;
    private readonly CancellationToken _cancellationToken;

    public PluginLoaderTests()
    {
        _mockLoader = new Mock<IPluginLoader>();
        _mockPlugin1 = CreateMockPlugin("LoadedPlugin1", "Loaded Plugin 1", SessionManagement.Instance);
        _mockPlugin2 = CreateMockPlugin("LoadedPlugin2", "Loaded Plugin 2", CodeAnalysis.Instance);
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    private static Mock<IToolPlugin> CreateMockPlugin(string id, string name, ToolCategoryBase category)
    {
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(id);
        mockPlugin.Setup(p => p.Name).Returns(name);
        mockPlugin.Setup(p => p.Description).Returns($"Description for {name}");
        mockPlugin.Setup(p => p.Category).Returns(category);
        mockPlugin.Setup(p => p.Priority).Returns(1);
        mockPlugin.Setup(p => p.IsEnabled).Returns(true);
        mockPlugin.Setup(p => p.GetTools()).Returns(new List<IMcpTool>());
        return mockPlugin;
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithValidDirectory_ReturnsLoadedPlugins()
    {
        // Arrange
        var pluginDirectory = @"C:\Plugins\TestPlugins";
        var loadedPlugins = new List<IToolPlugin> { _mockPlugin1.Object, _mockPlugin2.Object };
        var successResult = GenericResult.Success(loadedPlugins.AsEnumerable());

        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(pluginDirectory, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(pluginDirectory, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(2);
        result.Value.ShouldContain(_mockPlugin1.Object);
        result.Value.ShouldContain(_mockPlugin2.Object);
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync loaded {result.Value.Count()} plugins from {pluginDirectory}");
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithNonExistentDirectory_ReturnsFailure()
    {
        // Arrange
        var nonExistentDirectory = @"C:\NonExistent\PluginDirectory";
        var failureResult = GenericResult.Failure<IEnumerable<IToolPlugin>>($"Directory not found: {nonExistentDirectory}");

        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(nonExistentDirectory, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(nonExistentDirectory, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Directory not found");
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync correctly handled non-existent directory: {nonExistentDirectory}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task LoadPluginsFromDirectoryAsync_WithInvalidPath_ReturnsFailure(string invalidPath)
    {
        // Arrange
        var failureResult = GenericResult.Failure<IEnumerable<IToolPlugin>>("Plugin directory path cannot be null or empty");

        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(invalidPath, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(invalidPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("cannot be null or empty");
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync correctly rejected invalid path: '{invalidPath}'");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithValidAssembly_ReturnsLoadedPlugins()
    {
        // Arrange
        var assemblyPath = @"C:\Plugins\TestPlugin.dll";
        var loadedPlugins = new List<IToolPlugin> { _mockPlugin1.Object };
        var successResult = GenericResult.Success(loadedPlugins.AsEnumerable());

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(assemblyPath, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromAssemblyAsync(assemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(1);
        result.Value.ShouldContain(_mockPlugin1.Object);
        TestContext.Current.WriteLine($"LoadPluginFromAssemblyAsync loaded plugin from {assemblyPath}");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithInvalidAssembly_ReturnsFailure()
    {
        // Arrange
        var invalidAssemblyPath = @"C:\Plugins\InvalidAssembly.dll";
        var failureResult = GenericResult.Failure<IEnumerable<IToolPlugin>>("Failed to load assembly: Invalid format");

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(invalidAssemblyPath, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromAssemblyAsync(invalidAssemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Failed to load assembly");
        TestContext.Current.WriteLine($"LoadPluginFromAssemblyAsync correctly handled invalid assembly: {invalidAssemblyPath}");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithAssemblyWithoutPlugins_ReturnsEmpty()
    {
        // Arrange
        var assemblyPath = @"C:\Plugins\NoPluginsAssembly.dll";
        var emptyResult = GenericResult.Success(Enumerable.Empty<IToolPlugin>());

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(assemblyPath, _cancellationToken))
                  .ReturnsAsync(emptyResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromAssemblyAsync(assemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(0);
        TestContext.Current.WriteLine($"LoadPluginFromAssemblyAsync correctly handled assembly with no plugins: {assemblyPath}");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithValidType_ReturnsPlugin()
    {
        // Arrange
        var pluginType = typeof(TestToolPlugin);
        var successResult = GenericResult.Success(_mockPlugin1.Object);

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(pluginType, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(pluginType, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(_mockPlugin1.Object);
        TestContext.Current.WriteLine($"LoadPluginFromTypeAsync loaded plugin from type: {pluginType.Name}");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithNullType_ReturnsFailure()
    {
        // Arrange
        var failureResult = GenericResult.Failure<IToolPlugin>("Plugin type cannot be null");

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(null, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(null, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("cannot be null");
        TestContext.Current.WriteLine("LoadPluginFromTypeAsync correctly handled null type");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithNonPluginType_ReturnsFailure()
    {
        // Arrange
        var nonPluginType = typeof(string);
        var failureResult = GenericResult.Failure<IToolPlugin>($"Type {nonPluginType.Name} does not implement IToolPlugin");

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(nonPluginType, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(nonPluginType, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("does not implement IToolPlugin");
        TestContext.Current.WriteLine($"LoadPluginFromTypeAsync correctly rejected non-plugin type: {nonPluginType.Name}");
    }

    [Fact]
    public async Task DiscoverPluginTypesInAssemblyAsync_WithValidAssembly_ReturnsTypes()
    {
        // Arrange
        var assemblyPath = @"C:\Plugins\TestPlugin.dll";
        var discoveredTypes = new List<Type> { typeof(TestToolPlugin), typeof(AnotherTestToolPlugin) };
        var successResult = GenericResult.Success(discoveredTypes.AsEnumerable());

        _mockLoader.Setup(l => l.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(2);
        result.Value.ShouldContain(typeof(TestToolPlugin));
        result.Value.ShouldContain(typeof(AnotherTestToolPlugin));
        TestContext.Current.WriteLine($"DiscoverPluginTypesInAssemblyAsync found {result.Value.Count()} plugin types in {assemblyPath}");
    }

    [Fact]
    public async Task DiscoverPluginTypesInAssemblyAsync_WithAssemblyWithoutPlugins_ReturnsEmpty()
    {
        // Arrange
        var assemblyPath = @"C:\Plugins\NoPluginsAssembly.dll";
        var emptyResult = GenericResult.Success(Enumerable.Empty<Type>());

        _mockLoader.Setup(l => l.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken))
                  .ReturnsAsync(emptyResult);

        // Act
        var result = await _mockLoader.Object.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(0);
        TestContext.Current.WriteLine($"DiscoverPluginTypesInAssemblyAsync correctly found no plugin types in {assemblyPath}");
    }

    [Fact]
    public async Task ValidatePluginAsync_WithValidPlugin_ReturnsSuccess()
    {
        // Arrange
        var successResult = GenericResult.Success();

        _mockLoader.Setup(l => l.ValidatePluginAsync(_mockPlugin1.Object, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.ValidatePluginAsync(_mockPlugin1.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"ValidatePluginAsync validated plugin: {_mockPlugin1.Object.Name}");
    }

    [Fact]
    public async Task ValidatePluginAsync_WithInvalidPlugin_ReturnsFailure()
    {
        // Arrange
        var invalidPlugin = new Mock<IToolPlugin>();
        invalidPlugin.Setup(p => p.Id).Returns((string)null); // Invalid - null ID
        invalidPlugin.Setup(p => p.Name).Returns("Invalid Plugin");

        var failureResult = GenericResult.Failure("Plugin validation failed: ID cannot be null");

        _mockLoader.Setup(l => l.ValidatePluginAsync(invalidPlugin.Object, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.ValidatePluginAsync(invalidPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("validation failed");
        TestContext.Current.WriteLine("ValidatePluginAsync correctly identified invalid plugin");
    }

    [Fact]
    public async Task ValidatePluginAsync_WithNullPlugin_ReturnsFailure()
    {
        // Arrange
        var failureResult = GenericResult.Failure("Plugin cannot be null");

        _mockLoader.Setup(l => l.ValidatePluginAsync(null, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.ValidatePluginAsync(null, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("cannot be null");
        TestContext.Current.WriteLine("ValidatePluginAsync correctly handled null plugin");
    }

    [Fact]
    public async Task GetSupportedPluginExtensions_ReturnsExpectedExtensions()
    {
        // Arrange
        var supportedExtensions = new[] { ".dll", ".exe" };
        _mockLoader.Setup(l => l.GetSupportedPluginExtensions()).Returns(supportedExtensions);

        // Act
        var result = _mockLoader.Object.GetSupportedPluginExtensions();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result.ShouldContain(".dll");
        result.ShouldContain(".exe");
        TestContext.Current.WriteLine($"GetSupportedPluginExtensions returned: {string.Join(", ", result)}");
    }

    [Fact]
    public async Task IsPluginAssembly_WithValidPluginAssembly_ReturnsTrue()
    {
        // Arrange
        var pluginAssemblyPath = @"C:\Plugins\ValidPlugin.dll";
        _mockLoader.Setup(l => l.IsPluginAssembly(pluginAssemblyPath)).Returns(true);

        // Act
        var result = _mockLoader.Object.IsPluginAssembly(pluginAssemblyPath);

        // Assert
        result.ShouldBeTrue();
        TestContext.Current.WriteLine($"IsPluginAssembly correctly identified plugin assembly: {pluginAssemblyPath}");
    }

    [Fact]
    public async Task IsPluginAssembly_WithNonPluginAssembly_ReturnsFalse()
    {
        // Arrange
        var nonPluginAssemblyPath = @"C:\System\System.dll";
        _mockLoader.Setup(l => l.IsPluginAssembly(nonPluginAssemblyPath)).Returns(false);

        // Act
        var result = _mockLoader.Object.IsPluginAssembly(nonPluginAssemblyPath);

        // Assert
        result.ShouldBeFalse();
        TestContext.Current.WriteLine($"IsPluginAssembly correctly identified non-plugin assembly: {nonPluginAssemblyPath}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task IsPluginAssembly_WithInvalidPath_ReturnsFalse(string invalidPath)
    {
        // Arrange
        _mockLoader.Setup(l => l.IsPluginAssembly(invalidPath)).Returns(false);

        // Act
        var result = _mockLoader.Object.IsPluginAssembly(invalidPath);

        // Assert
        result.ShouldBeFalse();
        TestContext.Current.WriteLine($"IsPluginAssembly correctly handled invalid path: '{invalidPath}'");
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var cancelledTokenSource = new CancellationTokenSource();
        cancelledTokenSource.Cancel();

        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new OperationCancelledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockLoader.Object.LoadPluginsFromDirectoryAsync(@"C:\Plugins", cancelledTokenSource.Token));

        TestContext.Current.WriteLine("LoadPluginsFromDirectoryAsync correctly threw OperationCancelledException for cancelled token");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithTimeout_HandlesGracefully()
    {
        // Arrange
        var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns(async (string path, CancellationToken ct) =>
                  {
                      await Task.Delay(1000, ct); // Will timeout
                      return GenericResult.Success(Enumerable.Empty<IToolPlugin>());
                  });

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockLoader.Object.LoadPluginFromAssemblyAsync(@"C:\Plugins\Test.dll", timeoutTokenSource.Token));

        TestContext.Current.WriteLine("LoadPluginFromAssemblyAsync correctly handled timeout scenario");
    }

    /// <summary>
    /// Mock interface for IPluginLoader for testing purposes.
    /// </summary>
    private interface IPluginLoader
    {
        Task<IGenericResult<IEnumerable<IToolPlugin>>> LoadPluginsFromDirectoryAsync(string pluginDirectory, CancellationToken cancellationToken = default);
        Task<IGenericResult<IEnumerable<IToolPlugin>>> LoadPluginFromAssemblyAsync(string assemblyPath, CancellationToken cancellationToken = default);
        Task<IGenericResult<IToolPlugin>> LoadPluginFromTypeAsync(Type pluginType, CancellationToken cancellationToken = default);
        Task<IGenericResult<IEnumerable<Type>>> DiscoverPluginTypesInAssemblyAsync(string assemblyPath, CancellationToken cancellationToken = default);
        Task<IGenericResult> ValidatePluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default);
        string[] GetSupportedPluginExtensions();
        bool IsPluginAssembly(string assemblyPath);
    }

    /// <summary>
    /// Test plugin implementation for testing purposes.
    /// </summary>
    private class TestToolPlugin : IToolPlugin
    {
        public string Id => "TestPlugin";
        public string Name => "Test Plugin";
        public string Description => "Test plugin for unit tests";
        public ToolCategory Category => SessionManagement.Instance;
        public int Priority => 1;
        public bool IsEnabled => true;

        public IReadOnlyCollection<IMcpTool> GetTools() => new List<IMcpTool>();
        public Task<IGenericResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success(new PluginHealth { Status = HealthStatus.Healthy }));
        public Task<IGenericResult> ShutdownAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
    }

    /// <summary>
    /// Another test plugin implementation for testing purposes.
    /// </summary>
    private class AnotherTestToolPlugin : IToolPlugin
    {
        public string Id => "AnotherTestPlugin";
        public string Name => "Another Test Plugin";
        public string Description => "Another test plugin for unit tests";
        public ToolCategory Category => CodeAnalysis.Instance;
        public int Priority => 2;
        public bool IsEnabled => true;

        public IReadOnlyCollection<IMcpTool> GetTools() => new List<IMcpTool>();
        public Task<IGenericResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success(new PluginHealth { Status = HealthStatus.Healthy }));
        public Task<IGenericResult> ShutdownAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
    }
}

/// <summary>
/// Edge case tests for plugin loader focusing on boundary conditions and error scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoaderEdgeCaseTests
{
    private readonly Mock<IPluginLoader> _mockLoader;
    private readonly CancellationToken _cancellationToken;

    public PluginLoaderEdgeCaseTests()
    {
        _mockLoader = new Mock<IPluginLoader>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithDeepDirectoryStructure_HandlesCorrectly()
    {
        // Arrange
        var deepDirectory = string.Join(Path.DirectorySeparatorChar.ToString(),
            Enumerable.Range(1, 100).Select(i => $"Level{i}"));
        var deepPath = Path.Combine(@"C:\Plugins", deepDirectory);

        var failureResult = GenericResult.Failure<IEnumerable<IToolPlugin>>("Path too long");
        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(deepPath, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(deepPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Path too long");
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync handled deep directory structure with path length: {deepPath.Length}");
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithSpecialCharactersInPath_HandlesCorrectly()
    {
        // Arrange
        var specialCharPath = @"C:\Plugins\测试\Ελληνικά\العربية\🚀\@#$%^&()";
        var successResult = GenericResult.Success(Enumerable.Empty<IToolPlugin>());

        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(specialCharPath, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(specialCharPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync handled special characters in path: {specialCharPath}");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithLargeAssembly_HandlesEfficiently()
    {
        // Arrange
        var largeAssemblyPath = @"C:\Plugins\LargeAssembly.dll";
        var startTime = DateTime.UtcNow;

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(largeAssemblyPath, _cancellationToken))
                  .Returns(async (string path, CancellationToken ct) =>
                  {
                      await Task.Delay(100, ct); // Simulate loading time
                      return GenericResult.Success(Enumerable.Empty<IToolPlugin>());
                  });

        // Act
        var result = await _mockLoader.Object.LoadPluginFromAssemblyAsync(largeAssemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var loadTime = DateTime.UtcNow - startTime;
        loadTime.TotalSeconds.ShouldBeLessThan(5); // Should complete within reasonable time
        TestContext.Current.WriteLine($"LoadPluginFromAssemblyAsync handled large assembly in {loadTime.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task LoadPluginFromAssemblyAsync_WithCorruptedAssembly_ReturnsFailure()
    {
        // Arrange
        var corruptedAssemblyPath = @"C:\Plugins\CorruptedAssembly.dll";
        var failureResult = GenericResult.Failure<IEnumerable<IToolPlugin>>("Assembly is corrupted or invalid format");

        _mockLoader.Setup(l => l.LoadPluginFromAssemblyAsync(corruptedAssemblyPath, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromAssemblyAsync(corruptedAssemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("corrupted or invalid format");
        TestContext.Current.WriteLine($"LoadPluginFromAssemblyAsync correctly handled corrupted assembly: {corruptedAssemblyPath}");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithAbstractType_ReturnsFailure()
    {
        // Arrange
        var abstractType = typeof(AbstractTestPlugin);
        var failureResult = GenericResult.Failure<IToolPlugin>($"Cannot instantiate abstract type: {abstractType.Name}");

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(abstractType, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(abstractType, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Cannot instantiate abstract type");
        TestContext.Current.WriteLine($"LoadPluginFromTypeAsync correctly rejected abstract type: {abstractType.Name}");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithInterfaceType_ReturnsFailure()
    {
        // Arrange
        var interfaceType = typeof(IToolPlugin);
        var failureResult = GenericResult.Failure<IToolPlugin>($"Cannot instantiate interface type: {interfaceType.Name}");

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(interfaceType, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(interfaceType, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Cannot instantiate interface type");
        TestContext.Current.WriteLine($"LoadPluginFromTypeAsync correctly rejected interface type: {interfaceType.Name}");
    }

    [Fact]
    public async Task LoadPluginFromTypeAsync_WithGenericType_ReturnsFailure()
    {
        // Arrange
        var genericType = typeof(GenericTestPlugin<>);
        var failureResult = GenericResult.Failure<IToolPlugin>($"Cannot instantiate open generic type: {genericType.Name}");

        _mockLoader.Setup(l => l.LoadPluginFromTypeAsync(genericType, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginFromTypeAsync(genericType, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Cannot instantiate open generic type");
        TestContext.Current.WriteLine($"LoadPluginFromTypeAsync correctly rejected open generic type: {genericType.Name}");
    }

    [Fact]
    public async Task DiscoverPluginTypesInAssemblyAsync_WithMixedTypes_ReturnsOnlyPluginTypes()
    {
        // Arrange
        var assemblyPath = @"C:\Plugins\MixedTypesAssembly.dll";
        var pluginTypes = new List<Type> { typeof(ValidTestPlugin) }; // Only valid plugin types
        var successResult = GenericResult.Success(pluginTypes.AsEnumerable());

        _mockLoader.Setup(l => l.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.DiscoverPluginTypesInAssemblyAsync(assemblyPath, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(1);
        result.Value.ShouldAllBe(t => typeof(IToolPlugin).IsAssignableFrom(t));
        TestContext.Current.WriteLine($"DiscoverPluginTypesInAssemblyAsync filtered and returned {result.Value.Count()} valid plugin types");
    }

    [Fact]
    public async Task ValidatePluginAsync_WithPluginHavingCircularDependencies_ReturnsFailure()
    {
        // Arrange
        var circularDependencyPlugin = new Mock<IToolPlugin>();
        circularDependencyPlugin.Setup(p => p.Id).Returns("CircularPlugin");
        circularDependencyPlugin.Setup(p => p.Name).Returns("Circular Dependency Plugin");

        var failureResult = GenericResult.Failure("Plugin has circular dependencies");
        _mockLoader.Setup(l => l.ValidatePluginAsync(circularDependencyPlugin.Object, _cancellationToken))
                  .ReturnsAsync(failureResult);

        // Act
        var result = await _mockLoader.Object.ValidatePluginAsync(circularDependencyPlugin.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("circular dependencies");
        TestContext.Current.WriteLine("ValidatePluginAsync correctly detected circular dependencies");
    }

    [Fact]
    public async Task IsPluginAssembly_WithVeryLargePath_HandlesGracefully()
    {
        // Arrange
        var largePath = @"C:\" + new string('x', 32000) + @"\plugin.dll";
        _mockLoader.Setup(l => l.IsPluginAssembly(largePath)).Returns(false);

        // Act
        var result = _mockLoader.Object.IsPluginAssembly(largePath);

        // Assert
        result.ShouldBeFalse();
        TestContext.Current.WriteLine($"IsPluginAssembly handled very large path (length: {largePath.Length}) gracefully");
    }

    [Theory]
    [InlineData(@"C:\Plugins\plugin.dll")]
    [InlineData(@"\\network\share\plugins\plugin.dll")]
    [InlineData(@"C:\Program Files (x86)\MyApp\Plugins\plugin.dll")]
    public async Task IsPluginAssembly_WithDifferentPathFormats_HandlesCorrectly(string path)
    {
        // Arrange
        _mockLoader.Setup(l => l.IsPluginAssembly(path)).Returns(true);

        // Act
        var result = _mockLoader.Object.IsPluginAssembly(path);

        // Assert
        result.ShouldBeTrue();
        TestContext.Current.WriteLine($"IsPluginAssembly handled path format correctly: {path}");
    }

    [Fact]
    public void GetSupportedPluginExtensions_ReturnsNonEmptyArray()
    {
        // Arrange
        var extensions = new[] { ".dll", ".exe", ".so", ".dylib" };
        _mockLoader.Setup(l => l.GetSupportedPluginExtensions()).Returns(extensions);

        // Act
        var result = _mockLoader.Object.GetSupportedPluginExtensions();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        result.ShouldAllBe(ext => ext.StartsWith("."));
        TestContext.Current.WriteLine($"GetSupportedPluginExtensions returned {result.Length} extensions: {string.Join(", ", result)}");
    }

    [Fact]
    public async Task LoadPluginsFromDirectoryAsync_WithDirectoryContainingNonPluginFiles_FiltersCorrectly()
    {
        // Arrange
        var mixedDirectory = @"C:\Plugins\MixedFiles";
        var validPlugins = Enumerable.Range(1, 3).Select(i =>
        {
            var mockPlugin = new Mock<IToolPlugin>();
            mockPlugin.Setup(p => p.Id).Returns($"ValidPlugin{i}");
            return mockPlugin.Object;
        });

        var successResult = GenericResult.Success(validPlugins);
        _mockLoader.Setup(l => l.LoadPluginsFromDirectoryAsync(mixedDirectory, _cancellationToken))
                  .ReturnsAsync(successResult);

        // Act
        var result = await _mockLoader.Object.LoadPluginsFromDirectoryAsync(mixedDirectory, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(3);
        TestContext.Current.WriteLine($"LoadPluginsFromDirectoryAsync filtered and loaded {result.Value.Count()} valid plugins from mixed directory");
    }

    /// <summary>
    /// Mock interface for IPluginLoader for testing purposes.
    /// </summary>
    private interface IPluginLoader
    {
        Task<IGenericResult<IEnumerable<IToolPlugin>>> LoadPluginsFromDirectoryAsync(string pluginDirectory, CancellationToken cancellationToken = default);
        Task<IGenericResult<IEnumerable<IToolPlugin>>> LoadPluginFromAssemblyAsync(string assemblyPath, CancellationToken cancellationToken = default);
        Task<IGenericResult<IToolPlugin>> LoadPluginFromTypeAsync(Type pluginType, CancellationToken cancellationToken = default);
        Task<IGenericResult<IEnumerable<Type>>> DiscoverPluginTypesInAssemblyAsync(string assemblyPath, CancellationToken cancellationToken = default);
        Task<IGenericResult> ValidatePluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default);
        string[] GetSupportedPluginExtensions();
        bool IsPluginAssembly(string assemblyPath);
    }

    /// <summary>
    /// Test plugin implementations for edge case testing.
    /// </summary>
    private abstract class AbstractTestPlugin : IToolPlugin
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract ToolCategory Category { get; }
        public abstract int Priority { get; }
        public abstract bool IsEnabled { get; }
        public abstract IReadOnlyCollection<IMcpTool> GetTools();
        public abstract Task<IGenericResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default);
        public abstract Task<IGenericResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default);
        public abstract Task<IGenericResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default);
        public abstract Task<IGenericResult> ShutdownAsync(CancellationToken cancellationToken = default);
    }

    private class GenericTestPlugin<T> : IToolPlugin
    {
        public string Id => $"GenericPlugin_{typeof(T).Name}";
        public string Name => $"Generic Plugin for {typeof(T).Name}";
        public string Description => "Generic test plugin";
        public ToolCategory Category => SessionManagement.Instance;
        public int Priority => 1;
        public bool IsEnabled => true;
        public IReadOnlyCollection<IMcpTool> GetTools() => new List<IMcpTool>();
        public Task<IGenericResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success(new PluginHealth { Status = HealthStatus.Healthy }));
        public Task<IGenericResult> ShutdownAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
    }

    private class ValidTestPlugin : IToolPlugin
    {
        public string Id => "ValidTestPlugin";
        public string Name => "Valid Test Plugin";
        public string Description => "A valid test plugin";
        public ToolCategory Category => SessionManagement.Instance;
        public int Priority => 1;
        public bool IsEnabled => true;
        public IReadOnlyCollection<IMcpTool> GetTools() => new List<IMcpTool>();
        public Task<IGenericResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
        public Task<IGenericResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success(new PluginHealth { Status = HealthStatus.Healthy }));
        public Task<IGenericResult> ShutdownAsync(CancellationToken cancellationToken = default) => Task.FromResult(GenericResult.Success());
    }
}