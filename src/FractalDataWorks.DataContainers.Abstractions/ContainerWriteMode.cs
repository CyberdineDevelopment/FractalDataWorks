using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.DataStores.Abstractions;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Specifies how data should be written to a container.
/// </summary>
public enum ContainerWriteMode
{
    /// <summary>
    /// Overwrite any existing data completely.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Append new data to existing data.
    /// </summary>
    Append,

    /// <summary>
    /// Create new container, fail if it already exists.
    /// </summary>
    CreateNew,

    /// <summary>
    /// Update existing records based on key fields.
    /// </summary>
    Upsert
}