# DataPath Forward - Implementation Guide

## Overview

This worktree implements a **simple, composable data architecture** that enables:
- Querying data concepts without knowing underlying sources
- ETL at scale with streaming, batching, and parallel processing
- Static (compiled) and configurable (JSON) transformers
- Multi-source federation (SQL + REST + Files unified)
- Hierarchical and aggregated data views

**Core Principle**: Simple primitives that compose to create power.

## CRITICAL: Read This First

📖 **READ**: `datapathforward.txt` - Complete conversation transcript with all design decisions, ultrathink analysis, and architectural rationale.

This document summarizes implementation steps, but the exported conversation contains the full context and reasoning.

## What We're Building

### The Vision: TransactionData Example

```csharp
// User queries "TransactionData" concept
var transactions = await DataConcepts
    .Query<Transaction>("TransactionData")
    .Where(t => t.Date >= DateTime.Today.AddDays(-30))
    .Where(t => t.Amount > 100)
    .Execute(connectionProvider, ct);

// Behind the scenes:
// - Queries PayPal API → transforms to Transaction
// - Queries Stripe API → transforms to Transaction
// - Queries SQL database → already Transaction format
// - Unions all results
// - Returns unified view

// User never knows or cares about:
// - Multiple sources
// - Different schemas
// - Different connection types
// - How transformations happen
```

### Key Capabilities

1. **Data Concepts**: Logical abstraction over physical sources
2. **Multi-Source**: One concept can span SQL + REST + Files
3. **Transformers**: Static (code) OR configurable (JSON)
4. **DataCommands**: Full CRUD + Bulk operations
5. **Scale**: Streaming, batching, parallel processing (TPL)
6. **Composability**: Join concepts, aggregate, filter, transform

## Current State

### What EXISTS ✅

```
FractalDataWorks.Commands.Abstractions/
├── IGenericCommand                    ✅ Base for all commands
├── CommandTypes (TypeCollection)      ✅ Discoverable types

FractalDataWorks.Data.DataSets.Abstractions/
├── IDataQuery<T>                      ✅ Query interface with LINQ
├── ComparisonOperators                ✅ TypeCollection (Equal, Contains, etc.)
├── LogicalOperators                   ✅ TypeCollection (And, Or)
├── DataSetConfiguration               ✅ Multi-source configuration

FractalDataWorks.Services.Connections.*/
├── IGenericConnection                 ✅ Connection abstraction
├── ConnectionTypes                    ✅ SQL, REST, File connections

FractalDataWorks.Data.DataContainers.Abstractions/
├── IDataContainer                     ✅ Physical container access
├── IDataReader/IDataWriter            ✅ Read/write operations
```

### What's MISSING ❌

```
DataCommands TypeCollection            ❌ Need Query, Insert, Update, Delete, Merge, BulkInsert
DataConceptQueryExecutor               ❌ Execute queries against concepts
Transformers Infrastructure            ❌ Static + configurable transformers
Streaming Support                      ❌ IAsyncEnumerable for large datasets
Parallel Execution                     ❌ TPL integration for scale
```

## Implementation Milestones

### Milestone 1: Data Concepts with Multi-Source Query 🎯 START HERE

**Goal**: Query a data concept that spans multiple sources and get unified results.

**What to Build**:

1. **Extend DataSetConfiguration** (if needed)
   - Already has `Sources` dictionary ✅
   - Add `SourceMappingConfiguration` if missing
   - File: `src/FractalDataWorks.Data.DataSets.Abstractions/SourceMappingConfiguration.cs`

2. **Create DataConceptRegistry**
   - Loads concepts from configuration
   - File: `src/FractalDataWorks.Data.DataSets.Abstractions/IDataConceptRegistry.cs`
   - Implementation: `src/FractalDataWorks.Data.DataSets/DataConceptRegistry.cs`

