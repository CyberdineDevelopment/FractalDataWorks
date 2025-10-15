using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using Shouldly;
using System;

namespace FractalDataWorks.EnhancedEnums.Tests;

public sealed class EnumCollectionBaseTests
{
    // Constructor to force static initialization once
    public EnumCollectionBaseTests()
    {
        _ = nameof(TestEnumOptions);
    }

    // Test enum following Pattern 1: Abstract base with concrete options
    public abstract class TestEnumOption : EnumOptionBase<TestEnumOption>
    {
        protected TestEnumOption(int id, string name) : base(id, name) { }
        public abstract int Level { get; }
    }

    [EnumOption(typeof(TestEnumOptions), "Empty")]
    public sealed class EmptyTestOption : TestEnumOption
    {
        public EmptyTestOption() : base(0, "Empty") { }
        public override int Level => 0;
    }

    [EnumOption(typeof(TestEnumOptions), "Option1")]
    public sealed class Option1 : TestEnumOption
    {
        public Option1() : base(1, "Option1") { }
        public override int Level => 10;
    }

    [EnumOption(typeof(TestEnumOptions), "Option2")]
    public sealed class Option2 : TestEnumOption
    {
        public Option2() : base(2, "Option2") { }
        public override int Level => 20;
    }

    [EnumOption(typeof(TestEnumOptions), "Option3")]
    public sealed class Option3 : TestEnumOption
    {
        public Option3() : base(3, "Option3") { }
        public override int Level => 30;
    }

    // Manually implemented collection for testing (not using source generator)
    public sealed class TestEnumOptions : EnumCollectionBase<TestEnumOption>
    {
        private static readonly TestEnumOption[] _all =
        [
            new EmptyTestOption(),
            new Option1(),
            new Option2(),
            new Option3()
        ];

        public static TestEnumOption[] All() => _all;
        public static TestEnumOption Empty() => _all[0];
        public static TestEnumOption GetByName(string name) =>
            Array.Find(_all, o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase)) ?? Empty();
        public static TestEnumOption GetById(int id) =>
            Array.Find(_all, o => o.Id == id) ?? Empty();
        public static int Count() => _all.Length;
        public static bool Any() => _all.Length > 0;
        public static TestEnumOption GetByIndex(int index) =>
            index >= 0 && index < _all.Length ? _all[index] : Empty();
        public static bool TryGetByName(string name, out TestEnumOption option)
        {
            option = Array.Find(_all, o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase))!;
            return option != null;
        }
        public static bool TryGetById(int id, out TestEnumOption option)
        {
            option = Array.Find(_all, o => o.Id == id)!;
            return option != null;
        }
    }

    [Fact]
    public void All_ShouldReturnAllEnumOptions()
    {
        // Act
        var all = TestEnumOptions.All();

        // Assert
        all.Length.ShouldBe(4);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyOption()
    {
        // Act
        var empty = TestEnumOptions.Empty();

        // Assert
        empty.ShouldNotBeNull();
        empty.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Fact]
    public void GetByName_ShouldReturnCorrectOption_WhenNameExists()
    {
        // Act
        var result = TestEnumOptions.GetByName("Option1");

        // Assert
        result.ShouldNotBeNull();
        result.Level.ShouldBe(10);
    }

    [Fact]
    public void GetByName_ShouldBeCaseInsensitive()
    {
        // Act
        var result1 = TestEnumOptions.GetByName("OPTION1");
        var result2 = TestEnumOptions.GetByName("option1");
        var result3 = TestEnumOptions.GetByName("OpTiOn1");

        // Assert
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result3.ShouldNotBeNull();
        result1.Level.ShouldBe(10);
        result2.Level.ShouldBe(10);
        result3.Level.ShouldBe(10);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameIsNull()
    {
        // Act
        var result = TestEnumOptions.GetByName(null);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameIsWhiteSpace()
    {
        // Act
        var result = TestEnumOptions.GetByName("   ");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameDoesNotExist()
    {
        // Act
        var result = TestEnumOptions.GetByName("NonExistent");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Fact]
    public void GetById_ShouldReturnCorrectOption_WhenIdExists()
    {
        // Act
        var result = TestEnumOptions.GetById(1);

        // Assert
        result.ShouldNotBeNull();
        result.Level.ShouldBe(10);
    }

    [Fact]
    public void GetById_ShouldReturnEmpty_WhenIdDoesNotExist()
    {
        // Act
        var result = TestEnumOptions.GetById(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void TryGetById_ShouldReturnTrue_WhenIdExists(int id, bool expectedResult)
    {
        // Act
        var result = TestEnumOptions.TryGetById(id, out var value);

        // Assert
        result.ShouldBe(expectedResult);
        value.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetById_ShouldReturnFalse_WhenIdDoesNotExist()
    {
        // Act
        var result = TestEnumOptions.TryGetById(999, out var value);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Empty", true)]
    [InlineData("Option1", true)]
    [InlineData("option2", true)]
    [InlineData("OPTION3", true)]
    public void TryGetByName_ShouldReturnTrue_WhenNameExists(string name, bool expectedResult)
    {
        // Act
        var result = TestEnumOptions.TryGetByName(name, out var value);

        // Assert
        result.ShouldBe(expectedResult);
        value.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetByName_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Act
        var result = TestEnumOptions.TryGetByName("NonExistent", out var value);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AsEnumerable_ShouldReturnAllOptions()
    {
        // Act
        var enumerable = TestEnumOptions.AsEnumerable();

        // Assert
        enumerable.ShouldNotBeNull();
        enumerable.Count().ShouldBe(4);
    }

    [Fact]
    public void Count_ShouldReturnCorrectCount()
    {
        // Act
        var count = TestEnumOptions.Count;

        // Assert
        count.ShouldBe(4);
    }

    [Fact]
    public void Any_ShouldReturnTrue_WhenCollectionHasItems()
    {
        // Act
        var hasAny = TestEnumOptions.Any();

        // Assert
        hasAny.ShouldBeTrue();
    }

    [Fact]
    public void GetByIndex_ShouldReturnCorrectOption()
    {
        // Act
        var option0 = TestEnumOptions.GetByIndex(0);
        var option1 = TestEnumOptions.GetByIndex(1);
        var option2 = TestEnumOptions.GetByIndex(2);
        var option3 = TestEnumOptions.GetByIndex(3);

        // Assert
        option0.ShouldNotBeNull();
        option1.ShouldNotBeNull();
        option2.ShouldNotBeNull();
        option3.ShouldNotBeNull();
    }

    [Fact]
    public void GetByIndex_ShouldThrowIndexOutOfRangeException_WhenIndexIsInvalid()
    {
        // Act & Assert
        Should.Throw<IndexOutOfRangeException>(() => TestEnumOptions.GetByIndex(999));
    }
}
