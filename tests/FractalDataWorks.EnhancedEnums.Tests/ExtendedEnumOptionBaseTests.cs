using System;
using FractalDataWorks.EnhancedEnums.ExtendedEnums;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for ExtendedEnumOptionBase.
/// </summary>
public class ExtendedEnumOptionBaseTests
{
    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 3
    }

    public sealed class TestExtendedEnum : ExtendedEnumOptionBase<TestExtendedEnum, TestEnum>
    {
        public TestExtendedEnum(TestEnum enumValue) : base(enumValue) { }
    }

    [Fact]
    public void Constructor_InitializesEnumValue()
    {
        var option = new TestExtendedEnum(TestEnum.First);

        option.EnumValue.ShouldBe(TestEnum.First);
    }

    [Fact]
    public void Id_ReturnsUnderlyingEnumIntValue()
    {
        var option = new TestExtendedEnum(TestEnum.Second);

        option.Id.ShouldBe(2);
    }

    [Fact]
    public void Name_ReturnsEnumValueAsString()
    {
        var option = new TestExtendedEnum(TestEnum.Third);

        option.Name.ShouldBe("Third");
    }

    [Theory]
    [InlineData(TestEnum.None, 0, "None")]
    [InlineData(TestEnum.First, 1, "First")]
    [InlineData(TestEnum.Second, 2, "Second")]
    [InlineData(TestEnum.Third, 3, "Third")]
    public void Properties_ReturnCorrectValues(TestEnum enumValue, int expectedId, string expectedName)
    {
        var option = new TestExtendedEnum(enumValue);

        option.Id.ShouldBe(expectedId);
        option.Name.ShouldBe(expectedName);
        option.EnumValue.ShouldBe(enumValue);
    }

    [Fact]
    public void ImplicitConversion_ToEnum_ReturnsUnderlyingValue()
    {
        var option = new TestExtendedEnum(TestEnum.Second);

        TestEnum result = option;

        result.ShouldBe(TestEnum.Second);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        var option = new TestExtendedEnum(TestEnum.First);

        var result = option.ToString();

        result.ShouldBe("First");
    }

    [Fact]
    public void Equals_WithSameEnumValue_ReturnsTrue()
    {
        var option1 = new TestExtendedEnum(TestEnum.Second);
        var option2 = new TestExtendedEnum(TestEnum.Second);

        var result = option1.Equals(option2);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEnumValue_ReturnsFalse()
    {
        var option1 = new TestExtendedEnum(TestEnum.First);
        var option2 = new TestExtendedEnum(TestEnum.Second);

        var result = option1.Equals(option2);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithUnderlyingEnum_ReturnsTrue()
    {
        var option = new TestExtendedEnum(TestEnum.Third);

        var result = option.Equals(TestEnum.Third);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentUnderlyingEnum_ReturnsFalse()
    {
        var option = new TestExtendedEnum(TestEnum.First);

        var result = option.Equals(TestEnum.Second);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var option = new TestExtendedEnum(TestEnum.First);

        var result = option.Equals(null);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var option = new TestExtendedEnum(TestEnum.First);

        var result = option.Equals("First");

        result.ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_ForSameEnumValue_ReturnsSameHashCode()
    {
        var option1 = new TestExtendedEnum(TestEnum.Second);
        var option2 = new TestExtendedEnum(TestEnum.Second);

        var hash1 = option1.GetHashCode();
        var hash2 = option2.GetHashCode();

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCode_MatchesUnderlyingEnumHashCode()
    {
        var option = new TestExtendedEnum(TestEnum.Third);

        var optionHash = option.GetHashCode();
        var enumHash = TestEnum.Third.GetHashCode();

        optionHash.ShouldBe(enumHash);
    }
}
