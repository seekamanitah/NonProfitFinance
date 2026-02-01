# WiX Heat File Harvester
# Automatically generates WiX file fragments from published output

param(
    [string]$PublishDir = "..\publish",
    [string]$OutputFile = "PublishedFiles.wxs",
    [string]$WixPath = $null
)

$ErrorActionPreference = "Stop"

# Auto-detect WiX if not provided
if (-not $WixPath) {
    $wixSearchPaths = @(
        "C:\Program Files (x86)\WiX Toolset v3.14\bin",
        "C:\Program Files (x86)\WiX Toolset v3.11\bin",
        "C:\Program Files\WiX Toolset v3.14\bin",
        "C:\Program Files\WiX Toolset v3.11\bin"
    )
    
    foreach ($path in $wixSearchPaths) {
        if (Test-Path "$path\heat.exe") {
            $WixPath = $path
            break
        }
    }
    
    if (-not $WixPath) {
        Write-Error "WiX Toolset not found!"
        exit 1
    }
}

Write-Host "Using WiX from: $WixPath" -ForegroundColor Cyan

# Resolve paths
$publishDirFull = Resolve-Path $PublishDir
$outputFileFull = Join-Path $PSScriptRoot $OutputFile

Write-Host "Harvesting files from: $publishDirFull" -ForegroundColor Yellow
Write-Host "Output file: $outputFileFull" -ForegroundColor Yellow

# Run heat.exe to harvest files
& "$WixPath\heat.exe" dir "$publishDirFull" `
    -cg PublishedFilesGroup `
    -gg `
    -sfrag `
    -srd `
    -scom `
    -sreg `
    -dr INSTALLFOLDER `
    -var var.PublishDir `
    -out "$outputFileFull"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Heat.exe failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "`n[OK] File harvest complete!" -ForegroundColor Green
Write-Host "Generated: $OutputFile" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Include this file in your Product.wxs" -ForegroundColor Gray
Write-Host "2. Reference the ComponentGroup 'PublishedFilesGroup'" -ForegroundColor Gray
