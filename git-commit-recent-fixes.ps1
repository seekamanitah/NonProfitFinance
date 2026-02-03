#!/usr/bin/env pwsh
# Git commit script for recent bug fixes and enhancements

Write-Host "🔍 Checking Git status..." -ForegroundColor Cyan
git status

Write-Host "`n📦 Staging all changes..." -ForegroundColor Cyan
git add .

Write-Host "`n📝 Creating commit..." -ForegroundColor Cyan

# Multi-line commit message
$commitMessage = "Fix critical bugs, enhance imports, add Docker DNS fix, and improve Docker warnings`n`n" +
"BUGS FIXED:`n" +
"* HttpClient service registration missing in Blazor Server`n" +
"* Report filtering not respecting selected account filter`n" +
"* Starting balance locked after fund creation`n`n" +
"ENHANCEMENTS:`n" +
"* Added balance column mapping for bank statement imports`n" +
"* Added account selector dropdown to import all transactions into specific account`n" +
"* Import presets now save balance column and default fund settings`n" +
"* DefaultFundId takes precedence over CSV fund column for bulk imports`n`n" +
"AUTHORIZATION FIX:`n" +
"* Removed Admin role requirement from reset database endpoint for development`n" +
"* Base authentication still required (TODO: re-enable role check for production)`n`n" +
"DOCKER IMPROVEMENTS:`n" +
"* Removed obsolete 'version' attribute from docker-compose.yml`n" +
"* Added persistent volume for DataProtection keys to prevent session loss`n" +
"* Keys now persist across container restarts in 'nonprofit-keys' volume`n`n" +
"DOCKER DNS FIX:`n" +
"* Added fix-docker-dns.sh script to configure Docker DNS on remote servers`n" +
"* Added Dockerfile.dns-fix with alternative Ubuntu mirrors and retry logic`n" +
"* Added deploy-remote-dns-fix.sh for automated deployment with DNS fix`n" +
"* Added comprehensive documentation for DNS resolution issues`n`n" +
"DATABASE CHANGES:`n" +
"* ALTER TABLE migrations for ImportPresets.BalanceColumn`n" +
"* ALTER TABLE migrations for ImportPresets.DefaultFundId`n`n" +
"FILES MODIFIED:`n" +
"* Program.cs: HttpClient registration + database migrations`n" +
"* Controllers/AdminController.cs: Removed role requirement temporarily`n" +
"* Controllers/ImportExportController.cs: Added balance/account parameters`n" +
"* Services/ImportExportService.cs: DefaultFundId priority logic`n" +
"* Services/FundService.cs: Starting balance recalculation`n" +
"* Services/IImportExportService.cs: ImportMappingConfig extended`n" +
"* Models/ImportPreset.cs: New properties added`n" +
"* Components/Pages/ImportExport/ImportExportPage.razor: UI enhancements`n" +
"* Components/Pages/Reports/ReportBuilder.razor: Filter bug fix`n" +
"* Components/Shared/FundFormModal.razor: Editable starting balance`n" +
"* DTOs/Dtos.cs: UpdateFundRequest with StartingBalance`n`n" +
"NEW FILES ADDED:`n" +
"* fix-docker-dns.sh: Automated Docker DNS configuration script`n" +
"* Dockerfile.dns-fix: Alternative Dockerfile with DNS resilience`n" +
"* deploy-remote-dns-fix.sh: Deployment script with DNS fix`n" +
"* DOCKER_DNS_FIX_GUIDE.md: Complete DNS troubleshooting guide`n" +
"* DOCKER_DNS_QUICK_FIX.md: Quick reference for DNS issues`n`n" +
"All builds successful. Backward compatible changes.`n" +
"Fixes remote server deployment DNS resolution issues.`n" +
"Improves Docker container persistence and removes warnings."

git commit -m $commitMessage

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Commit successful!" -ForegroundColor Green
    
    Write-Host "`n🚀 Pushing to remote repository..." -ForegroundColor Cyan
    git push origin master
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Push successful! Changes are now on GitHub." -ForegroundColor Green
        Write-Host "`n📊 Summary:" -ForegroundColor Yellow
        Write-Host "  - 3 critical bugs fixed" -ForegroundColor White
        Write-Host "  - 2 import enhancements added" -ForegroundColor White
        Write-Host "  - 1 authorization issue resolved" -ForegroundColor White
        Write-Host "  - Docker DNS fix for remote deployment" -ForegroundColor White
        Write-Host "  - Database migrations included" -ForegroundColor White
    } else {
        Write-Host "`n❌ Push failed. Please check your network connection and credentials." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "`n❌ Commit failed. Please review the errors above." -ForegroundColor Red
    exit 1
}

Write-Host "`n✨ All done! Your changes are now in the remote repository." -ForegroundColor Green