3. **Create DataConceptQueryExecutor**
   - Executes IDataQuery against multiple sources
   - Unions results
   - File: `src/FractalDataWorks.Data.Execution/DataConceptQueryExecutor.cs`

4. **Simple Transformer Interface**
   - `IDataTransformer` with Transform method
   - File: `src/FractalDataWorks.Data.Transformers.Abstractions/IDataTransformer.cs`

5. **Demo Application**
   - Configure TransactionData concept with 2 sources
   - Query and verify unified results
   - File: `samples/DataConceptDemo/Program.cs`

**Files to Create** (~500 lines total):
```
src/FractalDataWorks.Data.Execution/
├── DataConceptQueryExecutor.cs        (~150 lines)
├── QueryCommandBuilder.cs             (~50 lines)

src/FractalDataWorks.Data.Transformers.Abstractions/
├── IDataTransformer.cs                (~30 lines)
├── TransformerBase.cs                 (~40 lines)
├── DataTransformers.cs                (~20 lines - TypeCollection)

src/FractalDataWorks.Data.DataSets/
├── DataConceptRegistry.cs             (~80 lines)

samples/DataConceptDemo/
├── Program.cs                         (~100 lines)
├── Models/Transaction.cs              (~20 lines)
├── Transformers/MockTransformer.cs    (~40 lines)
├── appsettings.json                   (~30 lines)
```

**Commit When**:
- ✅ DataConceptRegistry loads configuration
- ✅ DataConceptQueryExecutor queries single source successfully
- ✅ DataConceptQueryExecutor unions multiple sources
- ✅ Demo runs end-to-end

**Commit Message**:
```
Milestone 1: Multi-source data concepts

- Add DataConceptRegistry for configuration loading
- Implement DataConceptQueryExecutor for multi-source queries
- Add IDataTransformer interface
- Create TransactionData demo with 2 sources
- Unified query returns results from both sources

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] Query "TransactionData" concept
- [ ] Concept has 2 sources (mocked connections)
- [ ] Executor queries both sources
- [ ] Results are unioned
- [ ] User code doesn't reference sources

---

### Milestone 2: Static Transformers with TypeCollection

**Goal**: Add compiled, type-safe transformers as TypeOptions.

**What to Build**:

1. **TransformerBase Class**
   - Generic `TransformerBase<TIn, TOut>`
   - Constructor-based properties (id, name)
   - File: `src/FractalDataWorks.Data.Transformers.Abstractions/TransformerBase.cs`

2. **DataTransformers TypeCollection**
   - `[TypeCollection]` for discoverable transformers
   - File: `src/FractalDataWorks.Data.Transformers.Abstractions/DataTransformers.cs`

3. **Sample Static Transformers**
   - PayPalToTransactionTransformer
   - StripeToTransactionTransformer
   - File: `samples/DataConceptDemo/Transformers/`

4. **TransformerRegistry**
   - Lookup transformers by name
   - File: `src/FractalDataWorks.Data.Transformers/TransformerRegistry.cs`

5. **Integrate with Executor**
   - Use transformer from configuration
   - Apply to source results

**Files to Create** (~300 lines):
```
src/FractalDataWorks.Data.Transformers.Abstractions/
├── TransformerBase{TIn,TOut}.cs       (~60 lines)
├── TransformerContext.cs              (~30 lines)

src/FractalDataWorks.Data.Transformers/
├── TransformerRegistry.cs             (~60 lines)

samples/DataConceptDemo/Transformers/
├── PayPalToTransactionTransformer.cs  (~50 lines)
├── StripeToTransactionTransformer.cs  (~50 lines)
```

**Commit When**:
- ✅ TransformerBase compiles
- ✅ TypeCollection generates DataTransformers.All()
- ✅ TransformerRegistry finds transformers by name
- ✅ Executor applies transformer to source data
- ✅ Demo shows different schemas normalized

**Commit Message**:
```
Milestone 2: Static type-safe transformers

