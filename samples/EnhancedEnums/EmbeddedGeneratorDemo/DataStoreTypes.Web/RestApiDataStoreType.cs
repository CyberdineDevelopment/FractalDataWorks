using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Web;

/// <summary>
/// REST API DataStore type implementation
/// </summary>
[TypeOption("RestApi")]
public sealed class RestApiDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestApiDataStoreType"/> class.
    /// </summary>
    public RestApiDataStoreType() : base(8, "RestApi", "Web")
    {
    }
}