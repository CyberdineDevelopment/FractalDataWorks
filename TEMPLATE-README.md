# Repository Template Usage

This template provides a standardized setup for .NET projects with Azure DevOps CI/CD.

## Using This Template

1. **Copy all files** from this directory to your new repository
2. **Update placeholders** in the following files:
   - `README.md` - Update project name, description, and URLs
   - `LICENSE` - Update copyright year and owner
   - `azure-pipelines.yml` - Update if you need different parameters
   - `Directory.Build.props` - Update company/author information

3. **Initialize versioning**:
   ```bash
   nbgv install
   ```

4. **Create initial project structure**:
   - Add your projects to `src/`
   - Add test projects to `tests/`
   - Add documentation to `docs/`
   - Add sample projects to `samples/`

5. **Configure Azure DevOps**:
   - Create a pipeline from `azure-pipelines.yml`
   - Ensure the build service has permissions to the feed

## Build Configurations

The template includes multiple build configurations:
- **Debug** - Fast local development (no analyzers)
- **Experimental** - Minimal checks for rapid prototyping
- **Alpha** - Basic checks for internal development
- **Beta** - Recommended checks with warnings as errors
- **Preview** - Strict checks for release candidates
- **Release** - Production-ready with full enforcement

## Feed Configuration

All packages are published to the `dotnet-packages` feed with views:
- `@Experimental` - For experimental/* branches
- `@Alpha` - For develop branch
- `@Beta` - For beta/* branches
- `@Preview` - For release/* branches
- `@Release` - For main/master branch (default view)

## Included Analyzers

The template includes these analyzers by default:
- AsyncFixer - Async/await best practices
- Meziantou.Analyzer - Performance and correctness
- Microsoft.VisualStudio.Threading.Analyzers - Threading safety
- Roslynator.Analyzers - Code quality and refactoring

StyleCop.Analyzers is NOT included (use .editorconfig for style rules).