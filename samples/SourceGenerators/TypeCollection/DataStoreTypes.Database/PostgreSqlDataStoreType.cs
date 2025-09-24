using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Database;

/// <summary>
/// PostgreSQL DataStore type implementation.
/// Demonstrates concrete type with ID=2 for static property lookup.
/// </summary>
[TypeOption("PostgreSql")]
public sealed class PostgreSqlDataStoreType : DatabaseDataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataStoreType"/> class.
    /// The base constructor call provides ID=2 which will be extracted for static property lookup.
    /// </summary>
    public PostgreSqlDataStoreType() : base(2, "PostgreSql")
    {
    }

    public override string ConnectionStringTemplate =>
        "Host={host};Port={port};Database={database};Username={username};Password={password};";

    public override int DefaultPort => 5432;

    public override string SqlDialect => "PostgreSQL";
}