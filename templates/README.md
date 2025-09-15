# FractalDataWorks Project Templates

Complete Visual Studio solution wizard and installation packages for FractalDataWorks backend data platform development.

## 🚀 Quick Start

### Windows (Recommended)
```powershell
# Install templates and Visual Studio extension
.\install\install.ps1 -IncludeVsix

# Or just templates for CLI usage
.\install\install.ps1
```

### Linux/macOS
```bash
# Make scripts executable
chmod +x install/*.sh

# Install templates
./install/install.sh
```

## 📦 What's Included

### 1. Visual Studio Extension (VSIX)
**Location**: `templates/vsix/`

- **Solution Wizard**: GUI-driven template selection with comprehensive options
- **WPF Dialog**: Professional interface for configuring:
  - Solution type (API, ETL, Scheduler, Full Stack)
  - Authentication (None, API Key, JWT, Azure AD)
  - Database providers (SQL Server, PostgreSQL, MySQL, SQLite, Cosmos DB)
  - Docker containerization
  - CI/CD pipeline integration
  - Secret management options

- **Project Integration**: Seamless Visual Studio experience with:
  - IntelliSense support
  - Integrated debugging
  - Automatic NuGet package management
  - Solution folder organization

### 2. NuGet Template Package
**Location**: `templates/package/`

Complete `dotnet new` integration with:
- **Template Registry**: JSON-based template catalog
- **Parameter Validation**: Type-safe template parameters
- **Dependency Management**: Automatic package installation
- **Multi-platform Support**: Windows, Linux, macOS compatibility

### 3. Installation System
**Location**: `templates/install/`

Cross-platform installation with:
- **PowerShell Script**: `install.ps1` for Windows
- **Bash Script**: `install.sh` for Linux/macOS
- **Uninstall Support**: Complete removal scripts
- **Version Management**: Support for specific versions and prereleases

### 4. Template Catalog
**Location**: `templates/catalog/`

Comprehensive template organization:
- **templates.json**: Master template registry
- **categories.json**: Template categorization system
- **dependencies.json**: Package dependency management

## 🛠 Available Templates

### Host Applications
| Template | Command | Description |
|----------|---------|-------------|
| **API Host** | `Rec-api` | RESTful API with authentication, rate limiting, and external connections |
| **ETL Host** | `Rec-etl` | Data processing pipeline with transformations and batch/stream processing |
| **Scheduler Host** | `Rec-scheduler` | Background job scheduling with distributed task execution |

### Components
| Template | Command | Description |
|----------|---------|-------------|
| **Service** | `Rec-service` | Generic service component with dependency injection |
| **External Connection** | `Rec-connection` | External system integration components |

## 💻 Usage Examples

### Visual Studio (GUI)
1. Install VSIX: `.\install\install.ps1 -IncludeVsix`
2. Open Visual Studio
3. File → New → Project
4. Search "FractalDataWorks"
5. Follow the solution wizard

### Command Line Interface

#### API Host Examples
```bash
# Basic API host
dotnet new Rec-api -n MyApi

# API with Azure AD authentication and SQL Server
dotnet new Rec-api -n MyApi --authentication AzureAD --database SqlServer

# Full-featured API with Docker
dotnet new Rec-api -n MyApi \
  --authentication JWT \
  --database SqlServer \
  --docker true \
  --health-checks true
```

#### ETL Host Examples
```bash
# Batch processing ETL
dotnet new Rec-etl -n MyEtl --processing Batch --data-source Database

# Stream processing ETL
dotnet new Rec-etl -n MyEtl --processing Stream --data-source Api
```

#### Scheduler Host Examples
```bash
# Quartz.NET scheduler
dotnet new Rec-scheduler -n MyScheduler --scheduler Quartz

# Development scheduler
dotnet new Rec-scheduler -n MyScheduler --scheduler InMemory
```

#### Full Solution Example
```bash
# Create complete solution
mkdir MyDataPlatform && cd MyDataPlatform

# Initialize solution
dotnet new sln -n MyDataPlatform

# Add API host
dotnet new Rec-api -n MyDataPlatform.Api --authentication AzureAD
dotnet sln add MyDataPlatform.Api

# Add ETL host  
dotnet new Rec-etl -n MyDataPlatform.Etl --processing Both
dotnet sln add MyDataPlatform.Etl

# Add scheduler
dotnet new Rec-scheduler -n MyDataPlatform.Scheduler --scheduler Quartz
dotnet sln add MyDataPlatform.Scheduler
```

## ⚙ Template Parameters

### Common Parameters
| Parameter | Short | Values | Default | Description |
|-----------|-------|--------|---------|-------------|
| `--authentication` | `-auth` | None, ApiKey, JWT, AzureAD | None | Authentication method |
| `--database` | `-db` | None, SqlServer, PostgreSQL, MySQL, SQLite, CosmosDB | None | Database provider |
| `--docker` | `-d` | true, false | true | Include Docker support |
| `--health-checks` | `-hc` | true, false | true | Include health check endpoints |

### ETL-Specific Parameters
| Parameter | Short | Values | Default | Description |
|-----------|-------|--------|---------|-------------|
| `--processing` | `-pt` | Batch, Stream, Both | Both | Processing approach |
| `--data-source` | `-ds` | Database, Files, Api | Database | Primary data source |

