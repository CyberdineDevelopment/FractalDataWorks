using FractalDataWorks.Collections;

namespace FractalDataWorks.Collections.Tests;

public class TypeOptionBaseTests
{
    private class TestTypeOption : TypeOptionBase<TestTypeOption>
    {
        public TestTypeOption(int id, string name) : base(id, name) { }
        public TestTypeOption(int id, string name, string? category) : base(id, name, category) { }
        public TestTypeOption(int id, string name, string configKey, string displayName, string description, string? category)
            : base(id, name, configKey, displayName, description, category) { }
    }

    [Fact]
    public void Constructor_TwoParameters_SetsPropertiesCorrectly()
    {
        var option = new TestTypeOption(1, "TestName");

        option.Id.ShouldBe(1);
        option.Name.ShouldBe("TestName");
        option.Category.ShouldBe("NotCategorized");
        option.ConfigurationKey.ShouldBe("TypeOptions:TestName");
        option.DisplayName.ShouldBe("TestName");
        option.Description.ShouldBe("Type option: TestName");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithNullCategory_UsesNotCategorized()
    {
        var option = new TestTypeOption(2, "Test", null);

        option.Id.ShouldBe(2);
        option.Name.ShouldBe("Test");
        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithEmptyCategory_UsesNotCategorized()
    {
        var option = new TestTypeOption(3, "Test", string.Empty);

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_ThreeParameters_WithCategory_UsesProvidedCategory()
    {
        var option = new TestTypeOption(4, "Test", "MyCategory");

        option.Category.ShouldBe("MyCategory");
    }

    [Fact]
    public void Constructor_TwoParameters_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TestTypeOption(1, null!))
            .ParamName.ShouldBe("name");
    }

    [Fact]
    public void Constructor_ThreeParameters_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TestTypeOption(1, null!, "Category"))
            .ParamName.ShouldBe("name");
    }

    [Fact]
    public void Constructor_FullParameters_SetsAllPropertiesCorrectly()
    {
        var option = new TestTypeOption(
            5,
            "TestName",
            "Config:Key",
            "Display Name",
            "Test Description",
            "TestCategory");

        option.Id.ShouldBe(5);
        option.Name.ShouldBe("TestName");
        option.ConfigurationKey.ShouldBe("Config:Key");
        option.DisplayName.ShouldBe("Display Name");
        option.Description.ShouldBe("Test Description");
        option.Category.ShouldBe("TestCategory");
    }

    [Fact]
    public void Constructor_FullParameters_WithNullConfigurationKey_UsesDefault()
    {
        var option = new TestTypeOption(6, "Test", null!, "Display", "Desc", null);

        option.ConfigurationKey.ShouldBe("TypeOptions:Test");
    }

    [Fact]
    public void Constructor_FullParameters_WithNullDisplayName_UsesName()
    {
        var option = new TestTypeOption(7, "Test", "Key", null!, "Desc", null);

        option.DisplayName.ShouldBe("Test");
    }

    [Fact]
    public void Constructor_FullParameters_WithNullDescription_UsesDefault()
    {
        var option = new TestTypeOption(8, "Test", "Key", "Display", null!, null);

        option.Description.ShouldBe("Type option: Test");
    }

    [Fact]
    public void Constructor_FullParameters_WithNullCategory_UsesNotCategorized()
    {
        var option = new TestTypeOption(9, "Test", "Key", "Display", "Desc", null);

        option.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void Constructor_FullParameters_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TestTypeOption(1, null!, "Key", "Display", "Desc", "Cat"))
            .ParamName.ShouldBe("name");
    }

    [Fact]
    public void TypeOptionBase_ImplementsITypeOption()
    {
        var option = new TestTypeOption(10, "Test");
        ITypeOption iOption = option;

        iOption.Id.ShouldBe(10);
        iOption.Name.ShouldBe("Test");
        iOption.Category.ShouldBe("NotCategorized");
    }

    [Fact]
    public void TypeOptionBase_ImplementsGenericITypeOption()
    {
        var option = new TestTypeOption(11, "Test");
        ITypeOption<TestTypeOption> generic = option;

        generic.Id.ShouldBe(11);
        generic.Name.ShouldBe("Test");
    }
}
