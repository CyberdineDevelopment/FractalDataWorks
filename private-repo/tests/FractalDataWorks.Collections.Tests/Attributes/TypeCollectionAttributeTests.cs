using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Collections.Tests.Attributes;

public class TypeCollectionAttributeTests
{
    private class TestBase { }
    private class TestReturn { }
    private class TestCollection { }

    [Fact]
    public void Constructor_SetsAllRequiredProperties()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.BaseType.ShouldBe(typeof(TestBase));
        attribute.DefaultReturnType.ShouldBe(typeof(TestReturn));
        attribute.CollectionType.ShouldBe(typeof(TestCollection));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBaseTypeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeCollectionAttribute(
            null!,
            typeof(TestReturn),
            typeof(TestCollection)))
            .ParamName.ShouldBe("baseType");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDefaultReturnTypeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeCollectionAttribute(
            typeof(TestBase),
            null!,
            typeof(TestCollection)))
            .ParamName.ShouldBe("defaultReturnType");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCollectionTypeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            null!))
            .ParamName.ShouldBe("collectionType");
    }

    [Fact]
    public void BaseTypeName_ReturnsFullName()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.BaseTypeName.ShouldBe(typeof(TestBase).FullName);
    }

    [Fact]
    public void CollectionName_ReturnsCollectionTypeName()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.CollectionName.ShouldBe(typeof(TestCollection).Name);
    }

    [Fact]
    public void UseSingletonInstances_DefaultsToFalse()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.UseSingletonInstances.ShouldBeFalse();
    }

    [Fact]
    public void UseSingletonInstances_CanBeSet()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection))
        {
            UseSingletonInstances = true
        };

        attribute.UseSingletonInstances.ShouldBeTrue();
    }

    [Fact]
    public void UseMethods_DefaultsToFalse()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.UseMethods.ShouldBeFalse();
    }

    [Fact]
    public void UseMethods_CanBeSet()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection))
        {
            UseMethods = true
        };

        attribute.UseMethods.ShouldBeTrue();
    }

    [Fact]
    public void RestrictToCurrentCompilation_DefaultsToFalse()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection));

        attribute.RestrictToCurrentCompilation.ShouldBeFalse();
    }

    [Fact]
    public void RestrictToCurrentCompilation_CanBeSet()
    {
        var attribute = new TypeCollectionAttribute(
            typeof(TestBase),
            typeof(TestReturn),
            typeof(TestCollection))
        {
            RestrictToCurrentCompilation = true
        };

        attribute.RestrictToCurrentCompilation.ShouldBeTrue();
    }

    [Fact]
    public void AttributeUsage_AllowsSingleInstance()
    {
        var usage = typeof(TypeCollectionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void AttributeUsage_IsNotInherited()
    {
        var usage = typeof(TypeCollectionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>().FirstOrDefault();

        usage.ShouldNotBeNull();
        usage.Inherited.ShouldBeFalse();
    }
}
