using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for the MessageCollectionAttribute class.
/// </summary>
public class MessageCollectionAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidName_SetsProperties()
    {
        // Arrange
        const string name = "MyMessageCollection";

        // Act
        var attribute = new MessageCollectionAttribute(name);

        // Assert
        attribute.Name.ShouldBe(name);
        attribute.ReturnType.ShouldBe(typeof(IGenericMessage));
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new MessageCollectionAttribute(null!))
            .ParamName.ShouldBe("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new MessageCollectionAttribute(invalidName));
        exception.ParamName.ShouldBe("name");
        exception.Message.ShouldContain("Name cannot be empty or whitespace");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ReturnType_CanBeSet()
    {
        // Arrange
        var attribute = new MessageCollectionAttribute("TestCollection");
        var newReturnType = typeof(GenericMessage);

        // Act
        attribute.ReturnType = newReturnType;

        // Assert
        attribute.ReturnType.ShouldBe(newReturnType);
    }

    #endregion
}
