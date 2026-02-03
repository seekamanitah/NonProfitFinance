#!/bin/bash
# Quick deployment script for remote server with DNS fix

set -e  # Exit on error

echo "🚀 NonProfit Finance - Remote Deployment with DNS Fix"
echo "====================================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if running as root or with sudo
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}❌ Please run with sudo${NC}"
    echo "Usage: sudo ./deploy-remote-dns-fix.sh"
    exit 1
fi

echo "📍 Current directory: $(pwd)"
echo ""

# Step 1: Fix Docker DNS if needed
echo "🔧 Step 1: Checking Docker DNS configuration..."
if [ ! -f /etc/docker/daemon.json ] || ! grep -q "dns" /etc/docker/daemon.json 2>/dev/null; then
    echo -e "${YELLOW}⚠️  Docker DNS not configured. Fixing...${NC}"
    
    # Backup existing config
    if [ -f /etc/docker/daemon.json ]; then
        cp /etc/docker/daemon.json /etc/docker/daemon.json.backup.$(date +%Y%m%d%H%M%S)
    fi
    
    # Create new config with DNS
    cat > /etc/docker/daemon.json <<EOF
{
    "dns": ["8.8.8.8", "8.8.4.4", "1.1.1.1"],
    "log-driver": "json-file",
    "log-opts": {
        "max-size": "10m",
        "max-file": "3"
    }
}
EOF
    
    echo "✅ Docker DNS configured"
    echo "🔄 Restarting Docker..."
    systemctl restart docker
    sleep 5
    
    if systemctl is-active --quiet docker; then
        echo -e "${GREEN}✅ Docker restarted successfully${NC}"
    else
        echo -e "${RED}❌ Docker failed to restart${NC}"
        exit 1
    fi
else
    echo -e "${GREEN}✅ Docker DNS already configured${NC}"
fi

echo ""

# Step 2: Test DNS
echo "🧪 Step 2: Testing DNS resolution..."
if docker run --rm ubuntu:24.04 bash -c "apt-get update > /dev/null 2>&1"; then
    echo -e "${GREEN}✅ DNS resolution working${NC}"
    USE_REGULAR_DOCKERFILE=true
else
    echo -e "${YELLOW}⚠️  DNS still having issues, will use DNS-resilient Dockerfile${NC}"
    USE_REGULAR_DOCKERFILE=false
fi

echo ""

# Step 3: Stop existing containers
echo "⏹️  Step 3: Stopping existing containers..."
docker-compose down 2>/dev/null || true
echo "✅ Containers stopped"
echo ""

# Step 4: Pull latest code
echo "📥 Step 4: Pulling latest code from GitHub..."
git fetch origin
git reset --hard origin/master
echo "✅ Code updated"
echo ""

# Step 5: Choose Dockerfile
echo "🏗️  Step 5: Building Docker image..."
if [ "$USE_REGULAR_DOCKERFILE" = true ]; then
    echo "Using regular Dockerfile..."
    docker-compose build --no-cache
else
    echo "Using DNS-resilient Dockerfile..."
    # Temporarily use the DNS-fix Dockerfile
    if [ -f Dockerfile.dns-fix ]; then
        cp Dockerfile Dockerfile.original
        cp Dockerfile.dns-fix Dockerfile
        docker-compose build --no-cache
        mv Dockerfile.original Dockerfile
    else
        echo -e "${YELLOW}⚠️  Dockerfile.dns-fix not found, trying with regular Dockerfile...${NC}"
        docker-compose build --no-cache
    fi
fi

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Build successful${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    echo ""
    echo "Troubleshooting steps:"
    echo "1. Check Docker logs: journalctl -u docker -f"
    echo "2. Test DNS: docker run --rm ubuntu:24.04 ping -c 3 8.8.8.8"
    echo "3. Check firewall: ufw status"
    exit 1
fi

echo ""

# Step 6: Start containers
echo "▶️  Step 6: Starting containers..."
docker-compose up -d

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Containers started${NC}"
else
    echo -e "${RED}❌ Failed to start containers${NC}"
    exit 1
fi

echo ""

# Step 7: Wait for application to start
echo "⏳ Step 7: Waiting for application to start..."
sleep 10

# Step 8: Check health
echo "🏥 Step 8: Checking application health..."
for i in {1..30}; do
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Application is healthy!${NC}"
        break
    fi
    
    if [ $i -eq 30 ]; then
        echo -e "${YELLOW}⚠️  Health check timeout, but container may still be starting${NC}"
        echo "Check logs with: docker-compose logs -f"
    else
        echo -n "."
        sleep 2
    fi
done

echo ""

# Step 9: Display status
echo "📊 Step 9: Deployment Status"
echo "============================"
docker-compose ps

echo ""
echo -e "${GREEN}✨ Deployment complete!${NC}"
echo ""
echo "📋 Quick commands:"
echo "  View logs:        docker-compose logs -f"
echo "  Restart:          docker-compose restart"
echo "  Stop:             docker-compose down"
echo "  Check health:     curl http://localhost:8080/health"
echo "  View containers:  docker ps"
echo ""
echo "🌐 Access application:"
echo "  Local:            http://localhost:8080"
echo "  Remote:           http://$(curl -s ifconfig.me):8080"
echo ""
