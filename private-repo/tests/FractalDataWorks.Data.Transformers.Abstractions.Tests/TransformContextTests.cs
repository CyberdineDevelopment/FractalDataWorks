using FractalDataWorks.Data.Transformers.Abstractions;
using Shouldly;using System;using System.Collections.Generic;using Xunit;
namespace FractalDataWorks.Data.Transformers.Abstractions.Tests;

public class TransformContextTests
{
    [Fact]
    public void Constructor_CreatesEmptyMetadataDictionary()
    {
        // Act
        var context = new TransformContext();

        // Assert
        context.Metadata.ShouldNotBeNull();
        context.Metadata.ShouldBeEmpty();
        context.SourceName.ShouldBe(string.Empty);
        context.ConnectionType.ShouldBe(string.Empty);
        context.ThrowOnError.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithMetadata_CopiesMetadata()
    {
        // Arrange
        var originalMetadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42,
            ["Key3"] = true
        };

        // Act
        var context = new TransformContext(originalMetadata);

        // Assert
        context.Metadata.Count.ShouldBe(3);
        context.Metadata["Key1"].ShouldBe("Value1");
        context.Metadata["Key2"].ShouldBe(42);
        context.Metadata["Key3"].ShouldBe(true);
    }

    [Fact]
    public void Constructor_WithMetadata_CreatesSeparateCopy()
    {
        // Arrange
        var originalMetadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1"
        };

        var context = new TransformContext(originalMetadata);

        // Act - Modify original
        originalMetadata["Key2"] = "Value2";

        // Assert - Context metadata unaffected
        context.Metadata.ContainsKey("Key2").ShouldBeFalse();
    }

    [Fact]
    public void Metadata_IsCaseInsensitive()
    {
        // Arrange
        var context = new TransformContext();
        context.Metadata["TestKey"] = "Value";

        // Act & Assert
        context.Metadata["testkey"].ShouldBe("Value");
        context.Metadata["TESTKEY"].ShouldBe("Value");
        context.Metadata["TestKey"].ShouldBe("Value");
    }

    [Fact]
    public void SourceName_CanBeSet()
    {
        // Arrange
        var context = new TransformContext
        {
            SourceName = "PayPal"
        };

        // Assert
        context.SourceName.ShouldBe("PayPal");
    }

    [Fact]
    public void ConnectionType_CanBeSet()
    {
        // Arrange
        var context = new TransformContext
        {
            ConnectionType = "Rest"
        };

        // Assert
        context.ConnectionType.ShouldBe("Rest");
    }

    [Fact]
    public void ThrowOnError_CanBeSet()
    {
        // Arrange
        var context = new TransformContext
        {
            ThrowOnError = false
        };

        // Assert
        context.ThrowOnError.ShouldBeFalse();
    }

    [Fact]
    public void Metadata_CanBeModified()
    {
        // Arrange
        var context = new TransformContext();

        // Act
        context.Metadata["NewKey"] = "NewValue";

        // Assert
        context.Metadata["NewKey"].ShouldBe("NewValue");
    }
}
