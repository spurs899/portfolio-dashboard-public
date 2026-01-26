#!/usr/bin/env pwsh
# Sync Web and Contracts projects to public portfolio repo

Write-Host "Syncing to public portfolio repo..." -ForegroundColor Cyan

# Get current branch and commit (store before any git operations)
$currentBranch = git branch --show-current
if ([string]::IsNullOrEmpty($currentBranch)) {
    $currentBranch = "master"
}
$currentCommit = git rev-parse HEAD

# Create a temp directory
$tempDir = New-Item -ItemType Directory -Path (Join-Path $env:TEMP "portfolio-sync-$(Get-Random)") -Force

try {
    # Copy folders to temp - explicitly create folder names
    Write-Host "Copying projects to temp directory..." -ForegroundColor Yellow
    $webDest = Join-Path $tempDir "PortfolioManager.Web"
    $contractsDest = Join-Path $tempDir "PortfolioManager.Contracts"
    $screenshotsDest = Join-Path $tempDir "Screenshots"
    
    Copy-Item -Path "PortfolioManager.Web" -Destination $webDest -Recurse -Force
    Copy-Item -Path "PortfolioManager.Contracts" -Destination $contractsDest -Recurse -Force
    
    if (Test-Path "Screenshots") {
        Copy-Item -Path "Screenshots" -Destination $screenshotsDest -Recurse -Force
    }
    if (Test-Path "PUBLIC_README.md") {
        Copy-Item -Path "PUBLIC_README.md" -Destination (Join-Path $tempDir "README.md") -Force
    }
    if (Test-Path ".gitignore") {
        Copy-Item -Path ".gitignore" -Destination (Join-Path $tempDir ".gitignore") -Force
    }

    # Create temporary branch for public content
    Write-Host "Creating temporary branch..." -ForegroundColor Yellow
    git checkout --orphan temp-public-sync 2>&1 | Out-Null

    # Remove all files from git index
    git rm -rf . 2>&1 | Out-Null

    # Clean working directory (ignore errors for locked files)
    Get-ChildItem -Force | Where-Object { $_.Name -ne ".git" } | ForEach-Object {
        Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
    }

    # Copy from temp back to repo
    Copy-Item -Path (Join-Path $tempDir "*") -Destination "." -Recurse -Force

    # Stage and commit
    git add -A
    git commit -m "Sync Web and Contracts projects" 2>&1 | Out-Null

    # Push to public repo (force to overwrite)
    Write-Host "Pushing to public repository..." -ForegroundColor Yellow
    git push public temp-public-sync:main --force

    Write-Host ""
    Write-Host "Sync complete!" -ForegroundColor Green
    Write-Host "View at: https://github.com/spurs899/portfolio-dashboard-public" -ForegroundColor Cyan
}
finally {
    # Return to original branch and commit - use -f to force restore files
    Write-Host "Restoring original branch..." -ForegroundColor Yellow
    git checkout -f $currentBranch 2>&1 | Out-Null
    
    # Ensure we're at the right commit
    git reset --hard $currentCommit 2>&1 | Out-Null
    
    # Clean up temp branch
    git branch -D temp-public-sync 2>&1 | Out-Null
    
    # Remove temp directory
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
