using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.File;

/// <summary>
/// File System DataStore type implementation
/// </summary>
[TypeOption(typeof(DataStoreTypes), "FileSystem")]
public sealed class FileSystemDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDataStoreType"/> class.
    /// </summary>
    public FileSystemDataStoreType() : base(6, "FileSystem", "File")
    {
    }
}