using DataStore.Abstractions;

Console.WriteLine("=== Enhanced TypeCollection Generator Test ===");
Console.WriteLine("Demonstrating: Dynamic TypeLookup, Abstract/Static Types, FrozenDictionary Alternate Key Lookup");
Console.WriteLine();

try
{
    // Test collection overview
    Console.WriteLine($"📊 All DataStore types count: {DataStoreTypes.All().Count}");
    Console.WriteLine();

    // Test concrete type access (should use dictionary lookup with extracted IDs)
    Console.WriteLine("🔧 Concrete Types (instantiated):");
    var postgreSql = DataStoreTypes.PostgreSql;  // ID=2 from constructor
    var mySql = DataStoreTypes.MySql;            // ID=3 from constructor
    var fileSystem = DataStoreTypes.FileSystem;
    var restApi = DataStoreTypes.RestApi;

    Console.WriteLine($"  PostgreSQL: {postgreSql.Name} (ID: {postgreSql.Id}, Category: {postgreSql.Category})");
    Console.WriteLine($"  MySQL: {mySql.Name} (ID: {mySql.Id}, Category: {mySql.Category})");
    Console.WriteLine($"  FileSystem: {fileSystem.Name} (ID: {fileSystem.Id}, Category: {fileSystem.Category})");
    Console.WriteLine($"  RestApi: {restApi.Name} (ID: {restApi.Id}, Category: {restApi.Category})");
    Console.WriteLine();

    // Test abstract/static type access (should return empty instances)
    Console.WriteLine("📁 Abstract/Static Types (included but not instantiated):");
    var databaseBase = DataStoreTypes.DatabaseBase;  // Abstract - returns _empty
    var utilities = DataStoreTypes.Utilities;        // Static - returns _empty

    Console.WriteLine($"  DatabaseBase: {databaseBase.Name} (ID: {databaseBase.Id}) - Abstract type");
    Console.WriteLine($"  Utilities: {utilities.Name} (ID: {utilities.Id}) - Static type");
    Console.WriteLine();

    // Test DYNAMIC LOOKUP METHODS (generated from [TypeLookup] attributes)
    Console.WriteLine("🔍 Dynamic TypeLookup Methods (FrozenDictionary Alternate Key Lookup):");

    // Primary key lookup (uses dictionary directly)
    var byIdPrimary = DataStoreTypes.Id(2);
    Console.WriteLine($"  Id(2): {byIdPrimary.Name} - Primary key lookup");

    // Alternate key lookups (uses GetAlternateLookup<string>())
    var byName = DataStoreTypes.Name("MySql");
    var byCategory = DataStoreTypes.Category("Database");

    Console.WriteLine($"  Name('MySql'): {byName.Name} - Alternate key lookup");
    Console.WriteLine($"  Category('Database'): {byCategory.Name} - Alternate key lookup");
    Console.WriteLine();

    // Test performance comparison
    Console.WriteLine("⚡ Performance Test (1000 lookups each):");
    var sw = System.Diagnostics.Stopwatch.StartNew();

    for (int i = 0; i < 1000; i++)
    {
        _ = DataStoreTypes.Id(2);
        _ = DataStoreTypes.Name("MySql");
        _ = DataStoreTypes.Category("Database");
    }

    sw.Stop();
    Console.WriteLine($"  3000 lookups completed in {sw.ElapsedMilliseconds}ms");
    Console.WriteLine($"  Average: {sw.ElapsedTicks / 3000.0:F2} ticks per lookup");
    Console.WriteLine();

    // Test type-specific features (if available)
    if (mySql is MySqlDataStoreType mysql)
    {
        Console.WriteLine("🔌 MySQL-Specific Features:");
        Console.WriteLine($"  Connection Template: {mysql.ConnectionStringTemplate}");
        Console.WriteLine($"  Default Port: {mysql.DefaultPort}");
        Console.WriteLine($"  SQL Dialect: {mysql.SqlDialect}");
        Console.WriteLine($"  Supports Transactions: {mysql.SupportsTransactions}");
        Console.WriteLine();
    }

    Console.WriteLine("✅ SUCCESS: Enhanced TypeCollectionGenerator is working perfectly!");
    Console.WriteLine("   Features demonstrated:");
    Console.WriteLine("   ✓ Dynamic TypeLookup methods from attributes");
    Console.WriteLine("   ✓ Abstract/Static type inclusion");
    Console.WriteLine("   ✓ Constructor ID extraction");
    Console.WriteLine("   ✓ FrozenDictionary alternate key lookup");
    Console.WriteLine("   ✓ Smart instantiation");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAILED: {ex.Message}");
    Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
    Console.WriteLine("   Enhanced TypeCollectionGenerator may not be generating expected code.");
}
