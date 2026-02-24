# Build and publish Docker image to Docker Hub

$ErrorActionPreference = "Stop"

$VERSION = "1.0.0"
$IMAGE_NAME = "quantconnect/lean-webui"
$LATEST_TAG = "${IMAGE_NAME}:latest"
$VERSION_TAG = "${IMAGE_NAME}:${VERSION}"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Building and Publishing Lean WebUI Docker Image" -ForegroundColor Cyan
Write-Host "Version: ${VERSION}" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if logged into Docker Hub
Write-Host "Checking Docker Hub login status..." -ForegroundColor Yellow
$dockerInfo = docker info 2>&1 | Out-String
if (-not ($dockerInfo -match "Username")) {
    Write-Host "Not logged into Docker Hub. Please login:" -ForegroundColor Yellow
    docker login
}

Write-Host ""
Write-Host "Building Docker image..." -ForegroundColor Yellow

# Navigate to repository root (2 levels up from scripts directory)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location (Join-Path $scriptPath "..\..") -ErrorAction Stop

try {
    # Build the image
    docker build `
        -f WebUI/Dockerfile `
        -t $VERSION_TAG `
        -t $LATEST_TAG `
        --build-arg VERSION=$VERSION `
        .

    Write-Host ""
    Write-Host "Docker image built successfully!" -ForegroundColor Green
    Write-Host "Tags:" -ForegroundColor Green
    Write-Host "  - ${VERSION_TAG}" -ForegroundColor Cyan
    Write-Host "  - ${LATEST_TAG}" -ForegroundColor Cyan

    Write-Host ""
    Write-Host "Pushing to Docker Hub..." -ForegroundColor Yellow

    # Push version tag
    docker push $VERSION_TAG
    Write-Host "✓ Pushed ${VERSION_TAG}" -ForegroundColor Green

    # Push latest tag
    docker push $LATEST_TAG
    Write-Host "✓ Pushed ${LATEST_TAG}" -ForegroundColor Green

    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "Docker images published successfully!" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Pull commands:" -ForegroundColor Yellow
    Write-Host "  docker pull ${VERSION_TAG}" -ForegroundColor Cyan
    Write-Host "  docker pull ${LATEST_TAG}" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Run command:" -ForegroundColor Yellow
    Write-Host "  docker run -d -p 5000:5000 ${LATEST_TAG}" -ForegroundColor Cyan
}
finally {
    Pop-Location
}
