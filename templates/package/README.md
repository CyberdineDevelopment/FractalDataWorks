# FractalDataWorks Templates

Complete project and solution templates for FractalDataWorks backend data platform development.

## Templates Included

### Host Applications
- **API Host** (`Rec-api`) - RESTful API with authentication, rate limiting, and external connections
- **ETL Host** (`Rec-etl`) - Data processing pipeline with transformations and batch/stream processing  
- **Scheduler Host** (`Rec-scheduler`) - Background job scheduling with distributed task execution

### Components
- **Service** (`Rec-service`) - Generic FractalDataWorks service with dependency injection
- **External Connection** (`Rec-connection`) - External system integration components

## Installation

### Option 1: NuGet Package (Recommended)
```bash
dotnet new install FractalDataWorks.Templates
```

### Option 2: Local Installation
```bash
dotnet new install path/to/templates/package
```

## Usage

### Create a new API Host
```bash
# Basic API host
dotnet new Rec-api -n MyApi

# API with Azure AD authentication and SQL Server
dotnet new Rec-api -n MyApi --authentication AzureAD --database SqlServer

# API with Docker support
dotnet new Rec-api -n MyApi --docker true
```

### Create a new ETL Host
```bash
# Basic ETL host
dotnet new Rec-etl -n MyEtl

# ETL with batch processing and database source
dotnet new Rec-etl -n MyEtl --processing Batch --data-source Database
```

### Create a new Scheduler Host
```bash
# Basic scheduler with Quartz.NET
dotnet new Rec-scheduler -n MyScheduler --scheduler Quartz

# In-memory scheduler for development
dotnet new Rec-scheduler -n MyScheduler --scheduler InMemory
```

### Create a Service Component
```bash
dotnet new Rec-service -n MyCustomService
```

### Create an External Connection
```bash
# HTTP connection
dotnet new Rec-connection -n MyApiConnection --connection-type Http

# Database connection
dotnet new Rec-connection -n MyDbConnection --connection-type Database
```

## Template Parameters

### Common Parameters
- `--authentication` / `-auth`: Authentication type (None, ApiKey, JWT, AzureAD)
- `--database` / `-db`: Database provider (None, SqlServer, PostgreSQL, MySQL, SQLite, CosmosDB)
- `--docker` / `-d`: Include Docker support (true/false)
- `--health-checks` / `-hc`: Include health check endpoints (true/false)

### API Host Specific
- `--rate-limiting`: Include rate limiting middleware (true/false)
- `--cors`: Include CORS support (true/false)
- `--swagger`: Include Swagger/OpenAPI documentation (true/false)

### ETL Host Specific
- `--processing` / `-pt`: Processing type (Batch, Stream, Both)
- `--data-source` / `-ds`: Primary data source (Database, Files, Api)
- `--transformations`: Include transformation pipeline (true/false)

### Scheduler Host Specific
- `--scheduler` / `-s`: Scheduler implementation (InMemory, Quartz, Hangfire)
- `--persistence`: Include job persistence (true/false)
- `--distributed`: Include distributed execution (true/false)

### External Connection Specific
- `--connection-type` / `-ct`: Connection type (Http, Database, MessageQueue)
- `--retry-policy`: Include retry policies (true/false)
- `--circuit-breaker`: Include circuit breaker pattern (true/false)

## Advanced Usage

### Creating Full Solutions
The templates can be combined to create complete solutions:

```bash
# Create solution directory
mkdir MyDataPlatform
cd MyDataPlatform

# Initialize solution
dotnet new sln -n MyDataPlatform

# Add API host
dotnet new Rec-api -n MyDataPlatform.Api --authentication AzureAD --database SqlServer
dotnet sln add MyDataPlatform.Api

# Add ETL host
dotnet new Rec-etl -n MyDataPlatform.Etl --processing Both --data-source Database
dotnet sln add MyDataPlatform.Etl

# Add scheduler host
dotnet new Rec-scheduler -n MyDataPlatform.Scheduler --scheduler Quartz
dotnet sln add MyDataPlatform.Scheduler

# Add shared services
dotnet new Rec-service -n MyDataPlatform.Core
dotnet sln add MyDataPlatform.Core
```

### Custom Configuration
Templates support extensive customization through parameters. For complex scenarios, you can:

1. Generate with basic parameters
2. Customize generated code for your specific needs
3. Add additional NuGet packages as required
4. Configure environment-specific settings

## Integration with Visual Studio

The templates integrate with Visual Studio through the FractalDataWorks Templates Extension (VSIX), providing:

- **Solution Wizard**: GUI-driven template selection and configuration
- **Project Templates**: Available in Visual Studio's New Project dialog
- **IntelliSense Support**: Full code completion and syntax highlighting
- **Integrated Debugging**: Built-in debugging support for generated projects

## Package Contents

- **Templates**: Complete project templates with all necessary files
- **Configuration**: Template configuration and metadata
- **Dependencies**: Automatic NuGet package management
- **Documentation**: Comprehensive usage guides and examples
- **Tools**: Installation and management scripts

## Requirements

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** (for VSIX integration)
- **Docker Desktop** (optional, for containerization features)

## Support

For issues, questions, or contributions:
- **Repository**: https://github.com/FractalDataWorks/samples-implementation
- **Issues**: https://github.com/FractalDataWorks/samples-implementation/issues
- **Documentation**: https://github.com/FractalDataWorks/samples-implementation/tree/master/docs

## License

MIT License - see LICENSE file for details.

## Version History

### 1.0.0
- Initial release
- API Host template with authentication options
- ETL Host template with processing modes
- Scheduler Host template with multiple backends
- Service and Connection component templates
- Visual Studio integration
- Comprehensive parameter support
- Docker containerization support
- CI/CD pipeline templates