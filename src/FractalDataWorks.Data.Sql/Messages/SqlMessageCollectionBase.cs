using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.Sql.Messages;

/// <summary>
/// Collection definition to generate SqlMessages static class.
/// </summary>
[MessageCollection("SqlMessages")]
public abstract class SqlMessageCollectionBase : MessageCollectionBase<SqlMessage>
{
}
