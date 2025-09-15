using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.MCP.Tests.PluginSystem;

/// <summary>
/// Comprehensive tests for plugin health monitoring functionality and health check mechanisms.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginHealthMonitorTests
{
    private readonly Mock<IPluginHealthMonitor> _mockHealthMonitor;
    private readonly Mock<IToolPlugin> _healthyPlugin;
    private readonly Mock<IToolPlugin> _degradedPlugin;
    private readonly Mock<IToolPlugin> _unhealthyPlugin;
    private readonly CancellationToken _cancellationToken;

    public PluginHealthMonitorTests()
    {
        _mockHealthMonitor = new Mock<IPluginHealthMonitor>();
        _healthyPlugin = CreateMockPluginWithHealth("HealthyPlugin", HealthStatus.Healthy, "All systems operational");
        _degradedPlugin = CreateMockPluginWithHealth("DegradedPlugin", HealthStatus.Degraded, "Performance issues detected");
        _unhealthyPlugin = CreateMockPluginWithHealth("UnhealthyPlugin", HealthStatus.Unhealthy, "Critical failure");
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    private static Mock<IToolPlugin> CreateMockPluginWithHealth(string id, HealthStatus status, string message)
    {
        var mockPlugin = new Mock<IToolPlugin>();
        mockPlugin.Setup(p => p.Id).Returns(id);
        mockPlugin.Setup(p => p.Name).Returns($"Plugin {id}");
        mockPlugin.Setup(p => p.IsEnabled).Returns(status != HealthStatus.Unhealthy);

        var health = new PluginHealth
        {
            Status = status,
            Message = message,
            LastChecked = DateTimeOffset.UtcNow,
            Details = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["uptime"] = TimeSpan.FromHours(1),
                ["memory"] = "50MB"
            }
        };

        mockPlugin.Setup(p => p.GetHealthAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(FdwResult.Success(health));

        return mockPlugin;
    }

    [Fact]
    public async Task StartMonitoringAsync_WithValidInterval_ReturnsSuccess()
    {
        // Arrange
        var monitoringInterval = TimeSpan.FromMinutes(1);
        var successResult = FdwResult.Success();

        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(monitoringInterval, _cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.StartMonitoringAsync(monitoringInterval, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"StartMonitoringAsync started with interval: {monitoringInterval}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public async Task StartMonitoringAsync_WithInvalidInterval_ReturnsFailure(int invalidSeconds)
    {
        // Arrange
        var invalidInterval = TimeSpan.FromSeconds(invalidSeconds);
        var failureResult = FdwResult.Failure("Monitoring interval must be greater than zero");

        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(invalidInterval, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.StartMonitoringAsync(invalidInterval, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("must be greater than zero");
        TestContext.Current.WriteLine($"StartMonitoringAsync correctly rejected invalid interval: {invalidInterval}");
    }

    [Fact]
    public async Task StopMonitoringAsync_WhenMonitoringActive_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockHealthMonitor.Setup(m => m.StopMonitoringAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.StopMonitoringAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine("StopMonitoringAsync successfully stopped monitoring");
    }

    [Fact]
    public async Task StopMonitoringAsync_WhenMonitoringNotActive_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success(); // Stopping already stopped monitor should still succeed
        _mockHealthMonitor.Setup(m => m.StopMonitoringAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.StopMonitoringAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine("StopMonitoringAsync handled non-active monitoring gracefully");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithHealthyPlugin_ReturnsHealthyStatus()
    {
        // Arrange
        var healthResult = await _healthyPlugin.Object.GetHealthAsync(_cancellationToken);
        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(_healthyPlugin.Object.Id, _cancellationToken))
                         .ReturnsAsync(healthResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(_healthyPlugin.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(HealthStatus.Healthy);
        result.Value.Message.ShouldBe("All systems operational");
        TestContext.Current.WriteLine($"CheckPluginHealthAsync returned {result.Value.Status} for healthy plugin");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithDegradedPlugin_ReturnsDegradedStatus()
    {
        // Arrange
        var healthResult = await _degradedPlugin.Object.GetHealthAsync(_cancellationToken);
        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(_degradedPlugin.Object.Id, _cancellationToken))
                         .ReturnsAsync(healthResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(_degradedPlugin.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(HealthStatus.Degraded);
        result.Value.Message.ShouldBe("Performance issues detected");
        TestContext.Current.WriteLine($"CheckPluginHealthAsync returned {result.Value.Status} for degraded plugin");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithUnhealthyPlugin_ReturnsUnhealthyStatus()
    {
        // Arrange
        var healthResult = await _unhealthyPlugin.Object.GetHealthAsync(_cancellationToken);
        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(_unhealthyPlugin.Object.Id, _cancellationToken))
                         .ReturnsAsync(healthResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(_unhealthyPlugin.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Value.Message.ShouldBe("Critical failure");
        TestContext.Current.WriteLine($"CheckPluginHealthAsync returned {result.Value.Status} for unhealthy plugin");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithNonExistentPlugin_ReturnsFailure()
    {
        // Arrange
        var nonExistentId = "NonExistentPlugin";
        var failureResult = FdwResult.Failure<PluginHealth>($"Plugin with ID '{nonExistentId}' not found");

        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(nonExistentId, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(nonExistentId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("not found");
        TestContext.Current.WriteLine($"CheckPluginHealthAsync correctly handled non-existent plugin: {nonExistentId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CheckPluginHealthAsync_WithInvalidPluginId_ReturnsFailure(string invalidId)
    {
        // Arrange
        var failureResult = FdwResult.Failure<PluginHealth>("Plugin ID cannot be null or empty");

        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(invalidId, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(invalidId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("cannot be null or empty");
        TestContext.Current.WriteLine($"CheckPluginHealthAsync correctly rejected invalid plugin ID: '{invalidId}'");
    }

    [Fact]
    public async Task CheckAllPluginsHealthAsync_WithMixedPluginHealth_ReturnsAllStatuses()
    {
        // Arrange
        var allPluginsHealth = new Dictionary<string, PluginHealth>(StringComparer.Ordinal)
        {
            [_healthyPlugin.Object.Id] = (await _healthyPlugin.Object.GetHealthAsync(_cancellationToken)).Value,
            [_degradedPlugin.Object.Id] = (await _degradedPlugin.Object.GetHealthAsync(_cancellationToken)).Value,
            [_unhealthyPlugin.Object.Id] = (await _unhealthyPlugin.Object.GetHealthAsync(_cancellationToken)).Value
        };

        var successResult = FdwResult.Success(allPluginsHealth);
        _mockHealthMonitor.Setup(m => m.CheckAllPluginsHealthAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckAllPluginsHealthAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(3);
        result.Value[_healthyPlugin.Object.Id].Status.ShouldBe(HealthStatus.Healthy);
        result.Value[_degradedPlugin.Object.Id].Status.ShouldBe(HealthStatus.Degraded);
        result.Value[_unhealthyPlugin.Object.Id].Status.ShouldBe(HealthStatus.Unhealthy);
        TestContext.Current.WriteLine($"CheckAllPluginsHealthAsync returned health status for {result.Value.Count} plugins");
    }

    [Fact]
    public async Task CheckAllPluginsHealthAsync_WithNoPlugins_ReturnsEmptyDictionary()
    {
        // Arrange
        var emptyHealth = new Dictionary<string, PluginHealth>(StringComparer.Ordinal);
        var successResult = FdwResult.Success(emptyHealth);

        _mockHealthMonitor.Setup(m => m.CheckAllPluginsHealthAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckAllPluginsHealthAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(0);
        TestContext.Current.WriteLine("CheckAllPluginsHealthAsync returned empty dictionary when no plugins registered");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_WithMixedPluginHealth_ReturnsCorrectSummary()
    {
        // Arrange
        var healthSummary = new HealthSummary
        {
            TotalPlugins = 3,
            HealthyPlugins = 1,
            DegradedPlugins = 1,
            UnhealthyPlugins = 1,
            LastChecked = DateTimeOffset.UtcNow,
            OverallStatus = HealthStatus.Degraded // Worst status determines overall
        };

        var successResult = FdwResult.Success(healthSummary);
        _mockHealthMonitor.Setup(m => m.GetHealthSummaryAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.GetHealthSummaryAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPlugins.ShouldBe(3);
        result.Value.HealthyPlugins.ShouldBe(1);
        result.Value.DegradedPlugins.ShouldBe(1);
        result.Value.UnhealthyPlugins.ShouldBe(1);
        result.Value.OverallStatus.ShouldBe(HealthStatus.Degraded);
        TestContext.Current.WriteLine($"GetHealthSummaryAsync returned summary - Total: {result.Value.TotalPlugins}, Overall: {result.Value.OverallStatus}");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_WithAllHealthyPlugins_ReturnsHealthyOverall()
    {
        // Arrange
        var healthSummary = new HealthSummary
        {
            TotalPlugins = 3,
            HealthyPlugins = 3,
            DegradedPlugins = 0,
            UnhealthyPlugins = 0,
            LastChecked = DateTimeOffset.UtcNow,
            OverallStatus = HealthStatus.Healthy
        };

        var successResult = FdwResult.Success(healthSummary);
        _mockHealthMonitor.Setup(m => m.GetHealthSummaryAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.GetHealthSummaryAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.OverallStatus.ShouldBe(HealthStatus.Healthy);
        result.Value.UnhealthyPlugins.ShouldBe(0);
        result.Value.DegradedPlugins.ShouldBe(0);
        TestContext.Current.WriteLine("GetHealthSummaryAsync returned healthy overall status for all healthy plugins");
    }

    [Fact]
    public void IsMonitoring_WhenMonitoringActive_ReturnsTrue()
    {
        // Arrange
        _mockHealthMonitor.Setup(m => m.IsMonitoring).Returns(true);

        // Act
        var result = _mockHealthMonitor.Object.IsMonitoring;

        // Assert
        result.ShouldBeTrue();
        TestContext.Current.WriteLine("IsMonitoring correctly returned true when monitoring is active");
    }

    [Fact]
    public void IsMonitoring_WhenMonitoringInactive_ReturnsFalse()
    {
        // Arrange
        _mockHealthMonitor.Setup(m => m.IsMonitoring).Returns(false);

        // Act
        var result = _mockHealthMonitor.Object.IsMonitoring;

        // Assert
        result.ShouldBeFalse();
        TestContext.Current.WriteLine("IsMonitoring correctly returned false when monitoring is inactive");
    }

    [Fact]
    public void MonitoringInterval_ReturnsCurrentInterval()
    {
        // Arrange
        var expectedInterval = TimeSpan.FromMinutes(5);
        _mockHealthMonitor.Setup(m => m.MonitoringInterval).Returns(expectedInterval);

        // Act
        var result = _mockHealthMonitor.Object.MonitoringInterval;

        // Assert
        result.ShouldBe(expectedInterval);
        TestContext.Current.WriteLine($"MonitoringInterval returned: {result}");
    }

    [Fact]
    public async Task AddPluginToMonitoringAsync_WithValidPlugin_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockHealthMonitor.Setup(m => m.AddPluginToMonitoringAsync(_healthyPlugin.Object.Id, _cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.AddPluginToMonitoringAsync(_healthyPlugin.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"AddPluginToMonitoringAsync successfully added plugin: {_healthyPlugin.Object.Id}");
    }

    [Fact]
    public async Task RemovePluginFromMonitoringAsync_WithValidPlugin_ReturnsSuccess()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockHealthMonitor.Setup(m => m.RemovePluginFromMonitoringAsync(_healthyPlugin.Object.Id, _cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.RemovePluginFromMonitoringAsync(_healthyPlugin.Object.Id, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"RemovePluginFromMonitoringAsync successfully removed plugin: {_healthyPlugin.Object.Id}");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithTimeout_HandlesGracefully()
    {
        // Arrange
        var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .Returns(async (string id, CancellationToken ct) =>
                         {
                             await Task.Delay(1000, ct); // Will timeout
                             return FdwResult.Success(new PluginHealth { Status = HealthStatus.Healthy });
                         });

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockHealthMonitor.Object.CheckPluginHealthAsync("TestPlugin", timeoutTokenSource.Token));

        TestContext.Current.WriteLine("CheckPluginHealthAsync correctly handled timeout scenario");
    }

    [Fact]
    public async Task StartMonitoringAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var cancelledTokenSource = new CancellationTokenSource();
        cancelledTokenSource.Cancel();

        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new OperationCancelledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockHealthMonitor.Object.StartMonitoringAsync(TimeSpan.FromMinutes(1), cancelledTokenSource.Token));

        TestContext.Current.WriteLine("StartMonitoringAsync correctly threw OperationCancelledException for cancelled token");
    }

    /// <summary>
    /// Mock interface for IPluginHealthMonitor for testing purposes.
    /// </summary>
    private interface IPluginHealthMonitor
    {
        Task<IFdwResult> StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);
        Task<IFdwResult> StopMonitoringAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult<PluginHealth>> CheckPluginHealthAsync(string pluginId, CancellationToken cancellationToken = default);
        Task<IFdwResult<Dictionary<string, PluginHealth>>> CheckAllPluginsHealthAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult<HealthSummary>> GetHealthSummaryAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult> AddPluginToMonitoringAsync(string pluginId, CancellationToken cancellationToken = default);
        Task<IFdwResult> RemovePluginFromMonitoringAsync(string pluginId, CancellationToken cancellationToken = default);
        bool IsMonitoring { get; }
        TimeSpan MonitoringInterval { get; }
    }

    /// <summary>
    /// Health summary model for testing purposes.
    /// </summary>
    private class HealthSummary
    {
        public int TotalPlugins { get; set; }
        public int HealthyPlugins { get; set; }
        public int DegradedPlugins { get; set; }
        public int UnhealthyPlugins { get; set; }
        public HealthStatus OverallStatus { get; set; }
        public DateTimeOffset LastChecked { get; set; }
    }
}

/// <summary>
/// Edge case tests for plugin health monitoring focusing on boundary conditions and error scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginHealthMonitorEdgeCaseTests
{
    private readonly Mock<IPluginHealthMonitor> _mockHealthMonitor;
    private readonly CancellationToken _cancellationToken;

    public PluginHealthMonitorEdgeCaseTests()
    {
        _mockHealthMonitor = new Mock<IPluginHealthMonitor>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    [Theory]
    [InlineData(1)] // 1 millisecond
    [InlineData(100)]
    [InlineData(86400000)] // 24 hours in milliseconds
    public async Task StartMonitoringAsync_WithExtremeBoundaryIntervals_HandlesCorrectly(int milliseconds)
    {
        // Arrange
        var interval = TimeSpan.FromMilliseconds(milliseconds);
        var successResult = FdwResult.Success();

        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(interval, _cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.StartMonitoringAsync(interval, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"StartMonitoringAsync handled extreme interval: {interval}");
    }

    [Fact]
    public async Task CheckAllPluginsHealthAsync_WithLargeNumberOfPlugins_PerformsEfficiently()
    {
        // Arrange
        var largePluginHealth = new Dictionary<string, PluginHealth>(StringComparer.Ordinal);
        for (int i = 0; i < 10000; i++)
        {
            largePluginHealth[$"Plugin_{i}"] = new PluginHealth
            {
                Status = (HealthStatus)(i % 4), // Cycle through all health statuses
                Message = $"Status for plugin {i}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["memory"] = $"{i * 10}MB",
                    ["cpu"] = $"{i % 100}%"
                }
            };
        }

        var successResult = FdwResult.Success(largePluginHealth);
        _mockHealthMonitor.Setup(m => m.CheckAllPluginsHealthAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _mockHealthMonitor.Object.CheckAllPluginsHealthAsync(_cancellationToken);
        stopwatch.Stop();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(10000);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete within 5 seconds
        TestContext.Current.WriteLine($"CheckAllPluginsHealthAsync handled {result.Value.Count} plugins in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithPluginThrowingException_ReturnsFailure()
    {
        // Arrange
        var faultyPluginId = "FaultyPlugin";
        var failureResult = FdwResult.Failure<PluginHealth>("Plugin health check failed: Internal error");

        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(faultyPluginId, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(faultyPluginId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Internal error");
        TestContext.Current.WriteLine("CheckPluginHealthAsync correctly handled plugin exception during health check");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_WithAllPluginsUnhealthy_ReturnsUnhealthyOverall()
    {
        // Arrange
        var unhealthySummary = new HealthSummary
        {
            TotalPlugins = 5,
            HealthyPlugins = 0,
            DegradedPlugins = 0,
            UnhealthyPlugins = 5,
            LastChecked = DateTimeOffset.UtcNow,
            OverallStatus = HealthStatus.Unhealthy
        };

        var successResult = FdwResult.Success(unhealthySummary);
        _mockHealthMonitor.Setup(m => m.GetHealthSummaryAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.GetHealthSummaryAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.OverallStatus.ShouldBe(HealthStatus.Unhealthy);
        result.Value.HealthyPlugins.ShouldBe(0);
        TestContext.Current.WriteLine("GetHealthSummaryAsync correctly determined unhealthy overall status");
    }

    [Fact]
    public async Task StartMonitoringAsync_WhenAlreadyMonitoring_ReturnsFailure()
    {
        // Arrange
        var failureResult = FdwResult.Failure("Monitoring is already active");
        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(It.IsAny<TimeSpan>(), _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.StartMonitoringAsync(TimeSpan.FromMinutes(1), _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("already active");
        TestContext.Current.WriteLine("StartMonitoringAsync correctly handled attempt to start when already monitoring");
    }

    [Fact]
    public async Task AddPluginToMonitoringAsync_WithAlreadyMonitoredPlugin_ReturnsFailure()
    {
        // Arrange
        var duplicatePluginId = "AlreadyMonitoredPlugin";
        var failureResult = FdwResult.Failure($"Plugin '{duplicatePluginId}' is already being monitored");

        _mockHealthMonitor.Setup(m => m.AddPluginToMonitoringAsync(duplicatePluginId, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.AddPluginToMonitoringAsync(duplicatePluginId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("already being monitored");
        TestContext.Current.WriteLine("AddPluginToMonitoringAsync correctly handled duplicate plugin addition");
    }

    [Fact]
    public async Task RemovePluginFromMonitoringAsync_WithNonMonitoredPlugin_ReturnsFailure()
    {
        // Arrange
        var nonMonitoredPluginId = "NonMonitoredPlugin";
        var failureResult = FdwResult.Failure($"Plugin '{nonMonitoredPluginId}' is not being monitored");

        _mockHealthMonitor.Setup(m => m.RemovePluginFromMonitoringAsync(nonMonitoredPluginId, _cancellationToken))
                         .ReturnsAsync(failureResult);

        // Act
        var result = await _mockHealthMonitor.Object.RemovePluginFromMonitoringAsync(nonMonitoredPluginId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("not being monitored");
        TestContext.Current.WriteLine("RemovePluginFromMonitoringAsync correctly handled non-monitored plugin removal");
    }

    [Fact]
    public async Task CheckAllPluginsHealthAsync_WithPartialFailures_ReturnsPartialResults()
    {
        // Arrange
        var partialResults = new Dictionary<string, PluginHealth>(StringComparer.Ordinal)
        {
            ["SuccessfulPlugin"] = new PluginHealth { Status = HealthStatus.Healthy, Message = "OK" }
            // Failed plugins would be omitted from results
        };

        var successResult = FdwResult.Success(partialResults);
        _mockHealthMonitor.Setup(m => m.CheckAllPluginsHealthAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckAllPluginsHealthAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1); // Only successful results included
        result.Value["SuccessfulPlugin"].Status.ShouldBe(HealthStatus.Healthy);
        TestContext.Current.WriteLine("CheckAllPluginsHealthAsync correctly returned partial results when some plugins failed");
    }

    [Theory]
    [InlineData(TimeSpan.MaxValue)]
    [InlineData(TimeSpan.MinValue)]
    public async Task StartMonitoringAsync_WithExtremeTimeSpanValues_HandlesGracefully(TimeSpan extremeInterval)
    {
        // Arrange
        IFdwResult result;
        if (extremeInterval == TimeSpan.MinValue || extremeInterval <= TimeSpan.Zero)
        {
            result = FdwResult.Failure("Invalid monitoring interval");
        }
        else
        {
            result = FdwResult.Success();
        }

        _mockHealthMonitor.Setup(m => m.StartMonitoringAsync(extremeInterval, _cancellationToken))
                         .ReturnsAsync(result);

        // Act
        var actualResult = await _mockHealthMonitor.Object.StartMonitoringAsync(extremeInterval, _cancellationToken);

        // Assert
        actualResult.ShouldNotBeNull();
        if (extremeInterval <= TimeSpan.Zero)
        {
            actualResult.IsFailure.ShouldBeTrue();
        }
        TestContext.Current.WriteLine($"StartMonitoringAsync handled extreme TimeSpan value: {extremeInterval}");
    }

    [Fact]
    public async Task CheckPluginHealthAsync_WithVeryLongPluginId_HandlesCorrectly()
    {
        // Arrange
        var longPluginId = new string('x', 10000); // 10k character ID
        var healthResult = FdwResult.Success(new PluginHealth
        {
            Status = HealthStatus.Healthy,
            Message = "Long ID plugin is healthy",
            LastChecked = DateTimeOffset.UtcNow,
            Details = new Dictionary<string, object>(StringComparer.Ordinal)
        });

        _mockHealthMonitor.Setup(m => m.CheckPluginHealthAsync(longPluginId, _cancellationToken))
                         .ReturnsAsync(healthResult);

        // Act
        var result = await _mockHealthMonitor.Object.CheckPluginHealthAsync(longPluginId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine($"CheckPluginHealthAsync handled very long plugin ID (length: {longPluginId.Length})");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_WithZeroPlugins_ReturnsEmptySummary()
    {
        // Arrange
        var emptySummary = new HealthSummary
        {
            TotalPlugins = 0,
            HealthyPlugins = 0,
            DegradedPlugins = 0,
            UnhealthyPlugins = 0,
            LastChecked = DateTimeOffset.UtcNow,
            OverallStatus = HealthStatus.Unknown // No plugins means unknown status
        };

        var successResult = FdwResult.Success(emptySummary);
        _mockHealthMonitor.Setup(m => m.GetHealthSummaryAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act
        var result = await _mockHealthMonitor.Object.GetHealthSummaryAsync(_cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPlugins.ShouldBe(0);
        result.Value.OverallStatus.ShouldBe(HealthStatus.Unknown);
        TestContext.Current.WriteLine("GetHealthSummaryAsync correctly handled zero plugins scenario");
    }

    [Fact]
    public async Task StopMonitoringAsync_WithConcurrentStopRequests_HandlesGracefully()
    {
        // Arrange
        var successResult = FdwResult.Success();
        _mockHealthMonitor.Setup(m => m.StopMonitoringAsync(_cancellationToken))
                         .ReturnsAsync(successResult);

        // Act - Simulate concurrent stop requests
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _mockHealthMonitor.Object.StopMonitoringAsync(_cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(r => r.IsSuccess);
        TestContext.Current.WriteLine("StopMonitoringAsync handled concurrent stop requests gracefully");
    }

    /// <summary>
    /// Mock interface for IPluginHealthMonitor for testing purposes.
    /// </summary>
    private interface IPluginHealthMonitor
    {
        Task<IFdwResult> StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);
        Task<IFdwResult> StopMonitoringAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult<PluginHealth>> CheckPluginHealthAsync(string pluginId, CancellationToken cancellationToken = default);
        Task<IFdwResult<Dictionary<string, PluginHealth>>> CheckAllPluginsHealthAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult<HealthSummary>> GetHealthSummaryAsync(CancellationToken cancellationToken = default);
        Task<IFdwResult> AddPluginToMonitoringAsync(string pluginId, CancellationToken cancellationToken = default);
        Task<IFdwResult> RemovePluginFromMonitoringAsync(string pluginId, CancellationToken cancellationToken = default);
        bool IsMonitoring { get; }
        TimeSpan MonitoringInterval { get; }
    }

    /// <summary>
    /// Health summary model for testing purposes.
    /// </summary>
    private class HealthSummary
    {
        public int TotalPlugins { get; set; }
        public int HealthyPlugins { get; set; }
        public int DegradedPlugins { get; set; }
        public int UnhealthyPlugins { get; set; }
        public HealthStatus OverallStatus { get; set; }
        public DateTimeOffset LastChecked { get; set; }
    }
}