### Scheduler-Specific Parameters
| Parameter | Short | Values | Default | Description |
|-----------|-------|--------|---------|-------------|
| `--scheduler` | `-s` | InMemory, Quartz, Hangfire | Quartz | Scheduler implementation |

### External Connection-Specific Parameters
| Parameter | Short | Values | Default | Description |
|-----------|-------|--------|---------|-------------|
| `--connection-type` | `-ct` | Http, Database, MessageQueue | Http | Connection type |

## 🔧 Building Templates

### Development Build
```powershell
# Windows - basic build
.\build-templates.ps1

# Windows - full build with VSIX and NuGet package
.\build-templates.ps1 -BuildVsix -PackNuGet -Version 1.0.0
```

```bash
# Linux/macOS - basic build
./build-templates.sh

# Linux/macOS - full build with NuGet package
./build-templates.sh --pack-nuget --version 1.0.0 --configuration Release
```

### Build Outputs
- **VSIX Extension**: `dist/FractalDataWorks.Templates.Extension.vsix`
- **NuGet Package**: `dist/FractalDataWorks.Templates.{version}.nupkg`
- **Installation Scripts**: `dist/install/`

## 📋 Prerequisites

### All Platforms
- **.NET 8.0 SDK** or later
- **Git** (for GitHub source installations)

### Windows Additional
- **PowerShell 5.1** or later
- **Visual Studio 2022** (for VSIX extension)
- **MSBuild** (usually included with Visual Studio)

### Linux/macOS Additional
- **Bash 4.0** or later
- **curl** or **wget** (for downloads)

## 🗂 Directory Structure

```
templates/
├── vsix/                           # Visual Studio Extension
│   └── FractalDataWorks.Templates.Extension/
│       ├── Wizard/                 # Solution wizard components
│       │   ├── SolutionWizard.cs   # Main wizard implementation
│       │   ├── WizardDialog.xaml   # WPF configuration dialog
│       │   ├── WizardConfiguration.cs  # Configuration data model
│       │   ├── ProjectTemplateSelector.cs  # Template selection logic
│       │   └── ConfigurationBuilder.cs  # Solution/project builder
│       ├── FractalDataWorks.Templates.Extension.csproj
│       └── source.extension.vsixmanifest
├── package/                        # NuGet Template Package
│   ├── FractalDataWorks.Templates.nuspec
│   ├── template.config/            # Template host configuration
│   └── README.md                   # Package documentation
├── catalog/                        # Template Catalog System
│   ├── templates.json              # Template registry
│   ├── categories.json             # Template categorization
│   └── dependencies.json           # Package dependencies
├── install/                        # Installation Scripts
│   ├── install.ps1                 # Windows installer
│   ├── install.sh                  # Linux/macOS installer
│   ├── uninstall.ps1              # Windows uninstaller
│   ├── uninstall.sh               # Linux/macOS uninstaller
│   └── README.md                  # Installation guide
├── build-templates.ps1            # Windows build script
├── build-templates.sh             # Linux/macOS build script
└── README.md                      # This file
```

## 🔍 Template Features

### Generated Projects Include
- **Modern C# 12** and **.NET 8** features
- **Dependency Injection** setup with Microsoft DI container
- **Configuration Management** using FractalDataWorks configuration system
- **Logging** with structured logging and correlation IDs
- **Health Checks** with comprehensive system monitoring
- **Docker Support** with multi-stage builds and compose files
- **CI/CD Pipelines** for GitHub Actions, Azure DevOps
- **Security** with authentication, authorization, and secret management
- **Testing** with xUnit, Shouldly, and comprehensive test coverage

### Code Quality Standards
- **SOLID Principles** implementation
- **Enterprise Patterns** (Repository, Unit of Work, CQRS)
- **Error Handling** with structured error responses
- **Performance Optimization** with caching and connection pooling
- **Observability** with metrics, tracing, and monitoring
- **Documentation** with XML comments and OpenAPI specifications

## 🐛 Troubleshooting

### Common Issues

#### Templates Not Found
```bash
# Refresh template cache
dotnet new --debug:reinit

# List installed templates
dotnet new list
```

#### Permission Issues (Linux/macOS)
```bash
# Make scripts executable
chmod +x install/*.sh build-templates.sh

# Check permissions
ls -la install/
```

#### Visual Studio Extension Issues
1. Close all Visual Studio instances
2. Run installation as Administrator
3. Restart Visual Studio
4. Check Tools → Extensions and Updates

### Debug Information
Enable verbose logging:
```powershell
# Windows
.\install\install.ps1 -Verbose
.\build-templates.ps1 -Verbose
```

```bash
# Linux/macOS  
./install/install.sh --verbose
./build-templates.sh --verbose
```

## 📚 Documentation

- **Template Usage**: See individual template README files
- **API Reference**: [docs/API-REFERENCE.md](../docs/API-REFERENCE.md)
- **Architecture**: [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md)
- **Configuration**: [docs/CONFIGURATION.md](../docs/CONFIGURATION.md)

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch
3. **Make** changes to templates
4. **Test** with `./build-templates.ps1 -BuildVsix -PackNuGet`
5. **Submit** a pull request

## 📝 License

MIT License - see [LICENSE](../LICENSE) file for details.

---

**FractalDataWorks** - Comprehensive backend data platform development templates with enterprise-grade architecture and modern development practices.