- Add TransformerBase<TIn,TOut> with TypeCollection
- Implement DataTransformers TypeCollection pattern
- Create PayPal and Stripe transformers
- Integrate transformers with DataConceptQueryExecutor
- Demo normalizes different schemas to common Transaction type

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] DataTransformers.All() returns static transformers
- [ ] DataTransformers.GetByName("PayPalToTransaction") works
- [ ] Executor applies transformer based on config
- [ ] Different source schemas normalized to Transaction

---

### Milestone 3: DataCommands TypeCollection with CRUD

**Goal**: Expand DataCommands for full CRUD operations.

**What to Build**:

1. **Extend DataCommands TypeCollection**
   - Add Insert, Update, Delete, BulkInsert, Merge
   - File: `src/FractalDataWorks.Data.Abstractions/DataCommands.cs`

2. **Command Implementations**
   - QueryCommand (expand existing)
   - InsertCommand
   - UpdateCommand
   - DeleteCommand
   - BulkInsertCommand
   - MergeCommand
   - Files: `src/FractalDataWorks.Data/Commands/`

3. **Expression Components**
   - FilterExpression, ProjectionExpression, etc.
   - Files: `src/FractalDataWorks.Data.Abstractions/Expressions/`

4. **Connection Execute Overloads**
   - Handle different command types
   - Files: Update existing connections

**Files to Create/Modify** (~800 lines):
```
src/FractalDataWorks.Data/Commands/
├── QueryCommand.cs                    (~100 lines)
├── InsertCommand.cs                   (~60 lines)
├── UpdateCommand.cs                   (~70 lines)
├── DeleteCommand.cs                   (~50 lines)
├── BulkInsertCommand.cs               (~80 lines)
├── MergeCommand.cs                    (~90 lines)

src/FractalDataWorks.Data.Abstractions/Expressions/
├── FilterExpression.cs                (~60 lines)
├── ProjectionExpression.cs            (~50 lines)
├── OrderingExpression.cs              (~40 lines)
├── PagingExpression.cs                (~30 lines)
```

**Commit When**:
- ✅ All command types defined as TypeOptions
- ✅ DataCommands.Query, .Insert, .Update, etc. accessible
- ✅ At least one connection implements all commands
- ✅ Demo shows CRUD operations

**Commit Message**:
```
Milestone 3: Full CRUD DataCommands TypeCollection

- Expand DataCommands with Insert, Update, Delete, Merge
- Add BulkInsert for ETL scenarios
- Implement expression components (Filter, Projection, etc.)
- Update connections to handle all command types
- Demo shows full CRUD lifecycle

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] DataCommands TypeCollection has 6+ command types
- [ ] Can query, insert, update, delete via commands
- [ ] BulkInsert supports batching
- [ ] Merge supports upsert semantics

---

### Milestone 4: Streaming and Scale (TPL Integration)

**Goal**: Add streaming, batching, and parallel execution for ETL at scale.

**What to Build**:

1. **Streaming Support**
   - `IAsyncEnumerable<T>` support in connections
   - `ExecuteStream` method
   - File: Extend `IGenericConnection`

2. **Parallel Extraction**
   - Task.WhenAll for multiple sources
   - File: Extend `DataConceptQueryExecutor`

3. **Parallel Loading**
   - Parallel.ForEachAsync for batches
   - File: New `ParallelDataLoader.cs`

4. **Channel-Based Pipeline**
   - Producer-consumer with backpressure
   - File: `src/FractalDataWorks.Data.Execution/ChannelExecutor.cs`

5. **TPL Dataflow Option**
   - Optional complex pipeline
   - File: `src/FractalDataWorks.Data.Execution/DataflowExecutor.cs`

6. **Configuration Options**
   - Parallel settings in configuration
   - BatchSize, MaxDegreeOfParallelism, etc.

**Files to Create** (~600 lines):
```
src/FractalDataWorks.Data.Execution/
├── StreamingExecutor.cs               (~120 lines)
├── ParallelDataLoader.cs              (~100 lines)
├── ChannelExecutor.cs                 (~150 lines)
├── DataflowExecutor.cs                (~180 lines)

