using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for the MessageAttribute class.
/// </summary>
public class MessageAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var attribute = new MessageAttribute();

        // Assert
        attribute.ReturnType.ShouldBe(typeof(IGenericMessage));
        attribute.CollectionName.ShouldBeNull();
        attribute.Name.ShouldBeNull();
        attribute.ReturnTypeNamespace.ShouldBeNull();
        attribute.IncludeInGlobalCollection.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithCollectionName_SetsProperties()
    {
        // Arrange
        const string collectionName = "MyMessages";

        // Act
        var attribute = new MessageAttribute(collectionName);

        // Assert
        attribute.CollectionName.ShouldBe(collectionName);
        attribute.ReturnType.ShouldBe(typeof(IGenericMessage));
        attribute.IncludeInGlobalCollection.ShouldBeTrue();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSetAndRead()
    {
        // Arrange
        var attribute = new MessageAttribute();
        const string collectionName = "CustomCollection";
        const string name = "CustomName";
        var returnType = typeof(GenericMessage);
        const string returnTypeNamespace = "FractalDataWorks.Messages";

        // Act
        attribute.CollectionName = collectionName;
        attribute.Name = name;
        attribute.ReturnType = returnType;
        attribute.ReturnTypeNamespace = returnTypeNamespace;
        attribute.IncludeInGlobalCollection = false;

        // Assert
        attribute.CollectionName.ShouldBe(collectionName);
        attribute.Name.ShouldBe(name);
        attribute.ReturnType.ShouldBe(returnType);
        attribute.ReturnTypeNamespace.ShouldBe(returnTypeNamespace);
        attribute.IncludeInGlobalCollection.ShouldBeFalse();
    }

    #endregion
}
