using System;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Data;

/// <summary>
/// Base class for entities with GUID primary keys.
/// </summary>
/// <ExcludeFromTest>Simple base entity class with no business logic to test</ExcludeFromTest>
// Excluded from code coverage: Simple base entity class with no business logic
[ExcludeFromCodeCoverage]
public abstract class GuidEntityBase : EntityBase<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuidEntityBase"/> class.
    /// </summary>
    protected GuidEntityBase()
    {
        Id = Guid.NewGuid();
    }
}
