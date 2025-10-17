using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Services.Builders;

/// <summary>
/// Defines the contract for building enhanced enum collections using the Gang of Four Builder pattern.
/// This interface provides a fluent API for configuring and constructing collection classes
/// from enum type definitions and values during source generation.
/// </summary>
public interface IMessageCollectionBuilder
{

    /// <summary>
    /// Sets the enum type definition that contains metadata about the enum type to be processed.
    /// This includes namespace, class name, base type information, and generation preferences.
    /// </summary>
    /// <param name="definition">The enum type definition containing metadata for code generation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    IMessageCollectionBuilder WithDefinition(CollectionTypeInfoModel definition);

    /// <summary>
    /// Sets the collection of enum values to include in the generated collection.
    /// These values will be used to create static fields, factory methods, and lookup functionality.
    /// </summary>
    /// <param name="values">The list of enum values to include in the collection.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    IMessageCollectionBuilder WithValues(IList<CollectionValueInfoModel> values);

    /// <summary>
    /// Sets the return type for generated collection methods and properties.
    /// This type will be used as the return type for All(), ByName(), and other collection methods.
    /// </summary>
    /// <param name="returnType">The fully qualified or simplified return type name.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="returnType"/> is null or empty.</exception>
    IMessageCollectionBuilder WithReturnType(string returnType);

    /// <summary>
    /// Sets the compilation context for type symbol resolution and semantic analysis.
    /// This is required for resolving type references, implementing interfaces, and
    /// performing compile-time validation during source generation.
    /// </summary>
    /// <param name="compilation">The compilation context from the source generator.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compilation"/> is null.</exception>
    IMessageCollectionBuilder WithCompilation(Compilation compilation);

    /// <summary>
    /// Builds and returns the complete source code for the enum collection.
    /// The returned string contains all the necessary methods, properties, and fields
    /// for the enhanced enum collection based on the specified configuration.
    /// </summary>
    /// <returns>The complete source code for the enum collection class.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required configuration is missing or when the builder is in an invalid state.
    /// </exception>
    string Build();
}
