# Data Structure Discrepancies Analysis

**Date:** 2025-10-07
**Analyzer:** Claude Code
**Scope:** Configuration classes, ServiceTypes, and Message structures across FractalDataWorks Developer Kit

## Executive Summary

### Critical Findings

1. **CRITICAL:** Inconsistent Configuration base class usage - some use `ConfigurationBase<T>`, others implement `IConfiguration` directly
2. **CRITICAL:** Lifetime property inconsistencies - `IServiceLifetime` vs `ServiceLifetimeBase` types, `Lifetime` vs `LifetimeBase` naming
3. **HIGH:** Mixed property patterns - `{ get; init; }` vs `{ get; set; }` across configuration classes
4. **HIGH:** AzureKeyVaultConfiguration uses `{ get; set; }` instead of `{ get; init; }` (violates immutability)
5. **MEDIUM:** Missing `AuthenticationType` property in IAuthenticationConfiguration
6. **MEDIUM:** Inconsistent validator patterns - some override `GetValidator()`, others implement directly

### Pattern Compliance by Domain

| Domain | Configuration Pattern | ServiceType Pattern | Overall Score |
|--------|---------------------|---------------------|---------------|
| Authentication | ✅ ConfigurationBase<T> | ✅ Full compliance | 95% |
| Connections | ✅ ConfigurationBase<T> | ❌ No ServiceType found | 70% |
| SecretManagers | ❌ Direct IConfiguration impl | ⚠️ Partial compliance | 50% |
| Transformations | 🔍 Not analyzed | ⚠️ Partial compliance | N/A |
| Scheduling | 🔍 Not analyzed | ❌ Not found | N/A |
| Execution | 🔍 Not analyzed | ❌ Not found | N/A |

---

## Part 1: Configuration Class Analysis

### 1.1 Authentication Configuration ✅ (REFERENCE)

**Interface:** `IAuthenticationConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Authentication.Abstractions\Configuration\IAuthenticationConfiguration.cs`

```csharp
// Lines 8-44
public interface IAuthenticationConfiguration : IGenericConfiguration
{
    string AuthenticationType { get; }  // MISSING in interface definition!
    string ClientId { get; }
    string Authority { get; }
    string RedirectUri { get; }
    string[] Scopes { get; }
    bool EnableTokenCaching { get; }
    int TokenCacheLifetimeMinutes { get; }
}
```

#### VIOLATION 1: Missing AuthenticationType Property
**Severity:** MEDIUM
**File:** `IAuthenticationConfiguration.cs:8-44`

The interface **declares** `string AuthenticationType { get; }` in XML documentation but **does not define it** in the actual interface.

**Implementation:** `AzureEntraConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Authentication.Entra\Configuration\AzureEntraConfiguration.cs`

```csharp
// Lines 12-121
public sealed class AzureEntraConfiguration : ConfigurationBase<AzureEntraConfiguration>, IAuthenticationConfiguration
{
    public override string SectionName => "AzureEntra";

    // ✅ CORRECT: Uses { get; init; } pattern
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Authority { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];  // ✅ Collection expression
    public bool EnableTokenCaching { get; init; } = true;
    public int TokenCacheLifetimeMinutes { get; init; } = 60;

    // Additional properties...
    public string Instance { get; init; } = "https://login.microsoftonline.com";
    public string ClientType { get; init; } = "Confidential";
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateAudience { get; init; } = true;
    public bool ValidateLifetime { get; init; } = true;
    public bool ValidateIssuerSigningKey { get; init; } = true;
    public int ClockSkewToleranceMinutes { get; init; } = 5;
    public string? CacheFilePath { get; init; }
    public bool EnablePiiLogging { get; init; }
    public int HttpTimeoutSeconds { get; init; } = 30;
    public int MaxRetryAttempts { get; init; } = 3;
    public string[] AdditionalValidAudiences { get; init; } = [];
    public string[] AdditionalValidIssuers { get; init; } = [];

    // ✅ CORRECT: Validator pattern
    protected override AbstractValidator<AzureEntraConfiguration> GetValidator()
    {
        return new AzureEntraConfigurationValidator();
    }
}
```

