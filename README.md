# DataPath Forward - Simple, Composable Data Architecture

This worktree implements a unified data architecture where:
- Data concepts abstract over physical sources (SQL, REST, Files)
- Queries work without knowing underlying infrastructure
- ETL scales with streaming, batching, and parallelism
- Transformers can be static (compiled) or configurable (JSON)

## Quick Start

1. **Read First**: `DATAPATH_FORWARD.md` - Complete implementation guide with milestones
2. **Context**: `datapathforward.txt` - Full conversation with design decisions
3. **Status**: Milestone 1 - Multi-source data concepts (in progress)

## Current Status

```
Milestone 1: Multi-source data concepts        [ In Progress ]
Milestone 2: Static transformers               [ Not Started ]
Milestone 3: DataCommands CRUD                 [ Not Started ]
Milestone 4: Streaming and scale               [ Not Started ]
Milestone 5: Configurable transformers         [ Not Started ]
Milestone 6: Complete ETL demo                 [ Not Started ]
```

## The Vision

```csharp
// Query "TransactionData" - don't care it spans PayPal + Stripe + SQL
var transactions = await DataConcepts
    .Query<Transaction>("TransactionData")
    .Where(t => t.Date >= DateTime.Today.AddDays(-30))
    .Execute(connectionProvider, ct);

// Behind the scenes: queries 3 sources, transforms, unions results
```

## Architecture

```
Data Concepts (Logical)
    ↓
Multi-Source Configuration
    ↓
Query Executor (unions sources)
    ↓
Transformers (normalize schemas)
    ↓
Connections (SQL/REST/Files)
    ↓
Physical Storage
```

## Key Files

- `DATAPATH_FORWARD.md` - **START HERE** - Implementation guide with milestones
- `datapathforward.txt` - Full conversation export with design rationale
- `src/FractalDataWorks.Data.Execution/` - Query execution (Milestone 1)
- `src/FractalDataWorks.Data.Transformers.Abstractions/` - Transformer interfaces (Milestone 2)
- `samples/DataConceptDemo/` - Demo application

## Development

```powershell
# Create feature branch for current milestone
git checkout -b milestone-1-data-concepts

# Build
dotnet build -c Alpha

# Run tests
dotnet test

# Run demo
cd samples/DataConceptDemo
dotnet run
```

## Documentation

See `docs/` for architecture diagrams and guides (created per milestone).

## Contributing

Follow milestones in `DATAPATH_FORWARD.md`. Each milestone has:
- Clear goals
- Files to create
- Success criteria
- Commit guidelines

Read the conversation export for full context before starting.
