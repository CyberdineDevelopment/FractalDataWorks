using FractalDataWorks.Data.DataStores.Abstractions;

namespace FractalDataWorks.Data.DataStores.FileSystem;

/// <summary>
/// DataStore type definition for file system storage backend.
/// </summary>
public sealed class FileSystemDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDataStoreType"/> class.
    /// </summary>
    public FileSystemDataStoreType() : base(
        id: 3,
        name: "FileSystem",
        displayName: "File System",
        description: "File system data store supporting CSV, JSON, XML, and binary files",
        supportsRead: true,
        supportsWrite: true,
        supportsTransactions: false,
        category: "File")
    {
    }
}
