using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// Placeholder enum for SchemaCompatibilityMode
/// </summary>
public enum SchemaCompatibilityMode
{
    /// <summary>
    /// Basic compatibility check
    /// </summary>
    Basic,

    /// <summary>
    /// Strict compatibility check
    /// </summary>
    Strict
}