samples/ETLDemo/
├── Program.cs                         (~100 lines)
├── LargeDatasetSimulator.cs           (~50 lines)
```

**Commit When**:
- ✅ ExecuteStream returns IAsyncEnumerable
- ✅ Parallel extraction works (Task.WhenAll)
- ✅ Parallel loading works (Parallel.ForEachAsync)
- ✅ Channel-based pipeline demonstrates backpressure
- ✅ Performance test shows 3x+ speedup

**Commit Message**:
```
Milestone 4: Streaming and parallel execution for scale

- Add IAsyncEnumerable streaming support
- Implement parallel extraction with Task.WhenAll
- Add parallel batch loading with Parallel.ForEachAsync
- Create channel-based pipeline with backpressure
- Add TPL Dataflow option for complex ETL
- Performance: 3x speedup on 1M record demo

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] Stream 1M+ records without loading all to memory
- [ ] Extract from 3 sources in parallel (not sequential)
- [ ] Load batches in parallel (4+ concurrent)
- [ ] Channel pipeline handles backpressure
- [ ] Measurable performance improvement

---

### Milestone 5: Configurable Transformers (JSON Pipeline)

**Goal**: Add JSON-configured transformers for dynamic scenarios.

**What to Build**:

1. **Pipeline Configuration Model**
   - Field mapping, value mapping, calculations, filters
   - File: `src/FractalDataWorks.Data.Transformers.Abstractions/PipelineConfiguration.cs`

2. **Configurable Transformer**
   - Applies pipeline steps
   - File: `src/FractalDataWorks.Data.Transformers/ConfigurableTransformer.cs`

3. **Pipeline Steps**
   - FieldMappingStep
   - ValueMappingStep
   - CalculationStep
   - FilterStep
   - DefaultValueStep
   - Files: `src/FractalDataWorks.Data.Transformers/Steps/`

4. **Expression Evaluator**
   - Evaluate simple expressions from JSON
   - File: `src/FractalDataWorks.Data.Transformers/ExpressionEvaluator.cs`

5. **Demo**
   - Configure legacy system transformer via JSON
   - Files: `samples/DataConceptDemo/transformers.json`

**Files to Create** (~500 lines):
```
src/FractalDataWorks.Data.Transformers.Abstractions/
├── PipelineConfiguration.cs           (~80 lines)
├── PipelineStep.cs                    (~40 lines)

src/FractalDataWorks.Data.Transformers/
├── ConfigurableTransformer.cs         (~100 lines)
├── ExpressionEvaluator.cs             (~80 lines)
├── Steps/
│   ├── FieldMappingStep.cs            (~40 lines)
│   ├── ValueMappingStep.cs            (~40 lines)
│   ├── CalculationStep.cs             (~50 lines)
│   ├── FilterStep.cs                  (~30 lines)
│   └── DefaultValueStep.cs            (~30 lines)

samples/DataConceptDemo/
├── transformers.json                  (~50 lines)
```

**Commit When**:
- ✅ Pipeline configuration loads from JSON
- ✅ ConfigurableTransformer applies steps in order
- ✅ Field mapping works
- ✅ Value mapping works
- ✅ Simple calculations work (Amount * 0.029)
- ✅ Demo shows JSON-configured transformer

