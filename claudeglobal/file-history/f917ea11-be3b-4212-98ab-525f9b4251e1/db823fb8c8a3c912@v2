using System;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Collections.Tests;

/// <summary>
/// Tests for TypeOptionBase covering all pathways for 100% coverage.
/// </summary>
public class TypeOptionBaseTests
{
    private sealed class TestTypeOption : TypeOptionBase<TestTypeOption>
    {
        public TestTypeOption(int id, string name) : base(id, name) { }

        public TestTypeOption(int id, string name, string? category) : base(id, name, category) { }

        public TestTypeOption(int id, string name, string configurationKey, string displayName, string description, string? category)
            : base(id, name, configurationKey, displayName, description, category) { }
    }

    [Fact]
    public void Constructor_TwoParameters_InitializesIdAndName()
    {
        var option = new TestTypeOption(42, "TestOption");

        option.Id.ShouldBe(42);
        option.Name.ShouldBe("TestOption");
    }

    [Fact]
    public void Constructor_TwoParameters_SetsDefaultCategory()
    {
        var option = new TestTypeOption(1, "Test");

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_TwoParameters_SetsDefaultMetadata()
    {
        var option = new TestTypeOption(1, "TestName");

        option.ConfigurationKey.ShouldBe("TypeOptions:TestName");
        option.DisplayName.ShouldBe("TestName");
        option.Description.ShouldBe("Type option: TestName");
    }

    [Theory]
    [InlineData(0, "Zero")]
    [InlineData(1, "One")]
    [InlineData(-1, "Negative")]
    [InlineData(int.MaxValue, "Max")]
    public void Constructor_TwoParameters_WithVariousValues_InitializesCorrectly(int id, string name)
    {
        var option = new TestTypeOption(id, name);

        option.Id.ShouldBe(id);
        option.Name.ShouldBe(name);
    }

    [Fact]
    public void Constructor_TwoParameters_WithNullName_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new TestTypeOption(1, null!));
    }

    [Fact]
    public void Constructor_ThreeParameters_InitializesIdNameAndCategory()
    {
        var option = new TestTypeOption(1, "Test", "TestCategory");

        option.Id.ShouldBe(1);
        option.Name.ShouldBe("Test");
        option.Category.ShouldBe("TestCategory");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithNullCategory_SetsDefaultCategory()
    {
        var option = new TestTypeOption(1, "Test", null);

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithEmptyCategory_SetsDefaultCategory()
    {
        var option = new TestTypeOption(1, "Test", string.Empty);

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithNullName_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new TestTypeOption(1, null!, "Category"));
    }

    [Fact]
    public void Constructor_SixParameters_InitializesAllProperties()
    {
        var option = new TestTypeOption(
            id: 123,
            name: "MyType",
            configurationKey: "CustomKey",
            displayName: "My Type Display",
            description: "Custom description",
            category: "CustomCategory");

        option.Id.ShouldBe(123);
        option.Name.ShouldBe("MyType");
        option.ConfigurationKey.ShouldBe("CustomKey");
        option.DisplayName.ShouldBe("My Type Display");
        option.Description.ShouldBe("Custom description");
        option.Category.ShouldBe("CustomCategory");
    }

    [Fact]
    public void Constructor_SixParameters_WithNullConfigurationKey_SetsDefault()
    {
        var option = new TestTypeOption(1, "Test", null!, "Display", "Desc", "Cat");

        option.ConfigurationKey.ShouldBe("TypeOptions:Test");
    }

    [Fact]
    public void Constructor_SixParameters_WithNullDisplayName_UsesName()
    {
        var option = new TestTypeOption(1, "Test", "Key", null!, "Desc", "Cat");

        option.DisplayName.ShouldBe("Test");
    }

    [Fact]
    public void Constructor_SixParameters_WithNullDescription_SetsDefault()
    {
        var option = new TestTypeOption(1, "Test", "Key", "Display", null!, "Cat");

        option.Description.ShouldBe("Type option: Test");
    }

    [Fact]
    public void Constructor_SixParameters_WithNullCategory_SetsDefaultCategory()
    {
        var option = new TestTypeOption(1, "Test", "Key", "Display", "Desc", null);

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_SixParameters_WithNullName_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new TestTypeOption(1, null!, "Key", "Display", "Desc", "Cat"));
    }

    [Theory]
    [InlineData(null, "NotCategorized")]
    [InlineData("", "NotCategorized")]
    [InlineData("ValidCategory", "ValidCategory")]
    [InlineData("  ", "  ")]
    public void Category_ReturnsCorrectValue(string? category, string expected)
    {
        var option = new TestTypeOption(1, "Test", category);

        option.Category.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ReturnCorrectValuesForFullConstructor()
    {
        var option = new TestTypeOption(
            id: 999,
            name: "FullTest",
            configurationKey: "FullKey",
            displayName: "Full Test Display",
            description: "Full test description",
            category: "FullCategory");

        option.Id.ShouldBe(999);
        option.Name.ShouldBe("FullTest");
        option.ConfigurationKey.ShouldBe("FullKey");
        option.DisplayName.ShouldBe("Full Test Display");
        option.Description.ShouldBe("Full test description");
        option.Category.ShouldBe("FullCategory");
    }
}
