# NonProfit Finance Docker Deployment Script (PowerShell)
# Usage: .\deploy-docker.ps1

Write-Host "🚀 NonProfit Finance Docker Deployment" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# Configuration
$DOCKER_HOST = "192.168.100.107"
$DOCKER_PORT = "7171"
$IMAGE_NAME = "nonprofit-finance"
$IMAGE_TAG = "latest"
$DOCKER_USER = "tech"

Write-Host ""
Write-Host "📦 Step 1: Building Docker image..." -ForegroundColor Yellow
docker build -t "${IMAGE_NAME}:${IMAGE_TAG}" .

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Image built successfully!" -ForegroundColor Green

Write-Host ""
Write-Host "💾 Step 2: Saving image to tar file..." -ForegroundColor Yellow
docker save "${IMAGE_NAME}:${IMAGE_TAG}" -o "${IMAGE_NAME}.tar"

Write-Host ""
Write-Host "📤 Step 3: Copying files to Docker server..." -ForegroundColor Yellow
Write-Host "   Using SCP to transfer files..." -ForegroundColor Gray

# Check if scp is available
$scpPath = Get-Command scp -ErrorAction SilentlyContinue
if (-not $scpPath) {
    Write-Host "❌ SCP not found! Please install OpenSSH Client:" -ForegroundColor Red
    Write-Host "   Settings → Apps → Optional Features → Add OpenSSH Client" -ForegroundColor Yellow
    exit 1
}

# Copy files
scp "${IMAGE_NAME}.tar" docker-compose.yml "${DOCKER_USER}@${DOCKER_HOST}:~/"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to copy files to server!" -ForegroundColor Red
    Write-Host "⚠️  Make sure you have SSH access to ${DOCKER_USER}@${DOCKER_HOST}" -ForegroundColor Yellow
    Write-Host "⚠️  You may need to set up SSH keys or use password authentication" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "🔄 Step 4: Loading image on remote server..." -ForegroundColor Yellow
ssh "${DOCKER_USER}@${DOCKER_HOST}" "docker load -i ~/${IMAGE_NAME}.tar"

Write-Host ""
Write-Host "🚀 Step 5: Starting container..." -ForegroundColor Yellow
ssh "${DOCKER_USER}@${DOCKER_HOST}" "cd ~ && docker-compose down && docker-compose up -d"

Write-Host ""
Write-Host "🧹 Step 6: Cleaning up..." -ForegroundColor Yellow
Remove-Item "${IMAGE_NAME}.tar" -Force
ssh "${DOCKER_USER}@${DOCKER_HOST}" "rm ~/${IMAGE_NAME}.tar"

Write-Host ""
Write-Host "✅ Deployment complete!" -ForegroundColor Green

Write-Host ""
Write-Host "📊 Application Status:" -ForegroundColor Cyan
ssh "${DOCKER_USER}@${DOCKER_HOST}" "docker ps | grep nonprofit"

Write-Host ""
Write-Host "🌐 Access your application at:" -ForegroundColor Cyan
Write-Host "   http://${DOCKER_HOST}:${DOCKER_PORT}" -ForegroundColor Green

Write-Host ""
Write-Host "📝 Useful commands:" -ForegroundColor Cyan
Write-Host "   View logs:    ssh ${DOCKER_USER}@${DOCKER_HOST} 'docker logs -f nonprofit-finance'" -ForegroundColor Gray
Write-Host "   Restart:      ssh ${DOCKER_USER}@${DOCKER_HOST} 'docker-compose restart'" -ForegroundColor Gray
Write-Host "   Stop:         ssh ${DOCKER_USER}@${DOCKER_HOST} 'docker-compose down'" -ForegroundColor Gray
Write-Host "   Update data:  Use volume mounts at /app/data, /app/backups, /app/documents" -ForegroundColor Gray
Write-Host ""
