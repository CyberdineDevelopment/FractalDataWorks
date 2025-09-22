namespace FractalDataWorks.ServiceTypes.SourceGenerators.Generators;

/// <summary>
/// Data structure for efficient ServiceType collection information without ISymbol references.
/// </summary>
internal readonly record struct ServiceTypeCollectionInfo(
    string CollectionClassName,
    string CollectionNamespace,
    string BaseTypeName,
    string GeneratedCollectionName);