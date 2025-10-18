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
        System.Console.WriteLine($"Total Converter Types Registered: {DataTypeConverterTypes.All().Count}");

        foreach (var converter in DataTypeConverterTypes.All())
        {
            System.Console.WriteLine($"  Name: {converter.Name}");
            System.Console.WriteLine($"      Source Type: {converter.SourceTypeName}");
            System.Console.WriteLine($"      Target CLR Type: {converter.TargetClrType.Name}");
            System.Console.WriteLine();
        }

        // Demo: Single-class pattern - converter contains metadata AND implementation
        System.Console.WriteLine("These are actual converters - you can call Convert() directly!");
    }

}

// NOTE: Mock converter implementations (like MockSqlInt32Converter) are discovered
// by the TypeCollectionGenerator during compilation. The single-class pattern means
// each converter is both metadata and implementation - no separate "Type" class needed.
