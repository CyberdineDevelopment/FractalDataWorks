# FractalDataWorks.DataStores

## Overview

The DataStores framework provides ServiceType auto-discovery for data storage abstractions with unified interfaces that work across different storage systems and data persistence strategies.

## Features

- **ServiceType Auto-Discovery**: Add data store packages and they're automatically registered
- **Universal Storage Interface**: Same API works with all storage systems
- **Dynamic Store Creation**: Data stores created via factories
- **Source-Generated Collections**: High-performance store lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.DataStores\FractalDataWorks.DataStores.csproj" />
<ProjectReference Include="..\FractalDataWorks.DataStores.FileSystem\FractalDataWorks.DataStores.FileSystem.csproj" />
```

### 2. Register Services

```csharp
// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IGenericDataStoreProvider, GenericDataStoreProvider>();

// Single line registers ALL discovered data store types
DataStoreTypes.Register(builder.Services);
```

### 3. Configure Data Stores

```json
{
  "DataStores": {
    "DocumentStore": {
      "StoreType": "FileSystem",
      "BasePath": "C:\\Data\\Documents",
      "EnableVersioning": true,
      "CompressionLevel": "Optimal"
    },
    "CacheStore": {
      "StoreType": "Memory",
      "MaxSizeBytes": 104857600,
      "EvictionPolicy": "LRU"
    }
  }
}
```

### 4. Use Universal Data Storage

```csharp
public class DocumentService
{
    private readonly IGenericDataStoreProvider _dataStoreProvider;

    public DocumentService(IGenericDataStoreProvider dataStoreProvider)
    {
        _dataStoreProvider = dataStoreProvider;
    }

    public async Task<IGenericResult<string>> StoreDocumentAsync(string documentId, byte[] content)
    {
        var storeResult = await _dataStoreProvider.GetDataStore("DocumentStore");
        if (!storeResult.IsSuccess)
            return GenericResult<string>.Failure(storeResult.Error);

        using var store = storeResult.Value;

        // Universal storage - works with any data store
        var storageKey = $"documents/{documentId}";
        var result = await store.StoreAsync(storageKey, content);

        return result.IsSuccess
            ? GenericResult<string>.Success(storageKey)
            : GenericResult<string>.Failure(result.Error);
    }

    public async Task<IGenericResult<byte[]>> RetrieveDocumentAsync(string documentId)
    {
        var storeResult = await _dataStoreProvider.GetDataStore("DocumentStore");
        if (!storeResult.IsSuccess)
            return GenericResult<byte[]>.Failure(storeResult.Error);

        using var store = storeResult.Value;

        var storageKey = $"documents/{documentId}";
        var result = await store.RetrieveAsync(storageKey);

        return result;
    }

    public async Task<IGenericResult> DeleteDocumentAsync(string documentId)
    {
        var storeResult = await _dataStoreProvider.GetDataStore("DocumentStore");
        if (!storeResult.IsSuccess)
            return GenericResult.Failure(storeResult.Error);

        using var store = storeResult.Value;

        var storageKey = $"documents/{documentId}";
        return await store.DeleteAsync(storageKey);
    }
}
```

## Available Data Store Types

| Package | Store Type | Purpose |
|---------|-----------|---------|
| `FractalDataWorks.DataStores.FileSystem` | FileSystem | Local and network file storage |
| `FractalDataWorks.DataStores.AzureBlob` | AzureBlob | Azure Blob Storage |
| `FractalDataWorks.DataStores.AmazonS3` | AmazonS3 | Amazon S3 storage |
| `FractalDataWorks.DataStores.Memory` | Memory | In-memory storage with optional persistence |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `DataStoreTypeBase`
3. **Generates Collections**: Creates `DataStoreTypes.All`, `DataStoreTypes.Name()`, etc.
4. **Self-Registration**: Each data store type handles its own DI registration

## Adding Custom Data Store Types

```csharp
// 1. Create your data store type (singleton pattern)
public sealed class CustomDataStoreType : DataStoreTypeBase<IGenericDataStore, CustomDataStoreConfiguration, ICustomDataStoreFactory>
{
    public static CustomDataStoreType Instance { get; } = new();

    private CustomDataStoreType() : base(5, "Custom", "Data Stores") { }

