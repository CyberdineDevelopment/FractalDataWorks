using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.DataStores.Abstractions.Messages;

/// <summary>
/// Base class for collections of data store messages.
/// </summary>
[MessageCollection("DataStoreMessages")]
public abstract class DataStoreMessageCollectionBase : MessageCollectionBase<DataStoreMessage>
{
}
