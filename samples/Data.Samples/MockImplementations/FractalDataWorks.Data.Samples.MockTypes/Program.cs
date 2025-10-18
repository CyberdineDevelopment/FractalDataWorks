using System;
using System.Collections.Generic;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Sample program demonstrating TypeCollection pattern for data architecture.
/// Shows how TypeCollections provide automatic registration and discovery of type metadata.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("=== FractalDataWorks Data Architecture - TypeCollection Patterns Demo ===\n");
        System.Console.WriteLine("This demo shows BOTH TypeCollection patterns:\n");
        System.Console.WriteLine("1. Single-class pattern (Converters) - metadata + implementation in one class");
        System.Console.WriteLine("2. Type/Implementation split (Paths, Containers) - separate metadata and implementation\n");

        // Demonstrate DataTypeConverterTypes TypeCollection (single-class pattern)
        DemonstrateConverterTypes();
        System.Console.WriteLine();

        // Demonstrate PathTypes and ContainerTypes (Type/Implementation split pattern)
        DemonstratePathsAndContainers();
        System.Console.WriteLine();

        System.Console.WriteLine("=== Demo Complete ===");
        System.Console.WriteLine("\nBoth patterns are correct for their use cases:");
        System.Console.WriteLine("- Converters: Single-class (they need implementation logic for Convert/ConvertBack)");
        System.Console.WriteLine("- Paths/Containers: Type/Implementation split (pure metadata types)");
    }


    static void DemonstrateConverterTypes()
    {
        System.Console.WriteLine("--- DataTypeConverters TypeCollection ---");

        // Test direct instantiation of converter
        var mockConverter = new MockSqlInt32Converter();
        System.Console.WriteLine($"Direct instantiation test:");
        System.Console.WriteLine($"  Name: {mockConverter.Name}");
        System.Console.WriteLine($"  Source Type: {mockConverter.SourceTypeName}");
        System.Console.WriteLine($"  Target CLR Type: {mockConverter.TargetClrType.Name}");
        System.Console.WriteLine();

        // Test conversion
        System.Console.WriteLine("Testing type conversion:");
        var result = mockConverter.Convert(42);
        System.Console.WriteLine($"  mockConverter.Convert(42) = {result} (type: {result?.GetType().Name})");

        var nullResult = mockConverter.Convert(DBNull.Value);
        System.Console.WriteLine($"  mockConverter.Convert(DBNull.Value) = {(nullResult == null ? "null" : nullResult)}");
        System.Console.WriteLine();

        // Check TypeCollection registration
        System.Console.WriteLine($"Total Converters Registered in TypeCollection: {DataTypeConverters.All().Count}");

        if (DataTypeConverters.All().Count > 0)
        {
            System.Console.WriteLine("\nDiscovered Converters:");
            foreach (var converter in DataTypeConverters.All())
            {
                var converterBase = (DataTypeConverterBase)converter;
                System.Console.WriteLine($"  Name: {converterBase.Name}");
                System.Console.WriteLine($"      Source Type: {converter.SourceTypeName}");
                System.Console.WriteLine($"      Target CLR Type: {converter.TargetClrType.Name}");
                System.Console.WriteLine();
            }
        }
        else
        {
            System.Console.WriteLine("  (0 converters found - this assembly has MockSqlInt32Converter)");
            System.Console.WriteLine("  Note: Source generator runs at compile-time with RestrictToCurrentCompilation=false");
            System.Console.WriteLine("  Converters should be discovered across all referenced assemblies.");
        }

        // Demo: Single-class pattern - converter contains metadata AND implementation
        System.Console.WriteLine("Single-class pattern: converter contains both metadata AND implementation!");
    }

    static void DemonstratePathsAndContainers()
    {
        System.Console.WriteLine("--- PathTypes and ContainerTypes TypeCollections ---");
        System.Console.WriteLine("These use the Type/Implementation split pattern:\n");

        // Demonstrate the split pattern with Path example from MsSql sample
        System.Console.WriteLine("Example: SQL Database Path");
        System.Console.WriteLine("  PathType (metadata): SqlDatabasePathType - describes what a SQL path is");
        System.Console.WriteLine("  Path (implementation): DatabasePath - actual runtime path instance");
        System.Console.WriteLine();

        // Check what's registered
        System.Console.WriteLine($"Total Path Types Registered: {PathTypes.All().Count}");
        System.Console.WriteLine($"Total Container Types Registered: {ContainerTypes.All().Count}");

        if (PathTypes.All().Count > 0)
        {
            System.Console.WriteLine("\nRegistered Path Types:");
            foreach (var pathType in PathTypes.All())
            {
                System.Console.WriteLine($"  - {pathType.Name} (Domain: {pathType.Domain})");
            }
        }

        if (ContainerTypes.All().Count > 0)
        {
            System.Console.WriteLine("\nRegistered Container Types:");
            foreach (var containerType in ContainerTypes.All())
            {
                System.Console.WriteLine($"  - {containerType.Name}");
            }
        }

        if (PathTypes.All().Count == 0 && ContainerTypes.All().Count == 0)
        {
            System.Console.WriteLine("  (Note: Cross-assembly discovery - types found via reflection at runtime)");
        }

        System.Console.WriteLine("\nType/Implementation split pattern: Metadata types describe capabilities,");
        System.Console.WriteLine("while separate implementation classes handle runtime behavior.");
    }

}

// NOTE: This sample demonstrates both TypeCollection patterns:
//
// 1. SINGLE-CLASS PATTERN (Converters):
//    - One class contains both metadata AND implementation
//    - Used when types have behavior (Convert/ConvertBack methods)
//    - Example: MockSqlInt32Converter
//
// 2. TYPE/IMPLEMENTATION SPLIT (Paths, Containers):
//    - Separate Type class (metadata) and implementation class (runtime behavior)
//    - Used for pure metadata-driven types
//    - Example: SqlDatabasePathType (metadata) + DatabasePath (implementation)
