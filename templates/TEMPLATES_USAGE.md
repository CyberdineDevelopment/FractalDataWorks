# FractalDataWorks Developer Kit - Template Usage Guide

Complete guide for using FractalDataWorks domain service and connection templates.

## Quick Reference

### Project Templates

#### Create Domain Service
```powershell
dotnet new fractaldataworks-domain -n Billing
```

#### Create Connection Service
```powershell
dotnet new fractaldataworks-connection -n PostgreSql --ConnectionCategory Database
```

### Item Templates - Services

#### Add Service Class to Project
```powershell
dotnet new fractaldataworks-service -n MyService --DomainName MyDomain
```

#### Add ServiceType
```powershell
dotnet new fractaldataworks-servicetype -n MyServiceType --DomainName MyDomain
```

#### Add Structured Logging
```powershell
dotnet new fractaldataworks-logging -n MyService
```

### Item Templates - Data Architecture

#### Add DataCommand
```powershell
dotnet new fractaldataworks-datacommand -n Upsert --UniqueId 5 --Category Insert --HasInputData true --HasFilter true
```

#### Add DataTransformer
```powershell
dotnet new fractaldataworks-transformer -n PayPalToTransaction --UniqueId 1 --InputType PayPalTransaction --OutputType Transaction
```

#### Add DataConcept Configuration
```powershell
dotnet new fractaldataworks-dataconcept -n TransactionData --RecordTypeName MyApp.Models.Transaction --IncludeMultipleSources true
```

## Installation

```powershell
# Install templates from this directory
cd C:\development\github\Developer-Kit
dotnet new install templates/FractalDataWorks.Service.Domain
dotnet new install templates/FractalDataWorks.Service.Connection
dotnet new install templates/FractalDataWorks.ItemTemplates

# Verify installation
dotnet new list | Select-String "fractaldataworks"
```

## Generated Project Structure

### Domain Service (fractaldataworks-domain)
```
FractalDataWorks.Services.{DomainName}.Abstractions/
├── Commands/
│   ├── IDomainNameCommand.cs
│   ├── DomainNameCommands.cs ([TypeCollection])
│   └── ISampleCommand.cs
├── Configuration/
│   └── IDomainNameConfiguration.cs
├── Messages/
│   ├── DomainNameMessage.cs
│   ├── DomainNameMessageCollectionBase.cs ([MessageCollection])
│   └── ConfigurationNullMessage.cs
├── Providers/
│   ├── IDomainNameProvider.cs
│   └── IDomainNameService.cs
├── ServiceTypes/
│   ├── IDomainNameType.cs
│   ├── DomainNameTypes.cs ([ServiceTypeCollection])
│   └── DomainNameTypeBase.cs
└── DomainNameServiceBase.cs

FractalDataWorks.Services.{DomainName}/
├── ServiceTypes/
│   └── DefaultDomainNameServiceType.cs ([ServiceTypeOption])
├── Logging/
│   └── DomainNameServiceLog.cs (source-generated)
├── DomainNameProvider.cs
└── DefaultDomainNameService.cs
```

## Template Parameters

### Domain Service Template

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-n\|--name` | string | MyDomain | Domain name (required) |
| `--CreateAbstractionsProject` | bool | true | Create abstractions project |
| `--IncludeServiceTypes` | bool | true | Add ServiceType collection |
| `--IncludeCommands` | bool | true | Add command interfaces |
| `--IncludeProvider` | bool | true | Add provider pattern |
| `--IncludeMessages` | bool | true | Add message collection |
| `--IncludeLogging` | bool | true | Add structured logging |
| `--IncludeConfiguration` | bool | true | Add configuration |
| `--IncludeTypeCollections` | bool | false | Add TypeCollection examples |

### Connection Service Template

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-n\|--name` | string | MyConnection | Connection type (required) |
| `--ConnectionCategory` | choice | Database | Database\|MessageQueue\|Cache\|Api\|Custom |
| `--IncludeStateMachine` | bool | true | Include state tracking |
| `--IncludeTransactions` | bool | true | Include transactions |
| `--IncludeQueryTranslator` | bool | true | Include LINQ translator |
| `--IncludeMessages` | bool | true | Include messages |
| `--IncludeLogging` | bool | true | Include logging |

