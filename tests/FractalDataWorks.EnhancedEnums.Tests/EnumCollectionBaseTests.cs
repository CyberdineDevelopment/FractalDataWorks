using System;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for EnumCollectionBase covering all pathways for 100% coverage.
/// </summary>
public class EnumCollectionBaseTests
{
    public EnumCollectionBaseTests()
    {
        // Trigger static constructor initialization
        _ = TestEnumCollection.IsInitialized;
    }

    public sealed class TestEnum : EnumOptionBase<TestEnum>
    {
        public static readonly TestEnum Empty = new(0, "Empty");
        public static readonly TestEnum Option1 = new(1, "Option1");
        public static readonly TestEnum Option2 = new(2, "Option2");
        public static readonly TestEnum DuplicateName = new(3, "option1"); // Case insensitive duplicate

        private TestEnum(int id, string name) : base(id, name) { }
    }

    public sealed class TestEnumCollection : EnumCollectionBase<TestEnum>
    {
        // Dummy field to ensure static constructor runs
        public static readonly bool IsInitialized;

        static TestEnumCollection()
        {
            var items = ImmutableArray.Create(TestEnum.Empty, TestEnum.Option1, TestEnum.Option2, TestEnum.DuplicateName);
            Initialize(items);
            IsInitialized = true;
        }
    }

    [Fact]
    public void All_ReturnsAllEnumOptions()
    {
        var all = TestEnumCollection.All();

        all.Length.ShouldBe(4);
        all.ShouldContain(TestEnum.Empty);
        all.ShouldContain(TestEnum.Option1);
        all.ShouldContain(TestEnum.Option2);
        all.ShouldContain(TestEnum.DuplicateName);
    }

    [Fact]
    public void Empty_ReturnsEmptyOption()
    {
        var empty = TestEnumCollection.Empty();

        empty.ShouldBe(TestEnum.Empty);
        empty.Name.ShouldBe("Empty");
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        TestEnumCollection.Count.ShouldBe(4);
    }

    [Fact]
    public void Any_ReturnsTrue_WhenCollectionHasItems()
    {
        TestEnumCollection.Any().ShouldBeTrue();
    }

    [Fact]
    public void AsEnumerable_ReturnsAllItems()
    {
        var items = TestEnumCollection.AsEnumerable().ToList();

        items.Count.ShouldBe(4);
    }

    [Fact]
    public void GetByName_ReturnsCorrectOption()
    {
        var result = TestEnumCollection.GetByName("Option1");

        result.ShouldBe(TestEnum.DuplicateName); // Last value wins with duplicates
    }

    [Fact]
    public void GetByName_IsCaseInsensitive()
    {
        var result = TestEnumCollection.GetByName("OPTION2");

        result.ShouldBe(TestEnum.Option2);
    }

    [Fact]
    public void GetByName_WithNull_ReturnsEmpty()
    {
        var result = TestEnumCollection.GetByName(null);

        result.ShouldBe(TestEnum.Empty);
    }

    [Fact]
    public void GetByName_WithWhitespace_ReturnsEmpty()
    {
        var result = TestEnumCollection.GetByName("   ");

        result.ShouldBe(TestEnum.Empty);
    }

    [Fact]
    public void GetByName_WithNonExistent_ReturnsEmpty()
    {
        var result = TestEnumCollection.GetByName("NonExistent");

        result.ShouldBe(TestEnum.Empty);
    }

    [Fact]
    public void GetById_ReturnsCorrectOption()
    {
        var result = TestEnumCollection.GetById(2);

        result.ShouldBe(TestEnum.Option2);
    }

    [Fact]
    public void GetById_WithNonExistent_ReturnsEmpty()
    {
        var result = TestEnumCollection.GetById(999);

        result.ShouldBe(TestEnum.Empty);
    }

    [Fact]
    public void TryGetByName_ReturnsTrue_WhenFound()
    {
        var found = TestEnumCollection.TryGetByName("Option2", out var value);

        found.ShouldBeTrue();
        value.ShouldBe(TestEnum.Option2);
    }

    [Fact]
    public void TryGetById_ReturnsTrue_WhenFound()
    {
        var found = TestEnumCollection.TryGetById(1, out var value);

        found.ShouldBeTrue();
        value.ShouldBe(TestEnum.Option1);
    }

    [Fact]
    public void GetByIndex_ReturnsCorrectOption()
    {
        var result = TestEnumCollection.GetByIndex(1);

        result.ShouldBe(TestEnum.Option1);
    }

    [Fact]
    public void GetByIndex_ThrowsForInvalidIndex()
    {
        Should.Throw<IndexOutOfRangeException>(() => TestEnumCollection.GetByIndex(999));
    }
}
