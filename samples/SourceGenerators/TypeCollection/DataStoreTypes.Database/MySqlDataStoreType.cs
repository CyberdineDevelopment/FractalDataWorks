using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Database;

/// <summary>
/// MySQL DataStore type implementation.
/// Demonstrates concrete type with constructor ID extraction.
/// </summary>
[TypeOption("MySql")]
public sealed class MySqlDataStoreType : DatabaseDataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDataStoreType"/> class.
    /// The base constructor call provides ID=3 which will be extracted for static property lookup.
    /// </summary>
    public MySqlDataStoreType() : base(3, "MySql")
    {
    }

    public override string ConnectionStringTemplate =>
        "Server={server};Port={port};Database={database};Uid={username};Pwd={password};";

    public override int DefaultPort => 3306;

    public override string SqlDialect => "MySQL";
}