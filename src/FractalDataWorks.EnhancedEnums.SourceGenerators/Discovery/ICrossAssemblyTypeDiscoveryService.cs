using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Discovery;

/// <summary>
/// Interface for discovering types across assembly boundaries.
/// </summary>
/// <remarks>
/// This interface is part of source generator infrastructure that runs at compile-time.
/// </remarks>
public interface ICrossAssemblyTypeDiscoveryService
{
    /// <summary>
    /// Checks if an assembly wants to be discovered by the current compilation.
    /// </summary>
    /// <param name="assemblyToCheck">The assembly to check if it wants to be discovered.</param>
    /// <param name="currentAssemblyName">The name of the current assembly doing the discovery.</param>
    /// <returns>True if the assembly wants to be discovered by the current assembly.</returns>
    bool IsAssemblyDiscoverable(IAssemblySymbol assemblyToCheck, string currentAssemblyName);
    
    /// <summary>
    /// Gets the list of assembly names that should be included in cross-assembly discovery.
    /// This is now deprecated - use IsAssemblyDiscoverable instead.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of assembly names to include, or null/empty to include all referenced assemblies.</returns>
    IEnumerable<string> GetIncludedAssemblies(Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that derive from the specified base type.
    /// </summary>
    /// <param name="baseType">The base type to search for derived types.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that derive from the base type.</returns>
    IEnumerable<INamedTypeSymbol> FindDerivedTypes(INamedTypeSymbol baseType, Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that have the specified attribute.
    /// </summary>
    /// <param name="attributeType">The attribute type to search for.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that have the specified attribute.</returns>
    IEnumerable<INamedTypeSymbol> FindTypesWithAttribute(INamedTypeSymbol attributeType, Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that have an attribute with the specified name.
    /// </summary>
    /// <param name="attributeName">The name of the attribute to search for (e.g., "EnumOption" for [EnumOption]).</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that have the specified attribute.</returns>
    IEnumerable<INamedTypeSymbol> FindTypesWithAttributeName(string attributeName, Compilation compilation);
}