    public override Type FactoryType => typeof(ICustomDataStoreFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomDataStoreFactory, CustomDataStoreFactory>();
        services.AddScoped<CustomStorageEngine>();
        services.AddScoped<CustomIndexManager>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. DataStoreTypes.Register(services) will include it automatically
```

## Common Storage Patterns

### Hierarchical Storage

```csharp
public class AssetManagementService
{
    private readonly IGenericDataStoreProvider _dataStoreProvider;

    public AssetManagementService(IGenericDataStoreProvider dataStoreProvider)
    {
        _dataStoreProvider = dataStoreProvider;
    }

    public async Task<IGenericResult<string>> StoreAssetAsync(string category, string assetName, Stream assetData)
    {
        var storeResult = await _dataStoreProvider.GetDataStore("DocumentStore");
        if (!storeResult.IsSuccess)
            return GenericResult<string>.Failure(storeResult.Error);

        using var store = storeResult.Value;

        // Hierarchical storage path
        var storageKey = $"assets/{category}/{assetName}";

        // Convert stream to byte array
        using var memoryStream = new MemoryStream();
        await assetData.CopyToAsync(memoryStream);
        var content = memoryStream.ToArray();

        var result = await store.StoreAsync(storageKey, content);
        return result.IsSuccess
            ? GenericResult<string>.Success(storageKey)
            : GenericResult<string>.Failure(result.Error);
    }

    public async Task<IGenericResult<List<string>>> ListAssetsAsync(string category)
    {
        var storeResult = await _dataStoreProvider.GetDataStore("DocumentStore");
        if (!storeResult.IsSuccess)
            return GenericResult<List<string>>.Failure(storeResult.Error);

        using var store = storeResult.Value;

        var prefix = $"assets/{category}/";
        var result = await store.ListKeysAsync(prefix);

        return result;
    }
}
```

### Caching Layer

```csharp
public class CachedDataService
{
    private readonly IGenericDataStoreProvider _dataStoreProvider;

    public CachedDataService(IGenericDataStoreProvider dataStoreProvider)
    {
        _dataStoreProvider = dataStoreProvider;
    }

    public async Task<IGenericResult<T>> GetCachedDataAsync<T>(string cacheKey, Func<Task<T>> dataFactory, TimeSpan? expiration = null)
    {
        var cacheResult = await _dataStoreProvider.GetDataStore("CacheStore");
        if (!cacheResult.IsSuccess)
        {
            // Cache unavailable, get data directly
            var data = await dataFactory();
            return GenericResult<T>.Success(data);
        }

        using var cache = cacheResult.Value;

        // Try to get from cache first
        var cachedResult = await cache.RetrieveAsync<T>(cacheKey);
        if (cachedResult.IsSuccess)
            return cachedResult;

        // Cache miss, get data and cache it
        var freshData = await dataFactory();
        var cacheOptions = new StorageOptions
        {
            Expiration = expiration ?? TimeSpan.FromMinutes(30)
        };

        await cache.StoreAsync(cacheKey, freshData, cacheOptions);
        return GenericResult<T>.Success(freshData);
    }

    public async Task<IGenericResult> InvalidateCacheAsync(string cacheKey)
    {
        var cacheResult = await _dataStoreProvider.GetDataStore("CacheStore");
        if (!cacheResult.IsSuccess)
            return GenericResult.Success(); // Cache unavailable, consider it invalidated

        using var cache = cacheResult.Value;
        return await cache.DeleteAsync(cacheKey);
    }
}
```

### Multi-Tier Storage

```csharp
public class TieredStorageService
{
    private readonly IGenericDataStoreProvider _dataStoreProvider;

    public TieredStorageService(IGenericDataStoreProvider dataStoreProvider)
    {
        _dataStoreProvider = dataStoreProvider;
    }

    public async Task<IGenericResult<T>> GetDataAsync<T>(string key)
    {
        // Try hot cache first
        var hotCacheResult = await _dataStoreProvider.GetDataStore("HotCache");
        if (hotCacheResult.IsSuccess)
        {
            using var hotCache = hotCacheResult.Value;
            var hotResult = await hotCache.RetrieveAsync<T>(key);
            if (hotResult.IsSuccess)
                return hotResult;
        }

        // Try warm cache
        var warmCacheResult = await _dataStoreProvider.GetDataStore("WarmCache");
        if (warmCacheResult.IsSuccess)
        {
            using var warmCache = warmCacheResult.Value;
            var warmResult = await warmCache.RetrieveAsync<T>(key);
            if (warmResult.IsSuccess)
            {
                // Promote to hot cache
                if (hotCacheResult.IsSuccess)
                {
                    using var hotCache = hotCacheResult.Value;
                    await hotCache.StoreAsync(key, warmResult.Value, new StorageOptions { Expiration = TimeSpan.FromMinutes(5) });
                }
                return warmResult;
            }
        }

        // Try cold storage
        var coldStorageResult = await _dataStoreProvider.GetDataStore("ColdStorage");
        if (!coldStorageResult.IsSuccess)
            return GenericResult<T>.Failure("No storage tier available");

        using var coldStorage = coldStorageResult.Value;
        var coldResult = await coldStorage.RetrieveAsync<T>(key);

        if (coldResult.IsSuccess)
        {
            // Promote through tiers
            if (warmCacheResult.IsSuccess)
            {
                using var warmCache = warmCacheResult.Value;
                await warmCache.StoreAsync(key, coldResult.Value, new StorageOptions { Expiration = TimeSpan.FromHours(1) });
            }
        }

        return coldResult;
    }
}
```

## Architecture Benefits

- **Storage Agnostic**: Switch storage systems without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of data store types
- **Performance**: Source-generated collections use FrozenDictionary
- **Scalability**: Each data store type manages its own storage strategy
- **Flexibility**: Support for caching, tiering, and complex storage patterns

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).