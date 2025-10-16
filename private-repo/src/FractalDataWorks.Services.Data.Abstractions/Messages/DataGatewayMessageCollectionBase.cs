using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Data.Abstractions.Messages;

/// <summary>
/// Collection base for data gateway messages.
/// Generates static factory methods in DataGatewayMessages class.
/// </summary>
[MessageCollection("DataGatewayMessages")]
public abstract class DataGatewayMessageCollectionBase : MessageCollectionBase<DataGatewayMessage>
{
}
