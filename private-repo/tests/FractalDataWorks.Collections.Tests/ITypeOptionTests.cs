using FractalDataWorks.Collections;

namespace FractalDataWorks.Collections.Tests;

public class ITypeOptionTests
{
    private class TestTypeOption : ITypeOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }

    private class TestGenericTypeOption : ITypeOption<TestGenericTypeOption>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }

    [Fact]
    public void ITypeOption_CanBeImplemented()
    {
        var option = new TestTypeOption { Id = 1, Name = "Test", Category = "TestCategory" };

        option.Id.ShouldBe(1);
        option.Name.ShouldBe("Test");
        option.Category.ShouldBe("TestCategory");
    }

    [Fact]
    public void ITypeOption_Generic_CanBeImplemented()
    {
        var option = new TestGenericTypeOption { Id = 2, Name = "Generic", Category = "Category" };

        option.Id.ShouldBe(2);
        option.Name.ShouldBe("Generic");
        option.Category.ShouldBe("Category");
    }

    [Fact]
    public void ITypeOption_Generic_InheritsFromNonGeneric()
    {
        var typed = new TestGenericTypeOption { Id = 3, Name = "Test", Category = "Cat" };
        ITypeOption nonGeneric = typed;

        nonGeneric.Id.ShouldBe(3);
        nonGeneric.Name.ShouldBe("Test");
        nonGeneric.Category.ShouldBe("Cat");
    }
}
