Write-Host "Building FractalDataWorks MCP Server..." -ForegroundColor Green

$rootPath = Split-Path -Parent $PSScriptRoot
$mcpProject = Join-Path $rootPath "src\FractalDataWorks.MCP\FractalDataWorks.MCP.csproj"

# Build the project (will build dependencies automatically)
dotnet build $mcpProject -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "❌ Build failed with error code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}