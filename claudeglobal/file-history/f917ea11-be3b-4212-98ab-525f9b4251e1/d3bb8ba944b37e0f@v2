using System;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for EnumOptionBase.
/// </summary>
public class EnumOptionBaseTests
{
    private sealed class TestEnumOption : EnumOptionBase<TestEnumOption>
    {
        public TestEnumOption(int id, string name) : base(id, name) { }
    }

    [Fact]
    public void Constructor_InitializesIdAndName()
    {
        var option = new TestEnumOption(42, "TestOption");

        option.Id.ShouldBe(42);
        option.Name.ShouldBe("TestOption");
    }

    [Theory]
    [InlineData(0, "Zero")]
    [InlineData(1, "One")]
    [InlineData(-1, "Negative")]
    [InlineData(int.MaxValue, "Max")]
    public void Constructor_WithVariousValues_InitializesCorrectly(int id, string name)
    {
        var option = new TestEnumOption(id, name);

        option.Id.ShouldBe(id);
        option.Name.ShouldBe(name);
    }
}