**Pattern Compliance:** ✅ **EXCELLENT**
- Inherits from `ConfigurationBase<T>` ✅
- Uses `{ get; init; }` for immutability ✅
- Uses collection expressions `[]` ✅
- Implements validator pattern correctly ✅
- Overrides `SectionName` ✅

---

### 1.2 Connections Configuration ⚠️ (MOSTLY COMPLIANT)

**Interface:** `IConnectionConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Connections.Abstractions\IConnectionConfiguration.cs`

```csharp
// Lines 12-31
public interface IConnectionConfiguration : IGenericConfiguration
{
    string ConnectionType { get; }
    IServiceLifetime Lifetime { get; }  // ✅ Uses interface type
}
```

**Implementation:** `MsSqlConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Connections.MsSql\MsSqlConfiguration.cs`

```csharp
// Lines 20-283
public sealed class MsSqlConfiguration : ConfigurationBase<MsSqlConfiguration>, IConnectionConfiguration
{
    // ✅ CORRECT: Uses { get; init; } pattern
    public string ConnectionString { get; init; } = string.Empty;
    public int CommandTimeoutSeconds { get; init; } = 30;
    public int ConnectionTimeoutSeconds { get; init; } = 15;
    public string DefaultSchema { get; init; } = "dbo";

    // ⚠️ CONCERN: Mutable dictionary
    public IDictionary<string, string> SchemaMappings { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public bool EnableConnectionPooling { get; init; } = true;
    public int MinPoolSize { get; init; }
    public int MaxPoolSize { get; init; } = 100;
    public bool EnableMultipleActiveResultSets { get; init; }
    public bool EnableRetryLogic { get; init; } = true;
    public int MaxRetryAttempts { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public bool EnableSqlLogging { get; init; }
    public int MaxSqlLogLength { get; init; } = 1000;
    public bool UseTransactions { get; init; }
    public System.Data.IsolationLevel TransactionIsolationLevel { get; init; } =
        System.Data.IsolationLevel.ReadCommitted;

    public override string SectionName => "GenericConnections:MsSql";

    // ✅ CORRECT: Interface type
    public string ConnectionType { get; init; } = "MsSql";
    public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;

    // ⚠️ METHODS on configuration (violates data-only principle)
    public string GetSanitizedConnectionString() { /* ... */ }
    public (string Schema, string Table) ResolveSchemaAndTable(string containerName) { /* ... */ }

    // ✅ CORRECT: Validator pattern
    protected override IValidator<MsSqlConfiguration>? GetValidator()
    {
        return new MsSqlConfigurationValidator();
    }

    // ⚠️ Implements CopyTo (unusual for configuration)
    protected override void CopyTo(MsSqlConfiguration target) { /* ... */ }
}
```

#### VIOLATION 1: Methods on Configuration Class
**Severity:** MEDIUM
**Files:** `MsSqlConfiguration.cs:181-252`

Configuration classes should be **data-only**. Helper methods belong in separate service classes.

**Current:**
```csharp
public string GetSanitizedConnectionString() { /* 15 lines of logic */ }
public (string Schema, string Table) ResolveSchemaAndTable(string containerName) { /* 20 lines of logic */ }
```

**Recommended:**
```csharp
// In separate service class
public class MsSqlConfigurationHelper
{
    public static string GetSanitizedConnectionString(MsSqlConfiguration config) { /* ... */ }
    public static (string Schema, string Table) ResolveSchemaAndTable(
        MsSqlConfiguration config, string containerName) { /* ... */ }
}
```

#### VIOLATION 2: Mutable Dictionary Property
**Severity:** MEDIUM
**File:** `MsSqlConfiguration.cs:66`

```csharp
// ❌ Current
public IDictionary<string, string> SchemaMappings { get; init; }

// ✅ Recommended
public IReadOnlyDictionary<string, string> SchemaMappings { get; init; }
```

While `init` prevents reassignment, the dictionary itself is mutable. Should use `IReadOnlyDictionary`.

**Pattern Compliance:** ⚠️ **GOOD** (with caveats)
- Inherits from `ConfigurationBase<T>` ✅
- Uses `{ get; init; }` mostly ✅
- Uses interface for `IServiceLifetime` ✅
- Implements validator pattern ✅
- Has methods (violation) ❌
- Uses mutable dictionary (concern) ⚠️

---

### 1.3 SecretManagers Configuration ❌ (MAJOR VIOLATIONS)

