using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Abstractions.Tests;

public class InputHashCalculatorTests
{
    private class TestInputInfoModel : IInputInfoModel
    {
        private readonly string _content;

        public TestInputInfoModel(string content)
        {
            _content = content;
            InputHash = InputHashCalculator.CalculateHash(this);
        }

        public string InputHash { get; }

        public void WriteToHash(TextWriter writer)
        {
            writer.Write(_content);
        }
    }

    [Fact]
    public void CalculateHash_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => InputHashCalculator.CalculateHash(null!));
        exception.ParamName.ShouldBe("inputInfoModel");
        exception.Message.ShouldContain("Input info cannot be null.");
    }

    [Fact]
    public void CalculateHash_WithValidInput_ReturnsNonEmptyHash()
    {
        // Arrange
        var model = new TestInputInfoModel("test content");

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_WithSameContent_ReturnsSameHash()
    {
        // Arrange
        var model1 = new TestInputInfoModel("test content");
        var model2 = new TestInputInfoModel("test content");

        // Act
        var hash1 = InputHashCalculator.CalculateHash(model1);
        var hash2 = InputHashCalculator.CalculateHash(model2);

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void CalculateHash_WithDifferentContent_ReturnsDifferentHash()
    {
        // Arrange
        var model1 = new TestInputInfoModel("test content 1");
        var model2 = new TestInputInfoModel("test content 2");

        // Act
        var hash1 = InputHashCalculator.CalculateHash(model1);
        var hash2 = InputHashCalculator.CalculateHash(model2);

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void CalculateHash_WithEmptyContent_ReturnsValidHash()
    {
        // Arrange
        var model = new TestInputInfoModel("");

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_WithLargeContent_ReturnsValidHash()
    {
        // Arrange
        var largeContent = new string('x', 10000);
        var model = new TestInputInfoModel(largeContent);

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_WithSpecialCharacters_ReturnsValidHash()
    {
        // Arrange
        var specialContent = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";
        var model = new TestInputInfoModel(specialContent);

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_WithUnicodeCharacters_ReturnsValidHash()
    {
        // Arrange
        var unicodeContent = "Hello 世界 مرحبا мир";
        var model = new TestInputInfoModel(unicodeContent);

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_WithNewlines_ReturnsValidHash()
    {
        // Arrange
        var contentWithNewlines = "Line 1\nLine 2\r\nLine 3";
        var model = new TestInputInfoModel(contentWithNewlines);

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateHash_CalledMultipleTimes_ReturnsSameHash()
    {
        // Arrange
        var model = new TestInputInfoModel("test content");

        // Act
        var hash1 = InputHashCalculator.CalculateHash(model);
        var hash2 = InputHashCalculator.CalculateHash(model);
        var hash3 = InputHashCalculator.CalculateHash(model);

        // Assert
        hash1.ShouldBe(hash2);
        hash2.ShouldBe(hash3);
    }

    [Fact]
    public void CalculateHash_IsBase64Encoded()
    {
        // Arrange
        var model = new TestInputInfoModel("test content");

        // Act
        var hash = InputHashCalculator.CalculateHash(model);

        // Assert
        // Base64 strings should be valid and not throw when converting
        Should.NotThrow(() => Convert.FromBase64String(hash));
    }
}
