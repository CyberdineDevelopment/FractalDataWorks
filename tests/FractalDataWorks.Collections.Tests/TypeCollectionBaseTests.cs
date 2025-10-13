using FractalDataWorks.Collections;

namespace FractalDataWorks.Collections.Tests;

public class TypeCollectionBaseTests
{
    private class TestType
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestCollection : TypeCollectionBase<TestType>
    {
    }

    private class TestGenericCollection : TypeCollectionBase<TestType, object>
    {
    }

    [Fact]
    public void TypeCollectionBase_CanBeInstantiated()
    {
        var collection = new TestCollection();

        collection.ShouldNotBeNull();
    }

    [Fact]
    public void TypeCollectionBase_Generic_CanBeInstantiated()
    {
        var collection = new TestGenericCollection();

        collection.ShouldNotBeNull();
    }

    [Fact]
    public void TypeCollectionBase_IsAbstract()
    {
        typeof(TypeCollectionBase<TestType>).IsAbstract.ShouldBeTrue();
    }

    [Fact]
    public void TypeCollectionBase_Generic_IsAbstract()
    {
        typeof(TypeCollectionBase<TestType, object>).IsAbstract.ShouldBeTrue();
    }
}
