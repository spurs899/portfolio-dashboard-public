# Cache Busting - Deployment Script
# This script updates version numbers in index.html to force cache refresh

param(
    [string]$Version = "",
    [switch]$AutoIncrement
)

$ErrorActionPreference = "Stop"

# Get project root directory
$projectRoot = $PSScriptRoot
$webProject = Join-Path $projectRoot "PortfolioManager.Web"
$indexPath = Join-Path $webProject "wwwroot\index.html"
$versionPath = Join-Path $webProject "wwwroot\version.txt"

Write-Host "Portfolio Dashboard - Cache Busting Version Update" -ForegroundColor Cyan
Write-Host "=" * 60

# Generate version if not provided
if ([string]::IsNullOrEmpty($Version)) {
    if ($AutoIncrement -and (Test-Path $versionPath)) {
        # Read current version and increment
        $currentVersion = Get-Content $versionPath -Raw
        if ($currentVersion -match '^(\d+)\.(\d+)\.(\d+)$') {
            $major = [int]$matches[1]
            $minor = [int]$matches[2]
            $patch = [int]$matches[3] + 1
            $Version = "$major.$minor.$patch"
            Write-Host "Auto-incremented version: $currentVersion → $Version" -ForegroundColor Yellow
        } else {
            # Invalid format, start fresh
            $Version = "1.0.0"
            Write-Host "Starting new version: $Version (previous format invalid)" -ForegroundColor Yellow
        }
    } else {
        # Use timestamp-based version: YYYYMMDD.HHMM
        $Version = Get-Date -Format "yyyyMMdd.HHmm"
        Write-Host "Generated timestamp version: $Version" -ForegroundColor Yellow
    }
} else {
    Write-Host "Using provided version: $Version" -ForegroundColor Green
}

# Check if index.html exists
if (-not (Test-Path $indexPath)) {
    Write-Host "ERROR: index.html not found at $indexPath" -ForegroundColor Red
    exit 1
}

# Read index.html
$indexContent = Get-Content $indexPath -Raw

# Update version query strings
$indexContent = $indexContent -replace 'app\.css\?v=[^"]+', "app.css?v=$Version"
$indexContent = $indexContent -replace 'PortfolioManager\.Web\.styles\.css\?v=[^"]+', "PortfolioManager.Web.styles.css?v=$Version"
$indexContent = $indexContent -replace 'blazor\.webassembly\.js\?v=[^"]+', "blazor.webassembly.js?v=$Version"
$indexContent = $indexContent -replace "const APP_VERSION = '[^']+';", "const APP_VERSION = '$Version';"

# Write updated content
Set-Content -Path $indexPath -Value $indexContent -NoNewline

# Update version.txt
Set-Content -Path $versionPath -Value $Version -NoNewline

Write-Host ""
Write-Host "✓ Updated version to: $Version" -ForegroundColor Green
Write-Host "  - $indexPath" -ForegroundColor Gray
Write-Host "  - $versionPath" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Build the project: dotnet build" -ForegroundColor Gray
Write-Host "  2. Publish the project: dotnet publish -c Release" -ForegroundColor Gray
Write-Host "  3. Deploy to GitHub Pages" -ForegroundColor Gray
Write-Host ""
Write-Host "Usage examples:" -ForegroundColor Cyan
Write-Host "  .\update-version.ps1                  # Timestamp version (YYYYMMDD.HHMM)" -ForegroundColor Gray
Write-Host "  .\update-version.ps1 -AutoIncrement   # Auto-increment patch (1.0.1 → 1.0.2)" -ForegroundColor Gray
Write-Host "  .\update-version.ps1 '2.0.0'          # Custom version" -ForegroundColor Gray
Write-Host ""
Write-Host "Users will automatically see the new version on next visit!" -ForegroundColor Green
