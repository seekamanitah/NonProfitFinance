# Git Commit Script for Docker & Environment Files (PowerShell)

# Configure Git identity (one-time setup)
git config --global user.email "tech@firefighter.local"
git config --global user.name "Tech Admin"

# Navigate to project
cd C:\Users\tech\source\repos\NonProfitFinance

Write-Host "📋 Staging files..." -ForegroundColor Cyan
git add Dockerfile
git add .dockerignore
git add docker-compose.yml
git add .env.example
git add .gitignore

Write-Host "✅ Files staged" -ForegroundColor Green

Write-Host ""
Write-Host "📝 Committing changes..." -ForegroundColor Cyan

git commit -m "fix: Fix docker-compose invalid volume and depends_on configuration

- Removed invalid 'depends_on' circular reference (service depending on itself)
- Fixed volume definitions in docker-compose.yml
- Ensured proper network configuration
- Docker-compose now properly creates named volumes
- Resolves 'invalid compose project' error
- Ready for production deployment"

Write-Host ""
Write-Host "📤 Pushing to GitHub..." -ForegroundColor Cyan
git push origin master

Write-Host ""
Write-Host "✅ Git commit complete!" -ForegroundColor Green
Write-Host "📊 Changes pushed to: https://github.com/seekamanitah/NonProfitFinance" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 Ready to deploy on Docker server:" -ForegroundColor Cyan
Write-Host "   git pull origin master" -ForegroundColor Yellow
Write-Host "   docker-compose down" -ForegroundColor Yellow
Write-Host "   docker-compose up -d" -ForegroundColor Yellow



