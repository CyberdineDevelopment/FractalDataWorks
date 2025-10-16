using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Collections.Tests.Attributes;

public class TypeOptionAttributeTests
{
    private class TestCollection { }

    [Fact]
    public void Constructor_SetsCollectionTypeAndName()
    {
        var attribute = new TypeOptionAttribute(typeof(TestCollection), "TestName");

        attribute.CollectionType.ShouldBe(typeof(TestCollection));
        attribute.Name.ShouldBe("TestName");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCollectionTypeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeOptionAttribute(null!, "Name"))
            .ParamName.ShouldBe("collectionType");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeOptionAttribute(typeof(TestCollection), null!))
            .ParamName.ShouldBe("name");
    }

    [Fact]
    public void AttributeUsage_AllowsSingleInstance()
    {
        var usage = typeof(TypeOptionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void AttributeUsage_TargetsClass()
    {
        var usage = typeof(TypeOptionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.ValidOn.ShouldBe(AttributeTargets.Class);
    }
}