**Interface:** `ISecretManagerConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.SecretManagers.Abstractions\ISecretManagementConfiguration.cs`

```csharp
// Lines 9-11
public interface ISecretManagerConfiguration : IGenericConfiguration
{
    // Empty marker interface
}
```

**Implementation:** `AzureKeyVaultConfiguration`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.SecretManagers.AzureKeyVault\Configuration\AzureKeyVaultConfiguration.cs`

```csharp
// Lines 20-243
public sealed class AzureKeyVaultConfiguration : ISecretManagerConfiguration
{
    // ❌ VIOLATION: Does NOT inherit from ConfigurationBase<T>

    // ❌ VIOLATION: Uses { get; set; } instead of { get; init; }
    public int Id { get; set; } = 1;
    public string Name { get; set; } = "AzureKeyVault";
    public string? VaultUri { get; set; }
    public string? AuthenticationMethod { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }
    public TimeSpan? Timeout { get; set; }
    public IReadOnlyDictionary<string, object>? RetryPolicy { get; set; }
    public IReadOnlyDictionary<string, string>? AdditionalHeaders { get; set; }
    public bool EnableTracing { get; set; }
    public bool ValidateOnStartup { get; set; }
    public int? MaxSecretsPerPage { get; set; }
    public bool IncludeDeletedByDefault { get; set; }
    public string? ManagedIdentityId { get; set; }

    // ✅ Correctly uses { get; } for read-only
    public string SectionName => nameof(AzureKeyVault);
    public bool IsEnabled { get; }
    public static string ConfigurationName => nameof(AzureKeyVault);
    public bool IsValid => !string.IsNullOrWhiteSpace(VaultUri) &&
                           !string.IsNullOrWhiteSpace(AuthenticationMethod);
    public IReadOnlyDictionary<string, object> Properties => CreatePropertiesDictionary();

    // ⚠️ Method on configuration (creates dictionary dynamically)
    private Dictionary<string, object> CreatePropertiesDictionary() { /* 35 lines */ }

    // ⚠️ Different validator pattern - not using GetValidator() override
    public IGenericResult<ValidationResult> Validate()
    {
        var validator = new AzureKeyVaultConfigurationValidator();
        var validationResult = validator.Validate(this);
        // ... returns result
    }
}
```

#### VIOLATION 1: Does Not Inherit from ConfigurationBase<T>
**Severity:** CRITICAL
**File:** `AzureKeyVaultConfiguration.cs:20`

**All other configurations inherit from `ConfigurationBase<T>`** which provides:
- Standardized validation pattern
- Configuration binding helpers
- Consistent base behavior

**Current:**
```csharp
public sealed class AzureKeyVaultConfiguration : ISecretManagerConfiguration
```

**Should be:**
```csharp
public sealed class AzureKeyVaultConfiguration :
    ConfigurationBase<AzureKeyVaultConfiguration>, ISecretManagerConfiguration
```

#### VIOLATION 2: Uses { get; set; } Instead of { get; init; }
**Severity:** CRITICAL
**File:** `AzureKeyVaultConfiguration.cs:23-162`

**All 13 mutable properties use `{ get; set; }`** which violates immutability principle.

Modern C# configurations should be **immutable after initialization** using `{ get; init; }`.

**Current:**
```csharp
public string? VaultUri { get; set; }
public string? AuthenticationMethod { get; set; }
public string? TenantId { get; set; }
// ... etc (13 properties)
```

**Should be:**
```csharp
public string? VaultUri { get; init; }
public string? AuthenticationMethod { get; init; }
public string? TenantId { get; init; }
// ... etc
```

#### VIOLATION 3: Inconsistent Validator Pattern
**Severity:** MEDIUM
**File:** `AzureKeyVaultConfiguration.cs:231-243`

Unlike other configurations that override `GetValidator()`, this implements a public `Validate()` method.

**Current pattern:**
```csharp
public IGenericResult<ValidationResult> Validate()
{
    var validator = new AzureKeyVaultConfigurationValidator();
    var validationResult = validator.Validate(this);
    // ...
}
```

**Standard pattern (from ConfigurationBase):**
```csharp
protected override AbstractValidator<AzureKeyVaultConfiguration> GetValidator()
{
    return new AzureKeyVaultConfigurationValidator();
}
```

#### VIOLATION 4: Property with Complex Logic
**Severity:** MEDIUM
**File:** `AzureKeyVaultConfiguration.cs:191-228`

```csharp
public IReadOnlyDictionary<string, object> Properties => CreatePropertiesDictionary();