## Framework Conventions

All templates follow FractalDataWorks standards:

### Project Settings
- ImplicitUsings: **disabled** (explicit using statements required)
- Nullable: **enabled**
- LangVersion: **preview**
- Target Frameworks:
  - Abstractions: `netstandard2.0`
  - Implementation: `netstandard2.0;net10.0` or `net10.0`

### Source Generators
Automatically configured:
- Collections.SourceGenerators (TypeCollections)
- ServiceTypes.SourceGenerators (ServiceType discovery)
- Messages.SourceGenerators (Message collections)

### Naming
- Domain services: `FractalDataWorks.Services.{Domain}[.Abstractions]`
- Connections: `FractalDataWorks.Services.Connections.{Type}`
- ServiceTypes: Singleton pattern with deterministic GUIDs

## Complete Example: Billing Domain

```powershell
# 1. Create domain service (creates 2 projects)
dotnet new fractaldataworks-domain -n Billing

# 2. Navigate to implementation project
cd FractalDataWorks.Services.Billing

# 3. Add Stripe service type
dotnet new fractaldataworks-servicetype -n Stripe --DomainName Billing --namespace FractalDataWorks.Services.Billing.ServiceTypes

# 4. Add Stripe service
dotnet new fractaldataworks-service -n Stripe --DomainName Billing --namespace FractalDataWorks.Services.Billing

# 5. Add logging for Stripe
dotnet new fractaldataworks-logging -n Stripe --namespace FractalDataWorks.Services.Billing.Logging

# 6. Navigate to abstractions
cd ../FractalDataWorks.Services.Billing.Abstractions/Commands

# 7. Add commands
dotnet new fractaldataworks-command -n ProcessPayment --DomainName Billing --namespace FractalDataWorks.Services.Billing.Abstractions.Commands
dotnet new fractaldataworks-command -n RefundPayment --DomainName Billing --namespace FractalDataWorks.Services.Billing.Abstractions.Commands

# 8. Add messages
cd ../Messages
dotnet new fractaldataworks-message -n PaymentProcessed --DomainName Billing --namespace FractalDataWorks.Services.Billing.Abstractions.Messages
dotnet new fractaldataworks-message -n PaymentFailed --DomainName Billing --namespace FractalDataWorks.Services.Billing.Abstractions.Messages

# 9. Build everything
cd ../../..
dotnet build
```

## Troubleshooting

### Templates Not Found
```powershell
dotnet new uninstall FractalDataWorks.Service.Domain.Template
dotnet new install templates/FractalDataWorks.Service.Domain
```

### Source Generators Not Working
1. Clean and rebuild: `dotnet clean && dotnet build`
2. Check analyzer references have `OutputItemType="Analyzer"`
3. Enable diagnostics: Add `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` to csproj
4. Check generated files in `obj/{framework}/generated/`

### Build Errors
1. Run `dotnet restore` first
2. Ensure all using statements are explicit
3. Check target frameworks match
4. Verify source generator DLLs exist

## Next Steps After Template Creation

1. **Update TODO comments** - Replace placeholder implementations
2. **Add domain methods** - Implement business logic in service classes
3. **Configure ServiceType IDs** - Replace placeholder IDs with deterministic GUIDs
4. **Add unit tests** - Create test project matching src structure
5. **Configure logging** - Add domain-specific log events
6. **Add validation** - Implement FluentValidation for configurations
7. **Document** - Add XML documentation to public APIs

## Data Architecture Templates

### DataCommand Template

Creates a new DataCommand for universal data operations.

