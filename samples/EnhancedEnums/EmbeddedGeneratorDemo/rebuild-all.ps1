# PowerShell script to clear and rebuild everything for the embedded generator demo

Write-Host "Starting complete rebuild..." -ForegroundColor Green

# Kill any running processes
Write-Host "Killing build processes..." -ForegroundColor Yellow
taskkill /f /im MSBuild.exe /t 2>$null
taskkill /f /im dotnet.exe /t 2>$null
taskkill /f /im VBCSCompiler.exe /t 2>$null

# Clear all local packages
Write-Host "Clearing local packages..." -ForegroundColor Yellow
Remove-Item -Path "localpackages\*" -Force -ErrorAction SilentlyContinue

# Clear all bin/obj folders
Write-Host "Clearing bin/obj directories..." -ForegroundColor Yellow
Get-ChildItem -Path . -Name "bin" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    Remove-Item -Path $_ -Recurse -Force -ErrorAction SilentlyContinue 
}
Get-ChildItem -Path . -Name "obj" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    Remove-Item -Path $_ -Recurse -Force -ErrorAction SilentlyContinue 
}
Get-ChildItem -Path "..\..\..\" -Name "bin" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    Remove-Item -Path $_ -Recurse -Force -ErrorAction SilentlyContinue 
}
Get-ChildItem -Path "..\..\..\" -Name "obj" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    Remove-Item -Path $_ -Recurse -Force -ErrorAction SilentlyContinue 
}

# Clear NuGet cache
Write-Host "Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear

Write-Host "Building and packing Enhanced Enums framework..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.EnhancedEnums.SourceGenerators\FractalDataWorks.EnhancedEnums.SourceGenerators.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "Enhanced Enums Source Generators pack failed" }

Write-Host "Building and packing Collections framework..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.Collections\FractalDataWorks.Collections.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "Collections pack failed" }

Write-Host "Building and packing Collections Analyzers..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.Collections.Analyzers\FractalDataWorks.Collections.Analyzers.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "Collections Analyzers pack failed" }

Write-Host "Building and packing Collections Source Generators..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.Collections.SourceGenerators\FractalDataWorks.Collections.SourceGenerators.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "Collections Source Generators pack failed" }

Write-Host "Building and packing ServiceTypes framework..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "ServiceTypes pack failed" }

Write-Host "Building and packing ServiceTypes Source Generators..." -ForegroundColor Cyan
dotnet pack "..\..\..\src\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj" --configuration Debug -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "ServiceTypes Source Generators pack failed" }

Write-Host "Building DataStores.Abstractions..." -ForegroundColor Cyan
dotnet pack "DataStores.Abstractions\DataStores.Abstractions.csproj" --configuration Debug -p:PackageVersion=1.1.0 -o "localpackages\"
if ($LASTEXITCODE -ne 0) { throw "DataStores.Abstractions pack failed" }

Write-Host "Building DataStore type packages..." -ForegroundColor Cyan
@("DataStoreTypes.Database", "DataStoreTypes.File", "DataStoreTypes.Web") | ForEach-Object {
    Write-Host "  Packing $_..." -ForegroundColor DarkCyan
    dotnet pack "$_\$_.csproj" --configuration Debug -p:PackageVersion=1.1.0 -o "localpackages\"
    if ($LASTEXITCODE -ne 0) { throw "$_ pack failed" }
}

Write-Host "Building TestApp..." -ForegroundColor Cyan
dotnet build "TestApp\TestApp.csproj"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "TestApp build failed" -ForegroundColor Red
    exit 1
} else {
    Write-Host "TestApp build succeeded!" -ForegroundColor Green
}

Write-Host "Rebuild complete!" -ForegroundColor Green