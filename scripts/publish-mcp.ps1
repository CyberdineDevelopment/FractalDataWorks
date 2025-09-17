Write-Host "Publishing FractalDataWorks MCP Server..." -ForegroundColor Green

# Build all required dependencies first
Write-Host "Building dependencies..." -ForegroundColor Yellow
$requiredProjects = @(
    "src\FractalDataWorks.Abstractions\FractalDataWorks.Abstractions.csproj",
    "src\FractalDataWorks.Results\FractalDataWorks.Results.csproj",
    "src\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.csproj",
    "src\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj",
    "src\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj",
    "src\FractalDataWorks.Services.Connections.Abstractions\FractalDataWorks.Services.Connections.Abstractions.csproj",
    "src\FractalDataWorks.Configuration.Abstractions\FractalDataWorks.Configuration.Abstractions.csproj",
    "src\FractalDataWorks.MCP\FractalDataWorks.MCP.csproj"
)

$rootPath = Split-Path -Parent $PSScriptRoot
foreach ($project in $requiredProjects) {
    $projectPath = Join-Path $rootPath $project
    Write-Host "  Building $project..." -ForegroundColor Gray
    dotnet build $projectPath -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to build $project" -ForegroundColor Red
        exit 1
    }
}

# Create target directory if it doesn't exist
$targetPath = "C:\development\tools\mcp\fractal-roslyn"
if (!(Test-Path $targetPath)) {
    New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
}

# Publish the application
Write-Host "Publishing to $targetPath..." -ForegroundColor Yellow
$mcpProject = Join-Path $rootPath "src\FractalDataWorks.MCP\FractalDataWorks.MCP.csproj"
dotnet publish $mcpProject -c Release -o $targetPath --self-contained false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Successfully published to $targetPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "To update MCP config, run:" -ForegroundColor Cyan
    Write-Host "  claude mcp remove roslyn-analyzer" -ForegroundColor White
    Write-Host "  claude mcp add --scope project fractal-roslyn `"$targetPath\FractalDataWorks.MCP.exe`"" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ Publish failed with error code $LASTEXITCODE" -ForegroundColor Red
}