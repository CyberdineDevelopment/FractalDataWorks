using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.Abstractions.Messages;

/// <summary>
/// Collection definition to generate DataMessages static class.
/// </summary>
[MessageCollection("DataMessages", ReturnType = typeof(IDataMessage))]
public abstract class DataMessageCollectionBase : MessageCollectionBase<DataMessage>
{
}