**Commit Message**:
```
Milestone 5: JSON-configurable transformers

- Add PipelineConfiguration for JSON-defined transforms
- Implement ConfigurableTransformer with steps
- Add field mapping, value mapping, calculations, filters
- Add simple expression evaluator
- Demo shows legacy system normalized via JSON config

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] Load transformer pipeline from JSON
- [ ] Field mapping renames fields
- [ ] Value mapping converts codes
- [ ] Calculations evaluate expressions
- [ ] Filters remove unwanted records

---

### Milestone 6: Complete ETL Demo

**Goal**: End-to-end ETL pipeline using all features.

**What to Build**:

1. **Multi-Source ETL Configuration**
   - Extract from 3+ sources
   - Transform with static + configurable
   - Load to data warehouse
   - File: `samples/CompleteETLDemo/appsettings.json`

2. **ETL Orchestrator**
   - Coordinates extract, transform, load
   - Progress reporting
   - Error handling
   - File: `samples/CompleteETLDemo/ETLOrchestrator.cs`

3. **Monitoring and Metrics**
   - ETLStatistics (records processed, failed, rate)
   - Progress<ETLProgress> reporting
   - File: `src/FractalDataWorks.Data.Execution/ETLStatistics.cs`

4. **Demo Scenarios**
   - Daily sales ETL
   - Federated customer analytics
   - Hierarchical organization data
   - Files: `samples/CompleteETLDemo/Scenarios/`

**Files to Create** (~400 lines):
```
samples/CompleteETLDemo/
├── ETLOrchestrator.cs                 (~150 lines)
├── Program.cs                         (~80 lines)
├── appsettings.json                   (~100 lines)
├── Scenarios/
│   ├── DailySalesETL.cs               (~60 lines)
│   ├── CustomerAnalyticsETL.cs        (~50 lines)

src/FractalDataWorks.Data.Execution/
├── ETLStatistics.cs                   (~40 lines)
├── ETLProgress.cs                     (~30 lines)
```

**Commit When**:
- ✅ Complete ETL runs end-to-end
- ✅ Extracts from multiple sources
- ✅ Transforms with both static and configurable
- ✅ Loads with merge (upsert)
- ✅ Progress reporting works
- ✅ Error handling recovers gracefully
- ✅ Performance meets target (configurable)

**Commit Message**:
```
Milestone 6: Complete ETL pipeline demo

- Add ETL orchestrator with extract-transform-load
- Implement progress reporting and metrics
- Add daily sales ETL scenario
- Add federated customer analytics scenario
- Demo runs at scale with streaming and parallelism
- Error handling with retry and dead letter queue

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria**:
- [ ] ETL processes 100K+ records successfully
- [ ] Multiple sources unified into single destination
- [ ] Static transformers perform complex logic
- [ ] Configurable transformers handle dynamic mapping
- [ ] Parallel execution provides 3x+ speedup
- [ ] Failures handled gracefully with retry

---

## Documentation Requirements

### Per Milestone

After each milestone, add/update these documents:

1. **Architecture Diagram**
   - Mermaid diagram in `docs/architecture/milestone-N.md`
   - Show components added in this milestone

2. **API Documentation**
   - XML doc comments on all public interfaces
   - Usage examples in doc comments

3. **Configuration Guide**
   - Document new configuration options
   - File: `docs/configuration/milestone-N.md`

4. **Migration Notes**
   - Breaking changes (if any)
   - How to upgrade from previous milestone

### Final Documentation

After Milestone 6, create comprehensive docs:

1. **Getting Started Guide**
   - File: `docs/GettingStarted.md`
   - 5-minute tutorial
   - TransactionData example

2. **Concepts Guide**
   - File: `docs/Concepts.md`
   - Data concepts explained
   - Multi-source configuration
   - When to use vs. not use

3. **Transformers Guide**
   - File: `docs/Transformers.md`
   - Static vs. configurable
   - When to use each
   - How to create custom

4. **ETL Guide**
   - File: `docs/ETL.md`
   - Extract-Transform-Load patterns
   - Scale considerations
   - Performance tuning

5. **API Reference**
   - File: `docs/API.md`
   - All interfaces documented
   - Code examples for each

## Commit Guidelines

### When to Commit

✅ **DO commit** when:
- Milestone completion criteria met
- Tests pass
- Code compiles without warnings (Alpha config minimum)
- Demo application runs successfully
- XML doc comments added to public APIs

