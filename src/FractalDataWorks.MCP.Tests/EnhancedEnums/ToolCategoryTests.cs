using System;
using System.Linq;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.MCP.Tests.EnhancedEnums;

/// <summary>
/// Comprehensive tests for ToolCategoryBase and all tool category implementations.
/// </summary>
[ExcludeFromCodeCoverage]
public class ToolCategoryBaseTests
{
    [Fact]
    public void ToolCategoryBase_InheritsFromEnhancedEnumBase()
    {
        // Arrange & Act
        var categoryType = typeof(ToolCategoryBase);

        // Assert
        categoryType.IsAbstract.ShouldBeTrue();
        typeof(EnhancedEnumBase<ToolCategoryBase>).IsAssignableFrom(categoryType).ShouldBeTrue();
        TestContext.Current.WriteLine("ToolCategoryBase correctly inherits from EnhancedEnumBase<ToolCategoryBase>");
    }

    [Fact]
    public void DescriptionProperty_ReturnsExpectedValue()
    {
        // Arrange
        var testCategory = SessionManagement.Instance;

        // Act
        var description = testCategory.Description;

        // Assert
        description.ShouldNotBeNull();
        description.ShouldNotBeEmpty();
        description.ShouldBe("Tools for managing compilation sessions and workspace state");
        TestContext.Current.WriteLine($"SessionManagement description: {description}");
    }

    [Fact]
    public void DisplayPriority_DefaultImplementation_ReturnsId()
    {
        // Arrange
        var testCategory = CodeAnalysis.Instance;

        // Act
        var displayPriority = testCategory.DisplayPriority;

        // Assert
        displayPriority.ShouldBe(testCategory.Id);
        displayPriority.ShouldBe(2); // CodeAnalysis has Id = 2
        TestContext.Current.WriteLine($"CodeAnalysis DisplayPriority: {displayPriority} (equals Id: {testCategory.Id})");
    }

    [Fact]
    public void DisplayPriority_OverriddenImplementation_ReturnsDifferentValue()
    {
        // Arrange
        var serverManagement = ServerManagement.Instance;

        // Act
        var displayPriority = serverManagement.DisplayPriority;

        // Assert
        displayPriority.ShouldNotBe(serverManagement.Id);
        displayPriority.ShouldBe(100); // Overridden to show last in UI
        TestContext.Current.WriteLine($"ServerManagement DisplayPriority: {displayPriority} (Id: {serverManagement.Id})");
    }
}

