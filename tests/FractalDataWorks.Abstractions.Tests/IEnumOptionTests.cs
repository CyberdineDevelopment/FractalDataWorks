using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Abstractions.Tests;

public class IEnumOptionTests
{
    private class TestEnumOption : IEnumOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    private class TestGenericEnumOption : IEnumOption<TestGenericEnumOption>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public void IEnumOption_CanBeImplemented()
    {
        var option = new TestEnumOption { Id = 1, Name = "Test" };

        option.Id.ShouldBe(1);
        option.Name.ShouldBe("Test");
    }

    [Fact]
    public void IEnumOption_Id_CanBeSet()
    {
        var option = new TestEnumOption { Id = 42 };

        option.Id.ShouldBe(42);
    }

    [Fact]
    public void IEnumOption_Name_CanBeSet()
    {
        var option = new TestEnumOption { Name = "TestName" };

        option.Name.ShouldBe("TestName");
    }

    [Fact]
    public void IEnumOption_Generic_CanBeImplemented()
    {
        var option = new TestGenericEnumOption { Id = 1, Name = "Generic" };

        option.Id.ShouldBe(1);
        option.Name.ShouldBe("Generic");
    }

    [Fact]
    public void IEnumOption_Generic_InheritsFromNonGeneric()
    {
        var option = new TestGenericEnumOption { Id = 2, Name = "Test" };
        IEnumOption nonGeneric = option;

        nonGeneric.Id.ShouldBe(2);
        nonGeneric.Name.ShouldBe("Test");
    }
}
