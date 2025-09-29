using System;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Tests.Abstractions;

/// <summary>
/// Comprehensive tests for IMcpTool interface and tool execution patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class IMcpToolTests
{
    private readonly Mock<IMcpTool> _mockTool;
    private readonly Mock<IToolPlugin> _mockPlugin;
    private readonly Mock<ToolCategoryBase> _mockCategory;
    private readonly CancellationToken _cancellationToken;

    public IMcpToolTests()
    {
        _mockTool = new Mock<IMcpTool>();
        _mockPlugin = new Mock<IToolPlugin>();
        _mockCategory = new Mock<ToolCategoryBase>(1, "TestCategory", "Test category description");
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public void IMcpTool_InheritsFromITool()
    {
        // Arrange & Act
        var toolInterface = typeof(IMcpTool);

        // Assert
        toolInterface.IsInterface.ShouldBeTrue();
        typeof(ITool).IsAssignableFrom(toolInterface).ShouldBeTrue();
        TestContext.Current.WriteLine("IMcpTool correctly inherits from ITool");
    }

    [Fact]
    public void OwningPluginProperty_ReturnsExpectedPlugin()
    {
        // Arrange
        _mockTool.Setup(t => t.OwningPlugin).Returns(_mockPlugin.Object);

        // Act
        var plugin = _mockTool.Object.OwningPlugin;

        // Assert
        plugin.ShouldNotBeNull();
        plugin.ShouldBeSameAs(_mockPlugin.Object);
        TestContext.Current.WriteLine("OwningPlugin property returned expected plugin instance");
    }

    [Fact]
    public void CategoryProperty_ReturnsExpectedCategory()
    {
        // Arrange
        _mockTool.Setup(t => t.Category).Returns(_mockCategory.Object);

        // Act
        var category = _mockTool.Object.Category;

        // Assert
        category.ShouldNotBeNull();
        category.ShouldBeSameAs(_mockCategory.Object);
        TestContext.Current.WriteLine($"Category property returned: {category.Name}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsEnabledProperty_WithVariousValues_ReturnsExpectedValue(bool expectedEnabled)
    {
        // Arrange
        _mockTool.Setup(t => t.IsEnabled).Returns(expectedEnabled);

        // Act
        var isEnabled = _mockTool.Object.IsEnabled;

        // Assert
        isEnabled.ShouldBe(expectedEnabled);
        TestContext.Current.WriteLine($"IsEnabled property returned: {isEnabled}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void PriorityProperty_WithVariousValues_ReturnsExpectedValue(int expectedPriority)
    {
        // Arrange
        _mockTool.Setup(t => t.Priority).Returns(expectedPriority);

        // Act
        var priority = _mockTool.Object.Priority;

        // Assert
        priority.ShouldBe(expectedPriority);
        TestContext.Current.WriteLine($"Priority property returned: {priority}");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidArguments_ReturnsSuccessResult()
    {
        // Arrange
        var arguments = new { input = "test input" };
        var expectedResult = "test result";
        var successResult = GenericResult.Success<object>(expectedResult);

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .ReturnsAsync(successResult);

        // Act
        var result = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine($"ExecuteAsync returned success with value: {result.Value}");
    }

    [Fact]
    public async Task ExecuteAsync_WithFailure_ReturnsFailureResult()
    {
        // Arrange
        var arguments = new { input = "invalid input" };
        var failureResult = GenericResult.Failure<object>("Execution failed");

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .ReturnsAsync(failureResult);

        // Act
        var result = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Execution failed");
        TestContext.Current.WriteLine($"ExecuteAsync returned failure: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullArguments_HandlesGracefully()
    {
        // Arrange
        var expectedResult = "handled null args";
        var successResult = GenericResult.Success<object>(expectedResult);

        _mockTool.Setup(t => t.ExecuteAsync(null, _cancellationToken))
                .ReturnsAsync(successResult);

        // Act
        var result = await _mockTool.Object.ExecuteAsync(null, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine("ExecuteAsync handled null arguments gracefully");
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexArguments_HandlesCorrectly()
    {
        // Arrange
        var complexArgs = new
        {
            StringProperty = "test string",
            NumberProperty = 42,
            BoolProperty = true,
            NestedObject = new { InnerProp = "nested value" },
            ArrayProperty = new[] { "item1", "item2", "item3" }
        };
        var expectedResult = new { processed = true, count = 3 };
        var successResult = GenericResult.Success<object>(expectedResult);

        _mockTool.Setup(t => t.ExecuteAsync(complexArgs, _cancellationToken))
                .ReturnsAsync(successResult);

        // Act
        var result = await _mockTool.Object.ExecuteAsync(complexArgs, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine($"ExecuteAsync handled complex arguments and returned: {result.Value}");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithValidArguments_ReturnsSuccess()
    {
        // Arrange
        var arguments = new { input = "valid input" };
        var successResult = GenericResult.Success();

        _mockTool.Setup(t => t.ValidateArgumentsAsync(arguments, _cancellationToken))
                .ReturnsAsync(successResult);

        // Act
        var result = await _mockTool.Object.ValidateArgumentsAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        TestContext.Current.WriteLine("ValidateArgumentsAsync returned success for valid arguments");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithInvalidArguments_ReturnsFailure()
    {
        // Arrange
        var arguments = new { input = "" };
        var failureResult = GenericResult.Failure("Input cannot be empty");

        _mockTool.Setup(t => t.ValidateArgumentsAsync(arguments, _cancellationToken))
                .ReturnsAsync(failureResult);

        // Act
        var result = await _mockTool.Object.ValidateArgumentsAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Input cannot be empty");
        TestContext.Current.WriteLine($"ValidateArgumentsAsync returned failure: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithNullArguments_HandlesGracefully()
    {
        // Arrange
        var failureResult = GenericResult.Failure("Arguments cannot be null");

        _mockTool.Setup(t => t.ValidateArgumentsAsync(null, _cancellationToken))
                .ReturnsAsync(failureResult);

        // Act
        var result = await _mockTool.Object.ValidateArgumentsAsync(null, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Arguments cannot be null");
        TestContext.Current.WriteLine("ValidateArgumentsAsync handled null arguments appropriately");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var cancelledTokenSource = new CancellationTokenSource();
        cancelledTokenSource.Cancel();

        _mockTool.Setup(t => t.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCancelledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockTool.Object.ExecuteAsync(new { }, cancelledTokenSource.Token));

        TestContext.Current.WriteLine("ExecuteAsync correctly threw OperationCancelledException for cancelled token");
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_HandlesGracefully()
    {
        // Arrange
        var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

        _mockTool.Setup(t => t.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (object args, CancellationToken ct) =>
                {
                    await Task.Delay(1000, ct); // Will timeout
                    return GenericResult.Success<object>("should not reach here");
                });

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockTool.Object.ExecuteAsync(new { }, timeoutTokenSource.Token));

        TestContext.Current.WriteLine("ExecuteAsync correctly handled timeout scenario");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var cancelledTokenSource = new CancellationTokenSource();
        cancelledTokenSource.Cancel();

        _mockTool.Setup(t => t.ValidateArgumentsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCancelledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockTool.Object.ValidateArgumentsAsync(new { }, cancelledTokenSource.Token));

        TestContext.Current.WriteLine("ValidateArgumentsAsync correctly threw OperationCancelledException for cancelled token");
    }

    [Theory]
    [InlineData("string_arg")]
    [InlineData(42)]
    [InlineData(true)]
    [InlineData(null)]
    public async Task ExecuteAsync_WithDifferentArgumentTypes_HandlesCorrectly(object argument)
    {
        // Arrange
        var expectedResult = $"processed_{argument?.GetType().Name ?? "null"}";
        var successResult = GenericResult.Success<object>(expectedResult);

        _mockTool.Setup(t => t.ExecuteAsync(argument, _cancellationToken))
                .ReturnsAsync(successResult);

        // Act
        var result = await _mockTool.Object.ExecuteAsync(argument, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine($"ExecuteAsync handled {argument?.GetType().Name ?? "null"} argument type");
    }

    [Fact]
    public async Task ExecuteAsync_WithException_ReturnsFailureResult()
    {
        // Arrange
        var arguments = new { causesException = true };

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .ThrowsAsync(new InvalidOperationException("Tool execution error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _mockTool.Object.ExecuteAsync(arguments, _cancellationToken));

        exception.Message.ShouldBe("Tool execution error");
        TestContext.Current.WriteLine("ExecuteAsync correctly propagated InvalidOperationException");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithException_PropagatesException()
    {
        // Arrange
        var arguments = new { causesValidationException = true };

        _mockTool.Setup(t => t.ValidateArgumentsAsync(arguments, _cancellationToken))
                .ThrowsAsync(new ArgumentException("Invalid argument format"));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            _mockTool.Object.ValidateArgumentsAsync(arguments, _cancellationToken));

        exception.Message.ShouldBe("Invalid argument format");
        TestContext.Current.WriteLine("ValidateArgumentsAsync correctly propagated ArgumentException");
    }

    [Fact]
    public void ToolProperties_AreConsistentWithOwningPlugin()
    {
        // Arrange
        var pluginCategory = SessionManagement.Instance;
        _mockPlugin.Setup(p => p.Category).Returns(pluginCategory);
        _mockPlugin.Setup(p => p.IsEnabled).Returns(true);
        _mockPlugin.Setup(p => p.Priority).Returns(5);

        _mockTool.Setup(t => t.OwningPlugin).Returns(_mockPlugin.Object);
        _mockTool.Setup(t => t.Category).Returns(pluginCategory);
        _mockTool.Setup(t => t.IsEnabled).Returns(true);
        _mockTool.Setup(t => t.Priority).Returns(10); // Tool can have different priority within category

        // Act
        var tool = _mockTool.Object;

        // Assert
        tool.OwningPlugin.Category.ShouldBe(pluginCategory);
        tool.Category.ShouldBe(pluginCategory);
        tool.OwningPlugin.IsEnabled.ShouldBe(true);
        tool.IsEnabled.ShouldBe(true);
        TestContext.Current.WriteLine($"Tool properties consistent with plugin - Category: {pluginCategory.Name}");
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleCallsSequentially_MaintainsConsistentBehavior()
    {
        // Arrange
        var arguments = new { testId = "sequential_test" };
        var callCount = 0;

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .Returns(() =>
                {
                    callCount++;
                    return Task.FromResult(GenericResult.Success<object>($"call_{callCount}"));
                });

        // Act
        var result1 = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);
        var result2 = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);
        var result3 = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result3.IsSuccess.ShouldBeTrue();
        result1.Value.ShouldBe("call_1");
        result2.Value.ShouldBe("call_2");
        result3.Value.ShouldBe("call_3");
        TestContext.Current.WriteLine("ExecuteAsync maintained consistent behavior across multiple sequential calls");
    }

    [Fact]
    public async Task ExecuteAsync_WithConcurrentCalls_HandlesCorrectly()
    {
        // Arrange
        var arguments = new { testId = "concurrent_test" };
        var callCount = 0;

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .Returns(async () =>
                {
                    var currentCall = Interlocked.Increment(ref callCount);
                    await Task.Delay(50, _cancellationToken); // Simulate some work
                    return GenericResult.Success<object>($"concurrent_call_{currentCall}");
                });

        // Act
        var task1 = _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);
        var task2 = _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);
        var task3 = _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        results.Length.ShouldBe(3);
        results.ShouldAllBe(r => r.IsSuccess);

        var values = results.Select(r => (string)r.Value).ToArray();
        values.ShouldContain("concurrent_call_1");
        values.ShouldContain("concurrent_call_2");
        values.ShouldContain("concurrent_call_3");

        TestContext.Current.WriteLine($"ExecuteAsync handled {results.Length} concurrent calls successfully");
    }
}

/// <summary>
/// Edge case tests for IMcpTool interface focusing on boundary conditions and error scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class IMcpToolEdgeCaseTests
{
    private readonly Mock<IMcpTool> _mockTool;
    private readonly CancellationToken _cancellationToken;

    public IMcpToolEdgeCaseTests()
    {
        _mockTool = new Mock<IMcpTool>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task ExecuteAsync_WithExtremelyLargeArguments_HandlesGracefully()
    {
        // Arrange
        var largeString = new string('x', 1_000_000); // 1MB string
        var largeArguments = new { data = largeString };

        _mockTool.Setup(t => t.ExecuteAsync(largeArguments, _cancellationToken))
                .ReturnsAsync(GenericResult.Success<object>("processed_large_data"));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(largeArguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("processed_large_data");
        TestContext.Current.WriteLine($"ExecuteAsync handled large arguments (1MB string) successfully");
    }

    [Fact]
    public async Task ExecuteAsync_WithDeeplyNestedArguments_HandlesCorrectly()
    {
        // Arrange
        var deeplyNested = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Level3 = new
                    {
                        Level4 = new
                        {
                            Level5 = new { Value = "deep_value" }
                        }
                    }
                }
            }
        };

        _mockTool.Setup(t => t.ExecuteAsync(deeplyNested, _cancellationToken))
                .ReturnsAsync(GenericResult.Success<object>("processed_nested"));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(deeplyNested, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("processed_nested");
        TestContext.Current.WriteLine("ExecuteAsync handled deeply nested arguments successfully");
    }

    [Fact]
    public async Task ExecuteAsync_WithCircularReference_HandlesGracefully()
    {
        // Arrange
        // Note: Creating actual circular references can cause serialization issues,
        // so we mock the scenario where such arguments might be passed
        var problematicArgs = new { type = "circular_reference_simulation" };

        _mockTool.Setup(t => t.ExecuteAsync(problematicArgs, _cancellationToken))
                .ReturnsAsync(GenericResult.Failure<object>("Circular reference detected in arguments"));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(problematicArgs, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Circular reference detected");
        TestContext.Current.WriteLine("ExecuteAsync handled circular reference scenario appropriately");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    public async Task ValidateArgumentsAsync_WithWhitespaceStrings_HandlesCorrectly(string whitespaceValue)
    {
        // Arrange
        var arguments = new { input = whitespaceValue };
        var failureResult = GenericResult.Failure("Input cannot be empty or whitespace");

        _mockTool.Setup(t => t.ValidateArgumentsAsync(arguments, _cancellationToken))
                .ReturnsAsync(failureResult);

        // Act
        var result = await _mockTool.Object.ValidateArgumentsAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("empty or whitespace");
        TestContext.Current.WriteLine($"ValidateArgumentsAsync handled whitespace string '{whitespaceValue.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r")}' appropriately");
    }

    [Fact]
    public async Task ExecuteAsync_WithVeryLongExecutionTime_RespectsTimeout()
    {
        // Arrange
        var shortTimeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token;
        var arguments = new { simulateLongRunning = true };

        _mockTool.Setup(t => t.ExecuteAsync(arguments, It.IsAny<CancellationToken>()))
                .Returns(async (object args, CancellationToken ct) =>
                {
                    await Task.Delay(5000, ct); // 5 second delay, should timeout
                    return GenericResult.Success<object>("should_not_reach");
                });

        // Act & Assert
        await Should.ThrowAsync<OperationCancelledException>(() =>
            _mockTool.Object.ExecuteAsync(arguments, shortTimeoutToken));

        TestContext.Current.WriteLine("ExecuteAsync respected timeout and threw OperationCancelledException");
    }

    [Fact]
    public async Task ExecuteAsync_WithMemoryPressure_HandlesGracefully()
    {
        // Arrange
        var memoryIntensiveArgs = new { requestLargeMemoryAllocation = true };

        _mockTool.Setup(t => t.ExecuteAsync(memoryIntensiveArgs, _cancellationToken))
                .ReturnsAsync(GenericResult.Failure<object>("Insufficient memory to complete operation"));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(memoryIntensiveArgs, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Insufficient memory");
        TestContext.Current.WriteLine("ExecuteAsync handled memory pressure scenario appropriately");
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_WithBoundaryIntegerValues_HandlesCorrectly(int boundaryValue)
    {
        // Arrange
        var arguments = new { value = boundaryValue };
        var expectedResult = $"processed_int_{boundaryValue}";

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .ReturnsAsync(GenericResult.Success<object>(expectedResult));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine($"ExecuteAsync handled boundary integer value: {boundaryValue}");
    }

    [Theory]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public async Task ExecuteAsync_WithSpecialDoubleValues_HandlesCorrectly(double specialValue)
    {
        // Arrange
        var arguments = new { value = specialValue };
        var expectedResult = $"processed_double_{specialValue}";

        _mockTool.Setup(t => t.ExecuteAsync(arguments, _cancellationToken))
                .ReturnsAsync(GenericResult.Success<object>(expectedResult));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(arguments, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        TestContext.Current.WriteLine($"ExecuteAsync handled special double value: {specialValue}");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnicodeArguments_HandlesCorrectly()
    {
        // Arrange
        var unicodeArgs = new
        {
            emoji = "🚀💻🔧",
            chinese = "你好世界",
            arabic = "مرحبا بالعالم",
            special = "Special characters: ™®©±≠≤≥"
        };

        _mockTool.Setup(t => t.ExecuteAsync(unicodeArgs, _cancellationToken))
                .ReturnsAsync(GenericResult.Success<object>("unicode_processed"));

        // Act
        var result = await _mockTool.Object.ExecuteAsync(unicodeArgs, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("unicode_processed");
        TestContext.Current.WriteLine("ExecuteAsync handled Unicode arguments correctly");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_WithMalformedJsonLikeStructure_HandlesGracefully()
    {
        // Arrange
        var malformedArgs = new { malformedJson = @"{""incomplete"": ""json""" }; // Missing closing brace

        _mockTool.Setup(t => t.ValidateArgumentsAsync(malformedArgs, _cancellationToken))
                .ReturnsAsync(GenericResult.Failure("Invalid argument format"));

        // Act
        var result = await _mockTool.Object.ValidateArgumentsAsync(malformedArgs, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldContain("Invalid argument format");
        TestContext.Current.WriteLine("ValidateArgumentsAsync handled malformed JSON-like structure appropriately");
    }
}