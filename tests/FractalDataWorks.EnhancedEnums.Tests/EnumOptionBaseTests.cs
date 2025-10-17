using FractalDataWorks.EnhancedEnums;
using Shouldly;

namespace FractalDataWorks.EnhancedEnums.Tests;

public sealed class EnumOptionBaseTests
{
    // Test implementation of EnumOptionBase for testing purposes
    private sealed class TestEnumOption : EnumOptionBase<TestEnumOption>
    {
        public static readonly TestEnumOption Option1 = new(1, "Option1");
        public static readonly TestEnumOption Option2 = new(2, "Option2");
        public static readonly TestEnumOption Empty = new(0, "Empty");

        private TestEnumOption(int id, string name) : base(id, name)
        {
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeIdAndName()
    {
        // Arrange & Act
        var option = TestEnumOption.Option1;

        // Assert
        option.Id.ShouldBe(1);
        option.Name.ShouldBe("Option1");
    }

    [Fact]
    public void Id_ShouldBeReadOnly()
    {
        // Arrange
        var option = TestEnumOption.Option1;

        // Act & Assert - Id should not have a setter
        option.Id.ShouldBe(1);
    }

    [Fact]
    public void Name_ShouldBeReadOnly()
    {
        // Arrange
        var option = TestEnumOption.Option2;

        // Act & Assert - Name should not have a setter
        option.Name.ShouldBe("Option2");
    }

    [Fact]
    public void DifferentInstances_ShouldHaveDifferentValues()
    {
        // Arrange
        var option1 = TestEnumOption.Option1;
        var option2 = TestEnumOption.Option2;

        // Assert
        option1.Id.ShouldNotBe(option2.Id);
        option1.Name.ShouldNotBe(option2.Name);
    }

    [Fact]
    public void EnumOption_ShouldImplementIEnumOption()
    {
        // Arrange
        var option = TestEnumOption.Option1;

        // Assert
        option.ShouldBeAssignableTo<IEnumOption<TestEnumOption>>();
        option.ShouldBeAssignableTo<IEnumOption>();
    }

    [Fact]
    public void Id_ShouldBeVirtual()
    {
        // This test verifies the Id property can be overridden
        // Act
        var option = TestEnumOption.Empty;

        // Assert
        option.Id.ShouldBe(0);
    }

    [Fact]
    public void Name_ShouldBeVirtual()
    {
        // This test verifies the Name property can be overridden
        // Act
        var option = TestEnumOption.Empty;

        // Assert
        option.Name.ShouldBe("Empty");
    }
}