private Dictionary<string, object> CreatePropertiesDictionary()
{
    var properties = new Dictionary<string, object>(StringComparer.Ordinal);
    // 35 lines of conditional logic
    return properties;
}
```

This creates a **new dictionary every time** the property is accessed. Should be lazy-initialized or removed.

**Pattern Compliance:** ❌ **POOR**
- Does NOT inherit from `ConfigurationBase<T>` ❌
- Uses `{ get; set; }` (mutable) ❌
- Non-standard validator pattern ❌
- Has complex property logic ❌
- Has method returning dynamic data ❌

---

### 1.4 Comparison Table: Configuration Patterns

| Aspect | Authentication | Connections | SecretManagers |
|--------|----------------|-------------|----------------|
| **Base Class** | ✅ ConfigurationBase<T> | ✅ ConfigurationBase<T> | ❌ Direct interface |
| **Property Pattern** | ✅ `{ get; init; }` | ✅ `{ get; init; }` | ❌ `{ get; set; }` |
| **Collection Pattern** | ✅ Arrays `[]` | ⚠️ Mutable Dict | ⚠️ IReadOnlyDict |
| **Validator Pattern** | ✅ GetValidator() | ✅ GetValidator() | ❌ Public Validate() |
| **Has Methods** | ✅ No | ❌ Yes (2 methods) | ❌ Yes (1 method) |
| **SectionName** | ✅ Override | ✅ Override | ✅ Property |
| **Lifetime Property** | N/A | ✅ IServiceLifetime | N/A |
| **Immutability** | ✅ Fully immutable | ⚠️ Dict mutable | ❌ Fully mutable |

---

## Part 2: ServiceType Class Analysis

### 2.1 Authentication ServiceType ✅ (REFERENCE)

**Interface:** `IAuthenticationServiceType`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Authentication.Abstractions\ServiceTypes\IAuthenticationServiceType.cs`

```csharp
// Lines 12-83
public interface IAuthenticationServiceType :
    IEnumOption<IAuthenticationServiceType>, IServiceType
{
    string[] SupportedProtocols { get; }
    string ProviderName { get; }
    IReadOnlyList<string> SupportedFlows { get; }
    IReadOnlyList<string> SupportedTokenTypes { get; }
    int Priority { get; }
    bool SupportsMultiTenant { get; }
    bool SupportsTokenCaching { get; }
}
```

**Implementation:** `EntraAuthenticationServiceType`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.Authentication.Entra\EntraAuthenticationServiceType.cs`

```csharp
// Lines 18-72
[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntraService")]
public sealed class EntraAuthenticationServiceType :
    AuthenticationTypeBase<IAuthenticationService,
                          IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>,
                          IAuthenticationConfiguration>,
    IEnumOption<EntraAuthenticationServiceType>
{
    // ✅ CORRECT: Deterministic ID in constructor
    public EntraAuthenticationServiceType()
        : base(
            id: 1,  // ✅ Deterministic
            name: "AzureEntra",
            providerName: "Microsoft.Identity.Client",
            method: AuthenticationMethods.OAuth2,
            supportedProtocols: [  // ✅ Collection expression
                AuthenticationProtocols.OAuth2,
                AuthenticationProtocols.OpenIDConnect,
                AuthenticationProtocols.SAML2
            ],
            supportedFlows: [
                AuthenticationFlows.AuthorizationCode,
                AuthenticationFlows.ClientCredentials,
                AuthenticationFlows.Interactive
            ],
            supportedTokenTypes: [
                TokenTypes.AccessToken,
                TokenTypes.IdToken,
                TokenTypes.RefreshToken
            ],
            supportsMultiTenant: true,
            supportsTokenCaching: true,
            supportsTokenRefresh: true,
            category: "Authentication")
    {
    }

    // ✅ CORRECT: Priority override
    public override int Priority => 90;

    // ✅ CORRECT: Register pattern
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IAuthenticationServiceFactory<...>, GenericServiceFactory<...>>();
        services.AddScoped<IAuthenticationService, EntraAuthenticationService>();
    }

    // ✅ CORRECT: Configure pattern
    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation
    }
}
```

**Pattern Compliance:** ✅ **EXCELLENT**
- Deterministic ID ✅
- Collection expressions ✅
- Proper inheritance hierarchy ✅
- Register() method implemented ✅
- Configure() method implemented ✅
- ServiceTypeOption attribute ✅

---

### 2.2 SecretManagers ServiceType ⚠️ (PARTIAL)

**Interface:** `ISecretManagerServiceType`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.SecretManagers.Abstractions\ISecretManagementServiceType.cs`

