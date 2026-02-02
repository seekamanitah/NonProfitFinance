# Update Remote Docker Container Script
# This script updates a running Docker container with the latest code

param(
    [string]$DockerHost = "localhost",
    [int]$SshPort = 22,
    [string]$SshUser = "root",
    [switch]$Local = $false
)

Write-Host "NonProfit Finance - Docker Container Update" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

if ($Local) {
    Write-Host "Updating LOCAL Docker container..." -ForegroundColor Yellow
    Write-Host ""
    
    # Stop current containers
    Write-Host "1. Stopping containers..." -ForegroundColor Yellow
    docker-compose down
    
    # Pull latest code (if needed)
    Write-Host ""
    Write-Host "2. Pulling latest code from Git..." -ForegroundColor Yellow
    git pull origin master
    
    # Remove old database to force schema recreation
    Write-Host ""
    Write-Host "3. Removing old database..." -ForegroundColor Yellow
    if (Test-Path "NonProfitFinance.db") {
        Remove-Item "NonProfitFinance.db*" -Force
        Write-Host "   ✓ Database removed" -ForegroundColor Green
    } else {
        Write-Host "   ℹ No database found (will be created fresh)" -ForegroundColor Gray
    }
    
    # Rebuild and start
    Write-Host ""
    Write-Host "4. Rebuilding and starting containers..." -ForegroundColor Yellow
    docker-compose up -d --build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Local Docker container updated successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Container Status:" -ForegroundColor Cyan
        docker-compose ps
        Write-Host ""
        Write-Host "View logs with: docker-compose logs -f" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "✗ Docker update failed. Check logs above." -ForegroundColor Red
        exit 1
    }
    
} else {
    Write-Host "Updating REMOTE Docker container on $DockerHost..." -ForegroundColor Yellow
    Write-Host ""
    
    # Check if SSH is available
    $sshAvailable = Get-Command ssh -ErrorAction SilentlyContinue
    if (-not $sshAvailable) {
        Write-Host "✗ SSH not found. Please install OpenSSH or Git Bash." -ForegroundColor Red
        Write-Host ""
        Write-Host "Alternative: Run this script with -Local flag on the remote server" -ForegroundColor Yellow
        exit 1
    }
    
    # Create remote update script
    $remoteScript = @"
#!/bin/bash
echo 'Updating NonProfit Finance Docker container...'
cd /opt/nonprofit-finance || cd ~/nonprofit-finance || exit 1

echo '1. Stopping containers...'
docker-compose down

echo '2. Pulling latest code...'
git pull origin master

echo '3. Removing old database...'
rm -f NonProfitFinance.db*
echo '   Database removed'

echo '4. Rebuilding and starting...'
docker-compose up -d --build

echo ''
echo 'Container status:'
docker-compose ps

echo ''
echo 'View logs with: docker-compose logs -f'
"@
    
    # Save script to temp file
    $tempScript = [System.IO.Path]::GetTempFileName() + ".sh"
    $remoteScript | Out-File -FilePath $tempScript -Encoding UTF8 -NoNewline
    
    Write-Host "Connecting to $SshUser@$DockerHost..." -ForegroundColor Yellow
    
    # Copy script to remote
    scp -P $SshPort $tempScript "${SshUser}@${DockerHost}:/tmp/update-nonprofit.sh"
    
    if ($LASTEXITCODE -eq 0) {
        # Execute on remote
        ssh -p $SshPort "${SshUser}@${DockerHost}" "chmod +x /tmp/update-nonprofit.sh && /tmp/update-nonprofit.sh"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✓ Remote Docker container updated successfully!" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "✗ Remote update failed. Check SSH connection." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host ""
        Write-Host "✗ Failed to connect to remote server." -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "1. Check SSH connection: ssh -p $SshPort $SshUser@$DockerHost" -ForegroundColor White
        Write-Host "2. Verify Docker host address is correct" -ForegroundColor White
        Write-Host "3. Ensure SSH key is configured or enter password when prompted" -ForegroundColor White
        exit 1
    }
    
    # Cleanup
    Remove-Item $tempScript -Force
}

Write-Host ""
Write-Host "Update complete! Your Docker container is now running the latest code." -ForegroundColor Green
Write-Host ""
Write-Host "Important Notes:" -ForegroundColor Cyan
Write-Host "- Database was recreated with the new schema" -ForegroundColor White
Write-Host "- All seed data will be fresh (default categories, funds, etc.)" -ForegroundColor White
Write-Host "- Old transactions/data have been removed" -ForegroundColor White
Write-Host ""
Write-Host "Access your application at: http://${DockerHost}:5000" -ForegroundColor Green
