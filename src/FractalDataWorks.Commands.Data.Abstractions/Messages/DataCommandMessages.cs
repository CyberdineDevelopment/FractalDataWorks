using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Collection base for data command messages.
/// Generates static factory methods in DataCommandMessages class.
/// </summary>
[MessageCollection("DataCommandMessages")]
public abstract class DataCommandMessageCollectionBase : MessageCollectionBase<DataCommandMessage>
{
}