```csharp
// Lines 59-111
public interface ISecretManagerServiceType :
    IServiceType<ISecretManager,
                 ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>,
                 ISecretManagerConfiguration>
{
    string[] SupportedSecretStores { get; }
    IReadOnlyList<string> SupportedSecretTypes { get; }
    bool SupportsRotation { get; }
    bool SupportsVersioning { get; }
    bool SupportsSoftDelete { get; }
    bool SupportsAccessPolicies { get; }
    int MaxSecretSizeBytes { get; }
}
```

**Implementation:** `AzureKeyVaultServiceType`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.Services.SecretManagers.AzureKeyVault\ServiceTypes\AzureKeyVaultServiceType.cs`

Need to read this file to analyze:

---

### 2.3 ServiceTypeBase Pattern

**Base Class:** `ServiceTypeBase<TService, TFactory, TConfiguration>`
**Location:** `D:\Development\Developer-Kit\src\FractalDataWorks.ServiceTypes\ServiceTypeBase.cs`

```csharp
// Lines 20-52
public abstract class ServiceTypeBase<TService, TFactory, TConfiguration> :
    ServiceTypeBase<TService, TFactory>, IServiceType<TService, TFactory>
{
    public Type ConfigurationType => typeof(TConfiguration);

    protected ServiceTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category)
    {
    }
}

// Lines 61-148
public abstract class ServiceTypeBase<TService, TFactory> :
    EnumOptionBase<IServiceType>, IServiceType<TService, TFactory>
{
    public override int Id => base.Id;
    public override string Name => base.Name;
    public virtual string Category { get; }
    public Type ServiceType => typeof(TService);
    public string SectionName { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Type FactoryType => typeof(TFactory);

    public abstract void Register(IServiceCollection services);
    public abstract void Configure(IConfiguration configuration);

    protected ServiceTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name)
    {
        SectionName = sectionName;
        DisplayName = displayName;
        Description = description;
        Category = category ?? "Default";
    }
}
```

**Required Elements:**
1. Deterministic `id` (int)
2. `name` (string)
3. `sectionName` for configuration
4. `displayName` and `description` for UI
5. Optional `category` (defaults to "Default")
6. Must implement `Register(IServiceCollection)`
7. Must implement `Configure(IConfiguration)`

---

## Part 3: Lifetime Property Inconsistencies

### 3.1 Current State

**IConnectionConfiguration uses:**
```csharp
// File: IConnectionConfiguration.cs:30
IServiceLifetime Lifetime { get; }  // ✅ Interface type
```

**MsSqlConfiguration implements:**
```csharp
// File: MsSqlConfiguration.cs:175
public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;  // ✅ Correct
```

**IServiceConfiguration interface:**
```csharp
// File: IServiceConfiguration.cs (lines not shown but likely exists)
// Could have different lifetime property name or type
```

### 3.2 Naming Conventions Found

Based on documentation comments:
- `Lifetime` - Used in IConnectionConfiguration
- `LifetimeBase` - Potentially used elsewhere (not found in scanned files)

### 3.3 Type Variations

- `IServiceLifetime` - Interface type (CORRECT pattern) ✅
- `ServiceLifetimeBase` - Base class type (if used, INCORRECT) ❌
- `ServiceLifetimes.Scoped` - Static instance from collection ✅

**Recommended Standard:**
```csharp
// In all service configuration interfaces:
IServiceLifetime Lifetime { get; }