❌ **DON'T commit** when:
- Work in progress (use feature branches)
- Tests failing
- Breaking existing functionality without migration path
- Missing documentation

### Commit Message Format

```
<Type>: <Short summary (50 chars max)>

<Detailed description of changes (72 chars per line)>
- Bullet points for key changes
- What was added
- What was modified
- Why changes were made

<Optional: Breaking changes section>
BREAKING CHANGE: Description of what breaks

<Optional: Related issues>
Fixes #123
Relates to #456

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Commit Types**:
- `feat:` - New feature (milestone completion)
- `fix:` - Bug fix
- `refactor:` - Code restructure without behavior change
- `docs:` - Documentation only
- `test:` - Adding tests
- `perf:` - Performance improvement
- `chore:` - Maintenance (dependency updates, etc.)

### Example Commits

**Good**:
```
feat: Add multi-source data concept query execution

Implement DataConceptQueryExecutor that queries multiple configured
sources and unions results into a single unified view.

- Add DataConceptRegistry for loading concept configurations
- Implement QueryExecutor with multi-source support
- Add transformer integration for schema normalization
- Create TransactionData demo with 2 sources

Demo shows querying "TransactionData" concept that transparently
queries both PayPal API and SQL database, transforms to common
schema, and returns unified results.

Milestone 1 complete.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Bad**:
```
updated stuff
```

```
WIP - working on transformers but not done yet
```

## Testing Strategy

### Per Milestone

1. **Unit Tests**
   - Test each component in isolation
   - Mock dependencies
   - File: `tests/FractalDataWorks.Data.Execution.Tests/`

2. **Integration Tests**
   - Test with real connections (if available)
   - Or use in-memory test doubles
   - File: `tests/Integration/`

3. **Performance Tests**
   - Benchmark critical paths
   - Verify parallelism improvements
   - File: `tests/Performance/`

### Testing Checklist

Before committing a milestone:
- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Performance tests meet targets
- [ ] Demo application runs without errors
- [ ] No compiler warnings in Alpha config

## Project Structure

```
Developer-Kit-datapath/
├── src/
│   ├── FractalDataWorks.Data.Execution/          ← NEW
│   │   ├── DataConceptQueryExecutor.cs
│   │   ├── StreamingExecutor.cs
│   │   ├── ParallelDataLoader.cs
│   │   └── ChannelExecutor.cs
│   │
│   ├── FractalDataWorks.Data.Transformers.Abstractions/  ← NEW
│   │   ├── IDataTransformer.cs
│   │   ├── TransformerBase.cs
│   │   ├── DataTransformers.cs
│   │   └── PipelineConfiguration.cs
│   │
│   ├── FractalDataWorks.Data.Transformers/       ← NEW
│   │   ├── TransformerRegistry.cs
│   │   ├── ConfigurableTransformer.cs
│   │   └── Steps/
│   │
│   ├── FractalDataWorks.Data/                    ← EXTEND
│   │   └── Commands/                             ← NEW
│   │       ├── QueryCommand.cs
│   │       ├── InsertCommand.cs
│   │       ├── BulkInsertCommand.cs
│   │       └── MergeCommand.cs
│   │
│   └── (existing projects)
│
├── samples/
│   ├── DataConceptDemo/                          ← NEW
│   ├── ETLDemo/                                  ← NEW
│   └── CompleteETLDemo/                          ← NEW
│
├── tests/
│   ├── FractalDataWorks.Data.Execution.Tests/    ← NEW
│   └── Integration/                               ← NEW
│
├── docs/
│   ├── architecture/
│   │   ├── milestone-1.md
│   │   ├── milestone-2.md
│   │   └── ...
│   ├── GettingStarted.md
│   ├── Concepts.md
│   ├── Transformers.md
│   ├── ETL.md
│   └── API.md
│
├── DATAPATH_FORWARD.md                           ← This file
├── datapathforward.txt                           ← Conversation export
└── README.md                                     ← Update with status
```

