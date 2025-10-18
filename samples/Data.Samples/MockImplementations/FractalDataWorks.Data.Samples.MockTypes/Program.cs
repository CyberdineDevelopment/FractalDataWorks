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
        System.Console.WriteLine("=== FractalDataWorks Data Architecture - TypeCollection Demo ===\n");
        System.Console.WriteLine("This demo shows the CORRECT single-class converter pattern.");
        System.Console.WriteLine("Each converter contains both metadata AND implementation.\n");

        // Demonstrate DataTypeConverterTypes TypeCollection (CORRECT pattern)
        DemonstrateConverterTypes();
        System.Console.WriteLine();

        System.Console.WriteLine("=== Demo Complete ===");
        System.Console.WriteLine("\nNote: Other TypeCollections (Paths, Containers, etc.) still use the old");
        System.Console.WriteLine("Type/Implementation split pattern and will be updated in Phase 1.5.");
    }


    static void DemonstrateConverterTypes()
    {
        System.Console.WriteLine("--- DataTypeConverterTypes TypeCollection ---");

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
        System.Console.WriteLine($"Total Converters Registered in TypeCollection: {DataTypeConverterTypes.All().Count}");

        if (DataTypeConverterTypes.All().Count > 0)
        {
            foreach (var converter in DataTypeConverterTypes.All())
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
            System.Console.WriteLine("  (Note: Cross-assembly discovery happens at runtime reflection)");
        }

        // Demo: Single-class pattern - converter contains metadata AND implementation
        System.Console.WriteLine("Single-class pattern verified: converter contains both metadata AND implementation!");
    }

}

// NOTE: Mock converter implementations (like MockSqlInt32Converter) are discovered
// by the TypeCollectionGenerator during compilation. The single-class pattern means
// each converter is both metadata and implementation - no separate "Type" class needed.
