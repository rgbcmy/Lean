# Create Git tag for v1.0.0 release

$VERSION = "v1.0.0"
$TAG_NAME = "webui-v1.0.0"
$MESSAGE = "Lean WebUI v1.0.0 - First Official Release"

Write-Host "Creating Git tag: $TAG_NAME" -ForegroundColor Green

# Create annotated tag
git tag -a $TAG_NAME -m $MESSAGE

# Show tag info
git show $TAG_NAME --no-patch

Write-Host ""
Write-Host "Tag created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To push the tag to remote, run:" -ForegroundColor Yellow
Write-Host "  git push origin $TAG_NAME" -ForegroundColor Cyan
Write-Host ""
Write-Host "To create a GitHub release:" -ForegroundColor Yellow
Write-Host "  1. Go to https://github.com/QuantConnect/Lean/releases/new" -ForegroundColor Cyan
Write-Host "  2. Select tag: $TAG_NAME" -ForegroundColor Cyan
Write-Host "  3. Set title: 'Lean WebUI v1.0.0 - First Official Release'" -ForegroundColor Cyan
Write-Host "  4. Copy content from WebUI/RELEASE_NOTES.md" -ForegroundColor Cyan
Write-Host "  5. Upload any release assets (if needed)" -ForegroundColor Cyan
Write-Host "  6. Click 'Publish release'" -ForegroundColor Cyan