## Key Design Principles (From Conversation)

1. **Simple Primitives**: Each component is trivially simple
2. **Composability**: Power emerges from composition, not complexity
3. **Type Safety**: Generics everywhere, compiler catches errors
4. **Configuration-Driven**: JSON defines concepts, sources, transformers
5. **Railway-Oriented**: All operations return IGenericResult<T>
6. **TypeCollections**: Use for all fixed sets (DataCommands, Transformers)
7. **No Async Suffix**: Methods don't end with "Async"
8. **Immutability**: Operations return new objects, no mutation
9. **Deferred Execution**: Build queries, execute only when needed
10. **Scale by Default**: Streaming and parallelism built-in

## Common Patterns

### Adding a New Command Type

```csharp
// 1. Define command class
[TypeOption(typeof(DataCommands), "CustomCommand")]
public sealed class CustomCommand : DataCommandBase
{
    public CustomCommand(string containerName)
        : base(id: 10, name: "CustomCommand", commandType: "Custom", containerName)
    {
    }

    // Add command-specific properties
    public string CustomProperty { get; init; } = string.Empty;
}

// 2. Update connection to handle it
public async Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken ct)
{
    return command switch
    {
        CustomCommand custom => HandleCustom(custom, ct),
        // ... other cases
    };
}

// 3. Source generator creates: DataCommands.CustomCommand
```

### Adding a New Transformer

```csharp
// Static transformer
[TypeOption(typeof(DataTransformers), "MyTransformer")]
public sealed class MyTransformer : TransformerBase<SourceType, TargetType>
{
    public MyTransformer()
        : base(id: 100, name: "MyTransformer")
    {
    }

    public override IGenericResult<IEnumerable<TargetType>> Transform(
        IEnumerable<SourceType> source,
        TransformContext context,
        CancellationToken ct)
    {
        var transformed = source.Select(s => new TargetType
        {
            // Mapping logic
        });

        return GenericResult<IEnumerable<TargetType>>.Success(transformed);
    }
}

// Source generator creates: DataTransformers.MyTransformer
```

## Progress Tracking

Update this section as milestones complete:

- [ ] Milestone 1: Multi-source data concepts
- [ ] Milestone 2: Static transformers
- [ ] Milestone 3: DataCommands CRUD
- [ ] Milestone 4: Streaming and scale
- [ ] Milestone 5: Configurable transformers
- [ ] Milestone 6: Complete ETL demo

## Questions / Decisions Log

Document key decisions and questions here:

### Decision: Transformer Architecture
**Question**: Should transformers be static (TypeCollection) or configurable (JSON) or both?
**Answer**: Both. Static for complex logic (compiled, fast), configurable for simple mapping (flexible, no rebuild).
**Date**: 2025-10-14

### Decision: Parallel Execution Strategy
**Question**: Which TPL pattern for parallel execution?
**Answer**: All of them - Task.WhenAll, Parallel.ForEachAsync, Channels, TPL Dataflow. Configuration chooses which.
**Date**: 2025-10-14

(Add more as implementation progresses)

## Resources

- **Conversation Export**: `datapathforward.txt` - Read this for full context
- **CLAUDE.md**: `../CLAUDE.md` - Project conventions and patterns
- **Architecture Discussions**: `../discussions/data/` - Related design docs
- **TypeCollection Docs**: `../src/FractalDataWorks.Collections/README.md`
- **ServiceTypes Docs**: `../src/FractalDataWorks.ServiceTypes/README.md`

## Next Steps

1. ✅ Read `datapathforward.txt` completely
2. ✅ Review existing code in referenced projects
3. ✅ Start Milestone 1: Multi-source data concepts
4. Create feature branch: `git checkout -b milestone-1-data-concepts`
5. Build DataConceptQueryExecutor
6. Create demo application
7. Test, document, commit when criteria met

---

**Remember**: Simple primitives. Composable power. Read the conversation export for full context.
