using Shouldly;
using System.Reflection;

namespace FractalDataWorks.Results.Abstractions.Tests;

/// <summary>
/// Tests for FractalDataWorks.Results.Abstractions assembly.
/// Note: If the assembly only contains interfaces or is empty, it may be excluded from coverage.
/// </summary>
public sealed class AssemblyTests
{
    [Fact]
    public void Assembly_ShouldLoad()
    {
        // Act
        var assembly = Assembly.Load("FractalDataWorks.Results.Abstractions");

        // Assert
        assembly.ShouldNotBeNull();
        assembly.GetName().Name.ShouldBe("FractalDataWorks.Results.Abstractions");
    }
}
