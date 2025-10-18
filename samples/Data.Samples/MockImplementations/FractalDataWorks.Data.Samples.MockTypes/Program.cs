using System;
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

        // Demonstrate PathTypes TypeCollection
        DemonstratePathTypes();
        System.Console.WriteLine();

        // Demonstrate ContainerTypes TypeCollection
        DemonstrateContainerTypes();
        System.Console.WriteLine();

        // Demonstrate TranslatorTypes TypeCollection
        DemonstrateTranslatorTypes();
        System.Console.WriteLine();

        // Demonstrate FormatTypes TypeCollection
        DemonstrateFormatTypes();
        System.Console.WriteLine();

        // Demonstrate DataTypeConverterTypes TypeCollection
        DemonstrateConverterTypes();
        System.Console.WriteLine();

        // Demonstrate DataTransformerTypes TypeCollection
        DemonstrateTransformerTypes();
        System.Console.WriteLine();

        System.Console.WriteLine("=== Demo Complete ===");
    }

    static void DemonstratePathTypes()
    {
        System.Console.WriteLine("--- PathTypes TypeCollection ---");
        System.Console.WriteLine($"Total Path Types Registered: {PathTypes.All().Count}");

        foreach (var pathType in PathTypes.All())
        {
            System.Console.WriteLine($"  [{pathType.Id}] {pathType.Name}");
            System.Console.WriteLine($"      Display Name: {pathType.DisplayName}");
            System.Console.WriteLine($"      Description: {pathType.Description}");
            System.Console.WriteLine($"      Domain: {pathType.Domain}");
            System.Console.WriteLine($"      Category: {pathType.Category}");
            System.Console.WriteLine();
        }

        // Lookup by name - Note: actual method names may vary, check generated code
        // Demo: Type collection provides automatic registration and lookup
    }

    static void DemonstrateContainerTypes()
    {
        System.Console.WriteLine("--- ContainerTypes TypeCollection ---");
        System.Console.WriteLine($"Total Container Types Registered: {ContainerTypes.All().Count}");

        foreach (var containerType in ContainerTypes.All())
        {
            System.Console.WriteLine($"  [{containerType.Id}] {containerType.Name}");
            System.Console.WriteLine($"      Display Name: {containerType.DisplayName}");
            System.Console.WriteLine($"      Description: {containerType.Description}");
            System.Console.WriteLine($"      Supports Schema Discovery: {containerType.SupportsSchemaDiscovery}");
            System.Console.WriteLine($"      Category: {containerType.Category}");
            System.Console.WriteLine();
        }
    }

    static void DemonstrateTranslatorTypes()
    {
        System.Console.WriteLine("--- TranslatorTypes TypeCollection ---");
        System.Console.WriteLine($"Total Translator Types Registered: {TranslatorTypes.All().Count}");

        foreach (var translatorType in TranslatorTypes.All())
        {
            System.Console.WriteLine($"  [{translatorType.Id}] {translatorType.Name}");
            System.Console.WriteLine($"      Display Name: {translatorType.DisplayName}");
            System.Console.WriteLine($"      Description: {translatorType.Description}");
            System.Console.WriteLine($"      Domain: {translatorType.Domain}");
            System.Console.WriteLine($"      Category: {translatorType.Category}");
            System.Console.WriteLine();
        }
    }

    static void DemonstrateFormatTypes()
    {
        System.Console.WriteLine("--- FormatTypes TypeCollection ---");
        System.Console.WriteLine($"Total Format Types Registered: {FormatTypes.All().Count}");

        foreach (var formatType in FormatTypes.All())
        {
            System.Console.WriteLine($"  [{formatType.Id}] {formatType.Name}");
            System.Console.WriteLine($"      Display Name: {formatType.DisplayName}");
            System.Console.WriteLine($"      Description: {formatType.Description}");
            System.Console.WriteLine($"      MIME Type: {formatType.MimeType}");
            System.Console.WriteLine($"      Is Binary: {formatType.IsBinary}");
            System.Console.WriteLine($"      Supports Streaming: {formatType.SupportsStreaming}");
            System.Console.WriteLine($"      Category: {formatType.Category}");
            System.Console.WriteLine();
        }
    }

    static void DemonstrateConverterTypes()
    {
        System.Console.WriteLine("--- DataTypeConverterTypes TypeCollection ---");
        System.Console.WriteLine($"Total Converter Types Registered: {DataTypeConverterTypes.All().Count}");

        foreach (var converterType in DataTypeConverterTypes.All())
        {
            System.Console.WriteLine($"  [{converterType.Id}] {converterType.Name}");
            System.Console.WriteLine($"      Display Name: {converterType.DisplayName}");
            System.Console.WriteLine($"      Description: {converterType.Description}");
            System.Console.WriteLine($"      Source Type: {converterType.SourceTypeName}");
            System.Console.WriteLine($"      Target CLR Type: {converterType.TargetClrType.Name}");
            System.Console.WriteLine($"      Category: {converterType.Category}");
            System.Console.WriteLine();
        }
    }

    static void DemonstrateTransformerTypes()
    {
        System.Console.WriteLine("--- DataTransformerTypes TypeCollection ---");
        System.Console.WriteLine($"Total Transformer Types Registered: {DataTransformerTypes.All().Count}");

        foreach (var transformerType in DataTransformerTypes.All())
        {
            System.Console.WriteLine($"  [{transformerType.Id}] {transformerType.Name}");
            System.Console.WriteLine($"      Display Name: {transformerType.DisplayName}");
            System.Console.WriteLine($"      Description: {transformerType.Description}");
            System.Console.WriteLine($"      Supports Streaming: {transformerType.SupportsStreaming}");
            System.Console.WriteLine($"      Category: {transformerType.Category}");
            System.Console.WriteLine();
        }
    }
}
