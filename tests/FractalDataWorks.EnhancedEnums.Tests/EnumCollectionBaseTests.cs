using FractalDataWorks.EnhancedEnums;
using Shouldly;
using System.Collections.Immutable;

namespace FractalDataWorks.EnhancedEnums.Tests;

public sealed class EnumCollectionBaseTests
{
    // Test implementation for testing
    private sealed class TestEnumOption : EnumOptionBase<TestEnumOption>
    {
        public static readonly TestEnumOption Empty = new(0, "Empty");
        public static readonly TestEnumOption Option1 = new(1, "Option1");
        public static readonly TestEnumOption Option2 = new(2, "Option2");
        public static readonly TestEnumOption Option3 = new(3, "Option3");

        private TestEnumOption(int id, string name) : base(id, name)
        {
        }
    }

    private sealed class TestEnumCollection : EnumCollectionBase<TestEnumOption>
    {
        static TestEnumCollection()
        {
            // Initialize the collection with test values
            Initialize(ImmutableArray.Create(
                TestEnumOption.Empty,
                TestEnumOption.Option1,
                TestEnumOption.Option2,
                TestEnumOption.Option3
            ));
        }
    }

    [Fact]
    public void All_ShouldReturnAllEnumOptions()
    {
        // Act
        var all = TestEnumCollection.All();

        // Assert
        all.Length.ShouldBe(4);
        all.ShouldContain(TestEnumOption.Empty);
        all.ShouldContain(TestEnumOption.Option1);
        all.ShouldContain(TestEnumOption.Option2);
        all.ShouldContain(TestEnumOption.Option3);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyOption()
    {
        // Act
        var empty = TestEnumCollection.Empty();

        // Assert
        empty.ShouldBe(TestEnumOption.Empty);
        empty.Name.ShouldContain("Empty", Case.Insensitive);
    }

    [Fact]
    public void GetByName_ShouldReturnCorrectOption_WhenNameExists()
    {
        // Act
        var result = TestEnumCollection.GetByName("Option1");

        // Assert
        result.ShouldBe(TestEnumOption.Option1);
    }

    [Fact]
    public void GetByName_ShouldBeCaseInsensitive()
    {
        // Act
        var result1 = TestEnumCollection.GetByName("OPTION1");
        var result2 = TestEnumCollection.GetByName("option1");
        var result3 = TestEnumCollection.GetByName("OpTiOn1");

        // Assert
        result1.ShouldBe(TestEnumOption.Option1);
        result2.ShouldBe(TestEnumOption.Option1);
        result3.ShouldBe(TestEnumOption.Option1);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameIsNull()
    {
        // Act
        var result = TestEnumCollection.GetByName(null);

        // Assert
        result.ShouldBe(TestEnumOption.Empty);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameIsWhiteSpace()
    {
        // Act
        var result = TestEnumCollection.GetByName("   ");

        // Assert
        result.ShouldBe(TestEnumOption.Empty);
    }

    [Fact]
    public void GetByName_ShouldReturnEmpty_WhenNameDoesNotExist()
    {
        // Act
        var result = TestEnumCollection.GetByName("NonExistent");

        // Assert
        result.ShouldBe(TestEnumOption.Empty);
    }

    [Fact]
    public void GetById_ShouldReturnCorrectOption_WhenIdExists()
    {
        // Act
        var result = TestEnumCollection.GetById(1);

        // Assert
        result.ShouldBe(TestEnumOption.Option1);
    }

    [Fact]
    public void GetById_ShouldReturnEmpty_WhenIdDoesNotExist()
    {
        // Act
        var result = TestEnumCollection.GetById(999);

        // Assert
        result.ShouldBe(TestEnumOption.Empty);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void TryGetById_ShouldReturnTrue_WhenIdExists(int id, bool expectedResult)
    {
        // Act
        var result = TestEnumCollection.TryGetById(id, out var value);

        // Assert
        result.ShouldBe(expectedResult);
        value.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetById_ShouldReturnFalse_WhenIdDoesNotExist()
    {
        // Act
        var result = TestEnumCollection.TryGetById(999, out var value);

        // Assert
        result.ShouldBeFalse();
        value.ShouldBe(TestEnumOption.Empty);
    }

    [Theory]
    [InlineData("Empty", true)]
    [InlineData("Option1", true)]
    [InlineData("option2", true)]
    [InlineData("OPTION3", true)]
    public void TryGetByName_ShouldReturnTrue_WhenNameExists(string name, bool expectedResult)
    {
        // Act
        var result = TestEnumCollection.TryGetByName(name, out var value);

        // Assert
        result.ShouldBe(expectedResult);
        value.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetByName_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Act
        var result = TestEnumCollection.TryGetByName("NonExistent", out var value);

        // Assert
        result.ShouldBeFalse();
        value.ShouldBe(TestEnumCollection.Empty());
    }

    [Fact]
    public void AsEnumerable_ShouldReturnAllOptions()
    {
        // Act
        var enumerable = TestEnumCollection.AsEnumerable();

        // Assert
        enumerable.ShouldNotBeNull();
        enumerable.Count().ShouldBe(4);
    }

    [Fact]
    public void Count_ShouldReturnCorrectCount()
    {
        // Act
        var count = TestEnumCollection.Count;

        // Assert
        count.ShouldBe(4);
    }

    [Fact]
    public void Any_ShouldReturnTrue_WhenCollectionHasItems()
    {
        // Act
        var hasAny = TestEnumCollection.Any();

        // Assert
        hasAny.ShouldBeTrue();
    }

    [Fact]
    public void GetByIndex_ShouldReturnCorrectOption()
    {
        // Act
        var option0 = TestEnumCollection.GetByIndex(0);
        var option1 = TestEnumCollection.GetByIndex(1);
        var option2 = TestEnumCollection.GetByIndex(2);
        var option3 = TestEnumCollection.GetByIndex(3);

        // Assert
        option0.ShouldBe(TestEnumOption.Empty);
        option1.ShouldBe(TestEnumOption.Option1);
        option2.ShouldBe(TestEnumOption.Option2);
        option3.ShouldBe(TestEnumOption.Option3);
    }

    [Fact]
    public void GetByIndex_ShouldThrowIndexOutOfRangeException_WhenIndexIsInvalid()
    {
        // Act & Assert
        Should.Throw<IndexOutOfRangeException>(() => TestEnumCollection.GetByIndex(999));
    }

    // Test for empty collection scenario
    private sealed class EmptyTestEnumOption : EnumOptionBase<EmptyTestEnumOption>
    {
        public static readonly EmptyTestEnumOption Default = new(0, "Default");
        private EmptyTestEnumOption(int id, string name) : base(id, name) { }
    }

    private sealed class EmptyTestEnumCollection : EnumCollectionBase<EmptyTestEnumOption>
    {
        static EmptyTestEnumCollection()
        {
            Initialize(ImmutableArray<EmptyTestEnumOption>.Empty);
        }
    }

    [Fact]
    public void Any_ShouldReturnFalse_WhenCollectionIsEmpty()
    {
        // Act
        var hasAny = EmptyTestEnumCollection.Any();

        // Assert
        hasAny.ShouldBeFalse();
    }

    [Fact]
    public void Count_ShouldReturnZero_WhenCollectionIsEmpty()
    {
        // Act
        var count = EmptyTestEnumCollection.Count;

        // Assert
        count.ShouldBe(0);
    }

    // Test for duplicate handling
    private sealed class DuplicateTestEnumOption : EnumOptionBase<DuplicateTestEnumOption>
    {
        public static readonly DuplicateTestEnumOption Option1A = new(1, "Option1");
        public static readonly DuplicateTestEnumOption Option1B = new(1, "Option1"); // Duplicate
        public static readonly DuplicateTestEnumOption Option2 = new(2, "Option2");

        private DuplicateTestEnumOption(int id, string name) : base(id, name) { }
    }

    private sealed class DuplicateTestEnumCollection : EnumCollectionBase<DuplicateTestEnumOption>
    {
        static DuplicateTestEnumCollection()
        {
            // Test "last value wins" semantics for duplicates
            Initialize(ImmutableArray.Create(
                DuplicateTestEnumOption.Option1A,
                DuplicateTestEnumOption.Option1B,
                DuplicateTestEnumOption.Option2
            ));
        }
    }

    [Fact]
    public void GetById_ShouldReturnLastValue_WhenDuplicateIdsExist()
    {
        // Act
        var result = DuplicateTestEnumCollection.GetById(1);

        // Assert - Should return the last one added (Option1B)
        result.ShouldBe(DuplicateTestEnumOption.Option1B);
    }

    [Fact]
    public void GetByName_ShouldReturnLastValue_WhenDuplicateNamesExist()
    {
        // Act
        var result = DuplicateTestEnumCollection.GetByName("Option1");

        // Assert - Should return the last one added (Option1B)
        result.ShouldBe(DuplicateTestEnumOption.Option1B);
    }
}
