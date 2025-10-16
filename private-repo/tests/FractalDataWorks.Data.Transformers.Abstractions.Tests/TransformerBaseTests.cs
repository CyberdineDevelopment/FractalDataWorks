using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Transformers.Abstractions.Tests;

public class TransformerBaseTests
{
    private class TestTransformer : TransformerBase<string, int>
    {
        public TestTransformer() : base(id: 1, name: "TestTransformer")
        {
        }

        public override IGenericResult<IEnumerable<int>> Transform(
            IEnumerable<string> source,
            TransformContext context,
            CancellationToken cancellationToken = default)
        {
            var results = source.Select(s => int.Parse(s));
            return GenericResult<IEnumerable<int>>.Success(results);
        }
    }

    private class InvalidTransformer : TransformerBase<string, int>
    {
        public InvalidTransformer(int id, string name) : base(id, name)
        {
        }

        public override IGenericResult<IEnumerable<int>> Transform(
            IEnumerable<string> source,
            TransformContext context,
            CancellationToken cancellationToken = default)
        {
            return GenericResult<IEnumerable<int>>.Success(Enumerable.Empty<int>());
        }
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new InvalidTransformer(1, null!));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new InvalidTransformer(1, string.Empty));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new InvalidTransformer(1, "   "));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var transformer = new TestTransformer();

        // Assert
        transformer.Id.ShouldBe(1);
        transformer.Name.ShouldBe("TestTransformer");
        transformer.SourceType.ShouldBe(typeof(string));
        transformer.TargetType.ShouldBe(typeof(int));
    }

    [Fact]
    public void Transform_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var transformer = new TestTransformer();
        var source = new[] { "1", "2", "3" };
        var context = new TransformContext();

        // Act
        var result = transformer.Transform(source, context,TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Transform_SupportsAsyncWithCancellationToken()
    {
        // Arrange
        var transformer = new TestTransformer();
        var source = new[] { "1", "2" };
        var context = new TransformContext();
        var cts = new CancellationTokenSource();

        // Act
        var result = transformer.Transform(source, context, cts.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
