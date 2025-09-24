# DataStoreTypes.Database

Database DataStore type implementations for the TypeCollection demo.

This package provides concrete database DataStore types that are automatically discovered by the TypeCollection source generator.

## Types Included

- **MySqlDataStoreType** - MySQL database connections
- **PostgreSqlDataStoreType** - PostgreSQL database connections
- **SqlServerDataStoreType** - SQL Server database connections

## Usage

Reference this package alongside DataStores.Abstractions to automatically include these database types in the generated DataStoreTypes collection.