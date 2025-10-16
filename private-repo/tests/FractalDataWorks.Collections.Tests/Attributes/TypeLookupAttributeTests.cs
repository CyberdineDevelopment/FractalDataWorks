using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Collections.Tests.Attributes;

public class TypeLookupAttributeTests
{
    [Fact]
    public void Constructor_MinimalParameters_SetsDefaults()
    {
        var attribute = new TypeLookupAttribute("TestMethod");

        attribute.MethodName.ShouldBe("TestMethod");
        attribute.ReturnsList.ShouldBeFalse();
        attribute.ReturnType.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithReturnsList_SetsProperty()
    {
        var attribute = new TypeLookupAttribute("TestMethod", returnsList: true);

        attribute.MethodName.ShouldBe("TestMethod");
        attribute.ReturnsList.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithReturnType_SetsProperty()
    {
        var attribute = new TypeLookupAttribute("TestMethod", returnType: typeof(string));

        attribute.MethodName.ShouldBe("TestMethod");
        attribute.ReturnType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        var attribute = new TypeLookupAttribute("TestMethod", returnsList: true, returnType: typeof(int));

        attribute.MethodName.ShouldBe("TestMethod");
        attribute.ReturnsList.ShouldBeTrue();
        attribute.ReturnType.ShouldBe(typeof(int));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMethodNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeLookupAttribute(null!))
            .ParamName.ShouldBe("methodName");
    }

    [Fact]
    public void AttributeUsage_AllowsSingleInstance()
    {
        var usage = typeof(TypeLookupAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void AttributeUsage_TargetsProperty()
    {
        var usage = typeof(TypeLookupAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.ValidOn.ShouldBe(AttributeTargets.Property);
    }
}
