Write-Host "Publishing Roslyn MCP Server..." -ForegroundColor Green

# Create target directory if it doesn't exist
$targetPath = "C:\development\tools\mcp\roslyn"
if (!(Test-Path $targetPath)) {
    New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
}

# Publish the application
Write-Host "Publishing to $targetPath..." -ForegroundColor Yellow
dotnet publish RoslynMcpServer/RoslynMcpServer.csproj -c Release -o $targetPath --self-contained false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Successfully published to $targetPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "To update MCP config, run:" -ForegroundColor Cyan
    Write-Host "  claude mcp remove roslyn-analyzer" -ForegroundColor White
    Write-Host "  claude mcp add --scope project roslyn-analyzer `"$targetPath\RoslynMcpServer.exe`"" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ Publish failed with error code $LASTEXITCODE" -ForegroundColor Red
}