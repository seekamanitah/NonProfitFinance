#!/usr/bin/env pwsh
# Complete Docker cleanup and rebuild script for NonProfit Finance

$ErrorActionPreference = "Continue"

Write-Host "🗑️  Complete Docker Cleanup and Rebuild" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "⚠️  WARNING: This will DELETE ALL data in Docker containers!" -ForegroundColor Yellow
Write-Host "   - All containers will be stopped and removed" -ForegroundColor Yellow
Write-Host "   - All images will be removed" -ForegroundColor Yellow
Write-Host "   - All volumes will be removed (DATABASE DATA DELETED)" -ForegroundColor Yellow
Write-Host "   - All networks will be removed" -ForegroundColor Yellow
Write-Host ""

# Ask for confirmation
$confirmation = Read-Host "Type 'DELETE' to confirm complete cleanup"
if ($confirmation -ne "DELETE") {
    Write-Host "❌ Cleanup cancelled." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🛑 Step 1: Stopping all running containers..." -ForegroundColor Cyan
docker-compose down 2>$null
docker stop $(docker ps -aq) 2>$null
Write-Host "✅ Containers stopped" -ForegroundColor Green

Write-Host ""
Write-Host "🗑️  Step 2: Removing all containers..." -ForegroundColor Cyan
docker rm -f $(docker ps -aq) 2>$null
Write-Host "✅ Containers removed" -ForegroundColor Green

Write-Host ""
Write-Host "🗑️  Step 3: Removing NonProfit Finance images..." -ForegroundColor Cyan
docker rmi -f $(docker images "nonprofit-finance*" -q) 2>$null
docker rmi -f nonprofit-finance:latest 2>$null
Write-Host "✅ Images removed" -ForegroundColor Green

Write-Host ""
Write-Host "🗑️  Step 4: Removing volumes (DATABASE DATA DELETED)..." -ForegroundColor Cyan
docker volume rm $(docker volume ls -q) 2>$null
docker volume prune -f 2>$null
Write-Host "✅ Volumes removed" -ForegroundColor Green

Write-Host ""
Write-Host "🗑️  Step 5: Removing networks..." -ForegroundColor Cyan
docker network prune -f 2>$null
Write-Host "✅ Networks removed" -ForegroundColor Green

Write-Host ""
Write-Host "🧹 Step 6: Running Docker system prune..." -ForegroundColor Cyan
docker system prune -af --volumes 2>$null
Write-Host "✅ System cleaned" -ForegroundColor Green

Write-Host ""
Write-Host "📊 Step 7: Verifying cleanup..." -ForegroundColor Cyan
Write-Host ""
Write-Host "Remaining containers:" -ForegroundColor Yellow
docker ps -a
Write-Host ""
Write-Host "Remaining images:" -ForegroundColor Yellow
docker images
Write-Host ""
Write-Host "Remaining volumes:" -ForegroundColor Yellow
docker volume ls
Write-Host ""

Write-Host "✅ Complete cleanup finished!" -ForegroundColor Green
Write-Host ""
Write-Host "🏗️  Step 8: Rebuilding from scratch..." -ForegroundColor Cyan
Write-Host ""

# Rebuild
Write-Host "Building fresh Docker image..." -ForegroundColor Cyan
docker-compose build --no-cache

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🚀 Step 9: Starting containers..." -ForegroundColor Cyan
    docker-compose up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Containers started!" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "⏳ Waiting for application to start (30 seconds)..." -ForegroundColor Cyan
        Start-Sleep -Seconds 30
        
        Write-Host ""
        Write-Host "📊 Final Status:" -ForegroundColor Yellow
        docker-compose ps
        
        Write-Host ""
        Write-Host "✨ Complete rebuild finished!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Access application: http://localhost:8080" -ForegroundColor White
        Write-Host "  2. View logs: docker-compose logs -f" -ForegroundColor White
        Write-Host "  3. Check health: curl http://localhost:8080/health" -ForegroundColor White
        Write-Host ""
        Write-Host "⚠️  Note: All data has been reset. You'll need to:" -ForegroundColor Yellow
        Write-Host "     - Create a new account" -ForegroundColor White
        Write-Host "     - Configure organization settings" -ForegroundColor White
        Write-Host "     - Set up categories/accounts" -ForegroundColor White
        Write-Host "     - Or load demo data" -ForegroundColor White
    } else {
        Write-Host "❌ Failed to start containers" -ForegroundColor Red
        Write-Host "Check logs: docker-compose logs" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host "Check the output above for errors" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "🎉 All done! Fresh Docker environment ready." -ForegroundColor Green