/// <summary>
/// Tests for SessionManagement tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class SessionManagementTests
{
    [Fact]
    public void SessionManagement_IsSingleton()
    {
        // Arrange & Act
        var instance1 = SessionManagement.Instance;
        var instance2 = SessionManagement.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("SessionManagement.Instance is a singleton");
    }

    [Fact]
    public void SessionManagement_HasCorrectProperties()
    {
        // Arrange & Act
        var category = SessionManagement.Instance;

        // Assert
        category.Id.ShouldBe(1);
        category.Name.ShouldBe("SessionManagement");
        category.Description.ShouldBe("Tools for managing compilation sessions and workspace state");
        category.DisplayPriority.ShouldBe(1);
        TestContext.Current.WriteLine($"SessionManagement properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void SessionManagement_IsSealed()
    {
        // Arrange & Act
        var type = typeof(SessionManagement);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("SessionManagement class is sealed");
    }

    [Fact]
    public void SessionManagement_HasEnumOptionAttribute()
    {
        // Arrange & Act
        var type = typeof(SessionManagement);
        var attribute = type.GetCustomAttributes(typeof(FractalDataWorks.EnhancedEnums.Attributes.EnumOptionAttribute), false)
                           .FirstOrDefault();

        // Assert
        attribute.ShouldNotBeNull();
        TestContext.Current.WriteLine("SessionManagement has EnumOption attribute");
    }
}

/// <summary>
/// Tests for CodeAnalysis tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class CodeAnalysisTests
{
    [Fact]
    public void CodeAnalysis_IsSingleton()
    {
        // Arrange & Act
        var instance1 = CodeAnalysis.Instance;
        var instance2 = CodeAnalysis.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("CodeAnalysis.Instance is a singleton");
    }

    [Fact]
    public void CodeAnalysis_HasCorrectProperties()
    {
        // Arrange & Act
        var category = CodeAnalysis.Instance;

        // Assert
        category.Id.ShouldBe(2);
        category.Name.ShouldBe("CodeAnalysis");
        category.Description.ShouldBe("Tools for analyzing code quality, diagnostics, and compiler errors");
        category.DisplayPriority.ShouldBe(2);
        TestContext.Current.WriteLine($"CodeAnalysis properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void CodeAnalysis_IsSealed()
    {
        // Arrange & Act
        var type = typeof(CodeAnalysis);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("CodeAnalysis class is sealed");
    }
}

/// <summary>
/// Tests for VirtualEditing tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class VirtualEditingTests
{
    [Fact]
    public void VirtualEditing_IsSingleton()
    {
        // Arrange & Act
        var instance1 = VirtualEditing.Instance;
        var instance2 = VirtualEditing.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("VirtualEditing.Instance is a singleton");
    }

    [Fact]
    public void VirtualEditing_HasCorrectProperties()
    {
        // Arrange & Act
        var category = VirtualEditing.Instance;

        // Assert
        category.Id.ShouldBe(3);
        category.Name.ShouldBe("VirtualEditing");
        category.Description.ShouldBe("Tools for preview editing with rollback capabilities");
        category.DisplayPriority.ShouldBe(3);
        TestContext.Current.WriteLine($"VirtualEditing properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void VirtualEditing_IsSealed()
    {
        // Arrange & Act
        var type = typeof(VirtualEditing);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("VirtualEditing class is sealed");
    }
}

/// <summary>
/// Tests for Refactoring tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class RefactoringTests
{
    [Fact]
    public void Refactoring_IsSingleton()
    {
        // Arrange & Act
        var instance1 = Refactoring.Instance;
        var instance2 = Refactoring.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("Refactoring.Instance is a singleton");
    }

    [Fact]
    public void Refactoring_HasCorrectProperties()
    {
        // Arrange & Act
        var category = Refactoring.Instance;

        // Assert
        category.Id.ShouldBe(4);
        category.Name.ShouldBe("Refactoring");
        category.Description.ShouldBe("Tools for code refactoring and reorganization");
        category.DisplayPriority.ShouldBe(4);
        TestContext.Current.WriteLine($"Refactoring properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void Refactoring_IsSealed()
    {
        // Arrange & Act
        var type = typeof(Refactoring);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("Refactoring class is sealed");
    }
}

/// <summary>
/// Tests for TypeAnalysis tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class TypeAnalysisTests
{
    [Fact]
    public void TypeAnalysis_IsSingleton()
    {
        // Arrange & Act
        var instance1 = TypeAnalysis.Instance;
        var instance2 = TypeAnalysis.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("TypeAnalysis.Instance is a singleton");
    }

    [Fact]
    public void TypeAnalysis_HasCorrectProperties()
    {
        // Arrange & Act
        var category = TypeAnalysis.Instance;

        // Assert
        category.Id.ShouldBe(5);
        category.Name.ShouldBe("TypeAnalysis");
        category.Description.ShouldBe("Tools for type discovery, resolution, and analysis");
        category.DisplayPriority.ShouldBe(5);
        TestContext.Current.WriteLine($"TypeAnalysis properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void TypeAnalysis_IsSealed()
    {
        // Arrange & Act
        var type = typeof(TypeAnalysis);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("TypeAnalysis class is sealed");
    }
}

/// <summary>
/// Tests for ProjectDependencies tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProjectDependenciesTests
{
    [Fact]
    public void ProjectDependencies_IsSingleton()
    {
        // Arrange & Act
        var instance1 = ProjectDependencies.Instance;
        var instance2 = ProjectDependencies.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("ProjectDependencies.Instance is a singleton");
    }

    [Fact]
    public void ProjectDependencies_HasCorrectProperties()
    {
        // Arrange & Act
        var category = ProjectDependencies.Instance;

        // Assert
        category.Id.ShouldBe(6);
        category.Name.ShouldBe("ProjectDependencies");
        category.Description.ShouldBe("Tools for analyzing project references and dependencies");
        category.DisplayPriority.ShouldBe(6);
        TestContext.Current.WriteLine($"ProjectDependencies properties - Id: {category.Id}, Name: {category.Name}");
    }

    [Fact]
    public void ProjectDependencies_IsSealed()
    {
        // Arrange & Act
        var type = typeof(ProjectDependencies);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("ProjectDependencies class is sealed");
    }
}

/// <summary>
/// Tests for ServerManagement tool category.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServerManagementTests
{
    [Fact]
    public void ServerManagement_IsSingleton()
    {
        // Arrange & Act
        var instance1 = ServerManagement.Instance;
        var instance2 = ServerManagement.Instance;

        // Assert
        instance1.ShouldBeSameAs(instance2);
        TestContext.Current.WriteLine("ServerManagement.Instance is a singleton");
    }

    [Fact]
    public void ServerManagement_HasCorrectProperties()
    {
        // Arrange & Act
        var category = ServerManagement.Instance;

        // Assert
        category.Id.ShouldBe(7);
        category.Name.ShouldBe("ServerManagement");
        category.Description.ShouldBe("Tools for managing the MCP server lifecycle");
        category.DisplayPriority.ShouldBe(100); // Overridden to show last
        TestContext.Current.WriteLine($"ServerManagement properties - Id: {category.Id}, Name: {category.Name}, DisplayPriority: {category.DisplayPriority}");
    }

    [Fact]
    public void ServerManagement_HasOverriddenDisplayPriority()
    {
        // Arrange & Act
        var category = ServerManagement.Instance;

        // Assert
        category.DisplayPriority.ShouldBe(100);
        category.DisplayPriority.ShouldNotBe(category.Id);
        TestContext.Current.WriteLine($"ServerManagement DisplayPriority {category.DisplayPriority} is overridden from Id {category.Id}");
    }

    [Fact]
    public void ServerManagement_IsSealed()
    {
        // Arrange & Act
        var type = typeof(ServerManagement);

        // Assert
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("ServerManagement class is sealed");
    }
}

/// <summary>
/// Tests for the source-generated ToolCategories collection.
/// </summary>
[ExcludeFromCodeCoverage]
public class ToolCategoriesCollectionTests
{
    [Fact]
    public void ToolCategories_IsStaticPartialClass()
    {
        // Arrange & Act
        var type = typeof(ToolCategories);

        // Assert
        type.IsClass.ShouldBeTrue();
        type.IsAbstract.ShouldBeTrue();
        type.IsSealed.ShouldBeTrue();
        TestContext.Current.WriteLine("ToolCategories is a static partial class");
    }

    [Fact]
    public void ToolCategories_AllMethod_ReturnsAllCategories()
    {
        // Arrange & Act
        var allCategories = ToolCategories.All;

        // Assert
        allCategories.ShouldNotBeNull();
        allCategories.Count.ShouldBe(7); // All 7 defined categories

        // Verify all expected categories are present
        allCategories.ShouldContain(SessionManagement.Instance);
        allCategories.ShouldContain(CodeAnalysis.Instance);
        allCategories.ShouldContain(VirtualEditing.Instance);
        allCategories.ShouldContain(Refactoring.Instance);
        allCategories.ShouldContain(TypeAnalysis.Instance);
        allCategories.ShouldContain(ProjectDependencies.Instance);
        allCategories.ShouldContain(ServerManagement.Instance);

        TestContext.Current.WriteLine($"ToolCategories.All returned {allCategories.Count} categories");
    }

    [Theory]
    [InlineData(1, typeof(SessionManagement))]
    [InlineData(2, typeof(CodeAnalysis))]
    [InlineData(3, typeof(VirtualEditing))]
    [InlineData(4, typeof(Refactoring))]
    [InlineData(5, typeof(TypeAnalysis))]
    [InlineData(6, typeof(ProjectDependencies))]
    [InlineData(7, typeof(ServerManagement))]
    public void ToolCategories_ByIdMethod_ReturnsCorrectCategory(int id, Type expectedType)
    {
        // Arrange & Act
        var category = ToolCategories.ById(id);

        // Assert
        category.ShouldNotBeNull();
        category.ShouldBeOfType(expectedType);
        category.Id.ShouldBe(id);
        TestContext.Current.WriteLine($"ToolCategories.ById({id}) returned {category.Name} of type {expectedType.Name}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToolCategories_ByIdMethod_WithInvalidId_ReturnsNull(int invalidId)
    {
        // Arrange & Act
        var category = ToolCategories.ById(invalidId);

        // Assert
        category.ShouldBeNull();
        TestContext.Current.WriteLine($"ToolCategories.ById({invalidId}) correctly returned null");
    }

    [Theory]
    [InlineData("SessionManagement", typeof(SessionManagement))]
    [InlineData("CodeAnalysis", typeof(CodeAnalysis))]
    [InlineData("VirtualEditing", typeof(VirtualEditing))]
    [InlineData("Refactoring", typeof(Refactoring))]
    [InlineData("TypeAnalysis", typeof(TypeAnalysis))]
    [InlineData("ProjectDependencies", typeof(ProjectDependencies))]
    [InlineData("ServerManagement", typeof(ServerManagement))]
    public void ToolCategories_ByNameMethod_ReturnsCorrectCategory(string name, Type expectedType)
    {
        // Arrange & Act
        var category = ToolCategories.ByName(name);

        // Assert
        category.ShouldNotBeNull();
        category.ShouldBeOfType(expectedType);
        category.Name.ShouldBe(name);
        TestContext.Current.WriteLine($"ToolCategories.ByName('{name}') returned {category.Name} of type {expectedType.Name}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("NonExistentCategory")]
    [InlineData("sessionmanagement")] // Case sensitive
    [InlineData("SESSIONMANAGEMENT")] // Case sensitive
    public void ToolCategories_ByNameMethod_WithInvalidName_ReturnsNull(string invalidName)
    {
        // Arrange & Act
        var category = ToolCategories.ByName(invalidName);

        // Assert
        category.ShouldBeNull();
        TestContext.Current.WriteLine($"ToolCategories.ByName('{invalidName}') correctly returned null");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void ToolCategories_TryGetByIdMethod_WithValidId_ReturnsTrue(int id)
    {
        // Arrange & Act
        var found = ToolCategories.TryGetById(id, out var category);

        // Assert
        found.ShouldBeTrue();
        category.ShouldNotBeNull();
        category.Id.ShouldBe(id);
        TestContext.Current.WriteLine($"ToolCategories.TryGetById({id}) returned true with category {category.Name}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void ToolCategories_TryGetByIdMethod_WithInvalidId_ReturnsFalse(int invalidId)
    {
        // Arrange & Act
        var found = ToolCategories.TryGetById(invalidId, out var category);

        // Assert
        found.ShouldBeFalse();
        category.ShouldBeNull();
        TestContext.Current.WriteLine($"ToolCategories.TryGetById({invalidId}) correctly returned false");
    }

    [Theory]
    [InlineData("SessionManagement")]
    [InlineData("CodeAnalysis")]
    [InlineData("VirtualEditing")]
    [InlineData("Refactoring")]
    [InlineData("TypeAnalysis")]
    [InlineData("ProjectDependencies")]
    [InlineData("ServerManagement")]
    public void ToolCategories_TryGetByNameMethod_WithValidName_ReturnsTrue(string name)
    {
        // Arrange & Act
        var found = ToolCategories.TryGetByName(name, out var category);

        // Assert
        found.ShouldBeTrue();
        category.ShouldNotBeNull();
        category.Name.ShouldBe(name);
        TestContext.Current.WriteLine($"ToolCategories.TryGetByName('{name}') returned true with category {category.Name}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("NonExistent")]
    [InlineData("sessionmanagement")] // Case sensitive
    public void ToolCategories_TryGetByNameMethod_WithInvalidName_ReturnsFalse(string invalidName)
    {
        // Arrange & Act
        var found = ToolCategories.TryGetByName(invalidName, out var category);

        // Assert
        found.ShouldBeFalse();
        category.ShouldBeNull();
        TestContext.Current.WriteLine($"ToolCategories.TryGetByName('{invalidName}') correctly returned false");
    }

    [Fact]
    public void ToolCategories_OrderedByDisplayPriority_ReturnsCorrectOrder()
    {
        // Arrange & Act
        var orderedCategories = ToolCategories.All.OrderBy(c => c.DisplayPriority).ToList();

        // Assert
        orderedCategories.Count.ShouldBe(7);

        // Categories 1-6 should be in order of their IDs (DisplayPriority equals Id)
        for (int i = 0; i < 6; i++)
        {
            orderedCategories[i].DisplayPriority.ShouldBe(i + 1);
        }

        // ServerManagement should be last with DisplayPriority 100
        orderedCategories[6].ShouldBe(ServerManagement.Instance);
        orderedCategories[6].DisplayPriority.ShouldBe(100);

        TestContext.Current.WriteLine($"Categories ordered by DisplayPriority: {string.Join(", ", orderedCategories.Select(c => $"{c.Name}({c.DisplayPriority})"))}");
    }

    [Fact]
    public void ToolCategories_AllHaveUniqueIds()
    {
        // Arrange & Act
        var allCategories = ToolCategories.All;
        var uniqueIds = allCategories.Select(c => c.Id).Distinct().ToList();

        // Assert
        uniqueIds.Count.ShouldBe(allCategories.Count);
        TestContext.Current.WriteLine($"All {allCategories.Count} categories have unique IDs");
    }

    [Fact]
    public void ToolCategories_AllHaveUniqueNames()
    {
        // Arrange & Act
        var allCategories = ToolCategories.All;
        var uniqueNames = allCategories.Select(c => c.Name).Distinct().ToList();

        // Assert
        uniqueNames.Count.ShouldBe(allCategories.Count);
        TestContext.Current.WriteLine($"All {allCategories.Count} categories have unique names");
    }

    [Fact]
    public void ToolCategories_AllHaveNonEmptyDescriptions()
    {
        // Arrange & Act
        var allCategories = ToolCategories.All;

        // Assert
        allCategories.ShouldAllBe(c => !string.IsNullOrWhiteSpace(c.Description));
        TestContext.Current.WriteLine("All categories have non-empty descriptions");
    }
}