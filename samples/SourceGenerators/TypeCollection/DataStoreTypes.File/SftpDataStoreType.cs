using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.File;

/// <summary>
/// SFTP DataStore type implementation
/// </summary>
[TypeOption(typeof(DataStoreTypes), "Sftp")]
public sealed class SftpDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SftpDataStoreType"/> class.
    /// </summary>
    public SftpDataStoreType() : base(7, "Sftp", "File")
    {
    }
}