**Parameters:**
- `CommandName`: Name of the command (e.g., Upsert, BulkInsert)
- `UniqueId`: Unique integer ID (must be unique across all DataCommands)
- `ResultType`: Return type (default: int)
- `InputType`: Input data type (default: T)
- `Category`: Query/Insert/Update/Delete (default: Query)
- `HasInputData`: true if command takes input data (default: false)
- `HasFilter`: true to include Filter property (default: false)
- `HasProjection`: true to include Projection property (default: false)
- `HasOrdering`: true to include Ordering property (default: false)
- `HasPaging`: true to include Paging property (default: false)

**Example:**
```powershell
dotnet new fractaldataworks-datacommand -n Upsert --UniqueId 5 --Category Insert --HasInputData true --HasFilter true --ResultType int --InputType T
```

Generates: `UpsertCommand.cs` with TypeOption attribute, proper base class, and selected expression properties.

### DataTransformer Template

Creates a new DataTransformer for ETL operations.

**Parameters:**
- `TransformerName`: Name of the transformer (e.g., PayPalToTransaction)
- `UniqueId`: Unique integer ID (must be unique across all DataTransformers)
- `InputType`: Input type (default: object)
- `OutputType`: Output type (default: object)
- `IsGenericTransformer`: true for generic types (default: false)
- `SupportsStreaming`: true for single-record transformation (default: true)
- `SupportsParallel`: true if thread-safe (default: true)

**Example:**
```powershell
dotnet new fractaldataworks-transformer -n PayPalToTransaction --UniqueId 1 --InputType PayPalTransaction --OutputType Transaction --SupportsStreaming true
```

Generates: `PayPalToTransactionTransformer.cs` with TypeOption attribute, Transform method, and optional streaming support.

### DataConcept Configuration Template

Creates a JSON configuration file for a DataConcept (logical data abstraction).

**Parameters:**
- `ConceptName`: Name of the concept (e.g., TransactionData)
- `RecordTypeName`: .NET type name (e.g., MyApp.Models.Transaction)
- `Description`: Description of the concept (default: "Data concept description")
- `IncludeMultipleSources`: true for federated sources (default: true)
- `IncludeTransformer`: true to include transformer config (default: true)
- `IncludeDestination`: true for ETL destination (default: false)

**Example:**
```powershell
dotnet new fractaldataworks-dataconcept -n TransactionData --RecordTypeName MyApp.Models.Transaction --Description "Unified transaction data from multiple providers" --IncludeMultipleSources true --IncludeDestination true
```

Generates: `transactiondata.json` with schema, sources, transformers, and optional ETL destination configuration.

## Complete Data Architecture Example

### Scenario: Multi-Provider Transaction ETL

```powershell
# 1. Create DataConcept configuration
dotnet new fractaldataworks-dataconcept -n TransactionData --RecordTypeName MyApp.Models.Transaction --IncludeMultipleSources true --IncludeDestination true

# 2. Create transformers for each source
dotnet new fractaldataworks-transformer -n PayPalToTransaction --UniqueId 1 --InputType PayPalTransaction --OutputType Transaction
dotnet new fractaldataworks-transformer -n StripeToTransaction --UniqueId 2 --InputType StripeCharge --OutputType Transaction

# 3. Create enrichment transformer
dotnet new fractaldataworks-transformer -n EnrichWithCustomerData --UniqueId 3 --InputType Transaction --OutputType EnrichedTransaction

# 4. Create custom commands if needed
dotnet new fractaldataworks-datacommand -n BulkUpsert --UniqueId 10 --Category Insert --HasInputData true --ResultType BulkUpsertResult --InputType "IEnumerable<T>"

# 5. Build and run ETL
dotnet build
dotnet run --project MyApp.ETL
```

This creates a complete ETL pipeline:
- TransactionData concept queries PayPal + Stripe APIs
- Transformers normalize to common Transaction type
- Enrichment adds customer data
- Bulk upsert loads to data warehouse

## Support

- Framework conventions: See `CLAUDE.md` in solution root
- Source generator issues: Check `obj/` folder for generated files
- Template issues: Verify .NET 10 RC SDK installed
- Data architecture: See `discussions/data/` for complete architecture docs
