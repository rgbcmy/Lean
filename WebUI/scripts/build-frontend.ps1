# Production Build Script for Lean WebUI Frontend (Windows/PowerShell)
# Usage: .\build-frontend.ps1 [-Mode "production"] [-Analyze]

param(
    [string]$Mode = "production",
    [switch]$Analyze = $false
)

Write-Host "=== Lean WebUI Frontend Production Build ===" -ForegroundColor Cyan
Write-Host "Mode: $Mode" -ForegroundColor Yellow
Write-Host ""

# Change to frontend directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$FrontendDir = Join-Path (Split-Path -Parent $ScriptDir) "WebUI.Frontend"
Set-Location $FrontendDir

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "⚠️  node_modules not found. Running npm install..." -ForegroundColor Yellow
    npm install
}

try {
    # Clean previous build
    Write-Host "[1/5] Cleaning previous build..." -ForegroundColor Green
    if (Test-Path "dist") {
        Remove-Item -Recurse -Force dist
    }

    # Run linter
    Write-Host "[2/5] Running linter..." -ForegroundColor Green
    npm run lint
    if ($LASTEXITCODE -ne 0) {
        throw "Linting failed"
    }

    # Run type check
    Write-Host "[3/5] Running TypeScript type check..." -ForegroundColor Green
    npm run type-check
    if ($LASTEXITCODE -ne 0) {
        throw "Type check failed"
    }

    # Run tests
    Write-Host "[4/5] Running tests..." -ForegroundColor Green
    npm run test -- --run
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed"
    }

    # Build for production
    Write-Host "[5/5] Building for $Mode..." -ForegroundColor Green
    if ($Mode -eq "production") {
        npm run build:prod
    } elseif ($Mode -eq "staging") {
        npm run build:staging
    } else {
        npm run build
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Analyze bundle if requested
    if ($Analyze) {
        Write-Host ""
        Write-Host "[BONUS] Analyzing bundle size..." -ForegroundColor Green
        npm run analyze
    }

    Write-Host ""
    Write-Host "✓ Build complete!" -ForegroundColor Green
    Write-Host ""
    
    # Get directory size
    $distSize = (Get-ChildItem dist -Recurse | Measure-Object -Property Length -Sum).Sum
    $distSizeMB = [math]::Round($distSize / 1MB, 2)
    
    Write-Host "Output directory: dist/" -ForegroundColor Cyan
    Write-Host "Total size: $distSizeMB MB" -ForegroundColor Cyan
    Write-Host ""

    # List output files
    Write-Host "Build artifacts:" -ForegroundColor Cyan
    Get-ChildItem dist -Recurse -File | Select-Object -ExpandProperty FullName | ForEach-Object {
        $_.Replace($FrontendDir + "\", "")
    } | Sort-Object

    Write-Host ""
    Write-Host "To preview the build:" -ForegroundColor Yellow
    Write-Host "  npm run preview:prod"
    Write-Host ""
    Write-Host "To deploy:" -ForegroundColor Yellow
    Write-Host "  1. Copy dist\ to your web server"
    Write-Host "  2. Serve with any static file server (Nginx, IIS, etc.)"
    Write-Host "  3. Configure fallback to index.html for SPA routing"

} catch {
    Write-Host ""
    Write-Host "Build failed: $_" -ForegroundColor Red
    exit 1
}
