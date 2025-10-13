using Shouldly;
using System.Reflection;

namespace FractalDataWorks.DependencyInjection.Tests;

/// <summary>
/// Tests for FractalDataWorks.DependencyInjection assembly.
/// Note: The assembly is excluded from code coverage as it only contains placeholder/assembly-level attributes.
/// </summary>
public sealed class AssemblyTests
{
    [Fact]
    public void Assembly_ShouldLoad()
    {
        // Act
        var assembly = Assembly.Load("FractalDataWorks.DependencyInjection");

        // Assert - This project exists to hold assembly-level attributes
        assembly.ShouldNotBeNull();
        assembly.GetName().Name.ShouldBe("FractalDataWorks.DependencyInjection");
    }
}
