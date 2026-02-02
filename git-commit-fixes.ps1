# Git Commit Script for Database and UX Fixes
# Run this from the project root directory

Write-Host "NonProfit Finance - Committing Database & UX Fixes" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Check git status
Write-Host "Checking git status..." -ForegroundColor Yellow
git status

Write-Host ""
Write-Host "Adding all changes..." -ForegroundColor Yellow

# Add all changes
git add .

Write-Host ""
Write-Host "Creating commit..." -ForegroundColor Yellow

# Create detailed commit message
$commitMessage = @"
fix: Critical database schema and UX improvements

Database Fixes:
- Fixed RowVersion configuration for SQLite compatibility
  * Changed from .IsRowVersion() to .HasDefaultValue(0u)
  * Applied to Fund, Transaction, and Grant entities
  * Resolves 'NOT NULL constraint failed' errors
- Added proper schema detection and auto-recreation on startup
- Improved duplicate handling in CSV import service

UX Improvements:
- Auto-apply transaction filters with debouncing
- Fixed Fund Starting Balance input responsiveness
- Added real-time split transaction validation
- Improved error page to prevent infinite 404 loops
- Enhanced global search keyboard navigation
- Added visual loading indicators for filters

Import/Export Enhancements:
- Added try-catch for UNIQUE constraint violations
- Improved category/fund duplicate detection
- Better error handling during CSV imports

Technical Details:
- SQLite doesn't support automatic row versioning like SQL Server
- Using simple version counter with default value instead
- All concurrency tokens still function for optimistic locking
- Database auto-detects old schema and recreates if needed

Files Changed:
- Data/ApplicationDbContext.cs
- Components/Pages/Transactions/TransactionList.razor
- Components/Shared/FundFormModal.razor
- Components/Pages/Transactions/TransactionForm.razor
- Services/ImportExportService.cs
- Components/Shared/GlobalSearch.razor
- Components/Pages/Error.razor
- Program.cs

Breaking Changes: None
Migration Required: Database will auto-recreate on first run

Resolves: Database seeding errors, UX friction points from QA report
"@

git commit -m "$commitMessage"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Commit created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Pushing to remote repository..." -ForegroundColor Yellow
    
    # Push to remote
    git push origin master
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Successfully pushed to GitHub!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Run .\update-docker-remote.ps1 to update the Docker container" -ForegroundColor White
        Write-Host "2. Or manually pull and rebuild on your Docker host" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "✗ Push failed. Check your connection and permissions." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "✗ Commit failed. Check git status for issues." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Commit SHA:" -ForegroundColor Cyan
git rev-parse HEAD

Write-Host ""
Write-Host "Recent commits:" -ForegroundColor Cyan
git log --oneline -5