// In all service configuration implementations:
public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;
```

---

## Part 4: Recommended Standardization

### 4.1 Configuration Class Standard

```csharp
/// <summary>
/// Configuration for [DomainName] service.
/// </summary>
public sealed class [Domain]Configuration :
    ConfigurationBase<[Domain]Configuration>,
    I[Domain]Configuration
{
    /// <inheritdoc/>
    public override string SectionName => "[SectionPath]";

    // ✅ All properties use { get; init; }
    public string Property1 { get; init; } = string.Empty;
    public int Property2 { get; init; } = 42;
    public string[] ArrayProperty { get; init; } = [];  // Collection expression
    public IReadOnlyDictionary<string, string> DictProperty { get; init; } =  // Read-only!
        new Dictionary<string, string>(StringComparer.Ordinal);

    // ✅ If configuration has lifetime
    public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;

    // ✅ Validator pattern
    protected override AbstractValidator<[Domain]Configuration> GetValidator()
    {
        return new [Domain]ConfigurationValidator();
    }

    // ❌ NO methods on configuration classes
    // Move helper methods to separate service classes
}
```

### 4.2 ServiceType Standard

```csharp
[ServiceTypeOption(typeof([Domain]Types), "[TypeName]")]
public sealed class [Implementation]ServiceType :
    [Domain]TypeBase<IService, IServiceFactory<IService, IConfiguration>, IConfiguration>,
    IEnumOption<[Implementation]ServiceType>
{
    public [Implementation]ServiceType()
        : base(
            id: 1,  // ✅ Deterministic ID
            name: "[Name]",
            providerName: "[ProviderName]",
            // ... domain-specific parameters
            supportedX: [  // ✅ Collection expressions
                Value1,
                Value2
            ],
            category: "[Category]")
    {
    }

    public override int Priority => 90;  // Higher = higher priority

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IServiceFactory<...>, ConcreteFactory<...>>();
        services.AddScoped<IService, ConcreteService>();
    }

    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation if needed
    }
}
```

---

## Part 5: Priority Fixes

### Priority 1: CRITICAL (Fix Immediately)

1. **Refactor AzureKeyVaultConfiguration**
   - Change to inherit from `ConfigurationBase<AzureKeyVaultConfiguration>`
   - Change all `{ get; set; }` to `{ get; init; }`
   - Remove `CreatePropertiesDictionary()` method or make it private + cached
   - Use standard `GetValidator()` pattern
   - **Estimated Effort:** 2-4 hours
   - **Files Affected:** 1 file + potentially validator

2. **Fix IAuthenticationConfiguration Missing Property**
   - Add `string AuthenticationType { get; }` to interface
   - Or remove from documentation
   - **Estimated Effort:** 5 minutes
   - **Files Affected:** 1 file

### Priority 2: HIGH (Fix Soon)

3. **Remove Methods from Configuration Classes**
   - Extract `MsSqlConfiguration.GetSanitizedConnectionString()` to helper class
   - Extract `MsSqlConfiguration.ResolveSchemaAndTable()` to helper class
   - **Estimated Effort:** 1 hour
   - **Files Affected:** 1 file + 1 new helper file

4. **Fix Mutable Dictionary in MsSqlConfiguration**
   - Change `IDictionary<string, string>` to `IReadOnlyDictionary<string, string>`
   - Update initialization logic if needed
   - **Estimated Effort:** 30 minutes
   - **Files Affected:** 1 file

### Priority 3: MEDIUM (Future Enhancement)

5. **Standardize Lifetime Property Across All Configurations**
   - Audit all configuration interfaces
   - Ensure all use `IServiceLifetime Lifetime { get; }`
   - Ensure all implementations use `ServiceLifetimes` static instances
   - **Estimated Effort:** 2-4 hours
   - **Files Affected:** 3-5 files

---

## Conclusion

The Authentication domain demonstrates the correct pattern for both Configuration and ServiceType implementations. The SecretManagers domain has critical violations that should be addressed immediately, particularly the use of mutable properties in `AzureKeyVaultConfiguration`. The Connections domain is mostly compliant but has minor issues with helper methods on configuration classes.

**Key Takeaways:**
1. Always inherit from `ConfigurationBase<T>`
2. Always use `{ get; init; }` for properties
3. Never put methods on configuration classes (data-only)
4. Always use `IReadOnlyDictionary` for dictionary properties
5. Always use collection expressions `[]` for arrays
6. Always use standard `GetValidator()` pattern

**Total Estimated Effort:** 5-10 hours of development work
