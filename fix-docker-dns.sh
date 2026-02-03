#!/bin/bash
# Fix Docker DNS resolution issues on remote server

echo "🔧 Docker DNS Resolution Fix"
echo "============================"
echo ""

# Solution 1: Configure Docker daemon with Google DNS
echo "📋 Solution 1: Configure Docker DNS..."
echo ""

# Backup existing daemon.json if it exists
if [ -f /etc/docker/daemon.json ]; then
    echo "Backing up existing daemon.json..."
    sudo cp /etc/docker/daemon.json /etc/docker/daemon.json.backup.$(date +%Y%m%d%H%M%S)
fi

# Create or update daemon.json with DNS configuration
echo "Creating Docker daemon configuration with DNS servers..."
sudo tee /etc/docker/daemon.json > /dev/null <<EOF
{
    "dns": ["8.8.8.8", "8.8.4.4", "1.1.1.1"],
    "log-driver": "json-file",
    "log-opts": {
        "max-size": "10m",
        "max-file": "3"
    }
}
EOF

echo "✅ Docker daemon.json configured"
echo ""

# Restart Docker service
echo "🔄 Restarting Docker service..."
sudo systemctl restart docker

# Wait for Docker to restart
sleep 5

# Verify Docker is running
if sudo systemctl is-active --quiet docker; then
    echo "✅ Docker service restarted successfully"
else
    echo "❌ Docker service failed to restart"
    echo "Please check: sudo journalctl -u docker"
    exit 1
fi

echo ""
echo "🧪 Testing DNS resolution in Docker container..."
if sudo docker run --rm ubuntu:24.04 bash -c "apt-get update > /dev/null 2>&1 && echo 'DNS Working'"; then
    echo "✅ DNS resolution is working!"
else
    echo "⚠️  DNS still having issues. Trying alternative solution..."
    echo ""
    echo "📋 Solution 2: Use alternative Ubuntu mirror in Dockerfile"
    echo "Edit your Dockerfile and replace the RUN apt-get line with:"
    echo ""
    cat <<'DOCKERFILE'
RUN sed -i 's|http://archive.ubuntu.com|http://mirrors.kernel.org|g' /etc/apt/sources.list.d/ubuntu.sources && \
    apt-get update && apt-get install -y \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*
DOCKERFILE
    echo ""
fi

echo ""
echo "📚 Additional troubleshooting steps:"
echo "1. Check firewall: sudo ufw status"
echo "2. Check DNS resolution: docker run --rm ubuntu:24.04 cat /etc/resolv.conf"
echo "3. Test network: docker run --rm ubuntu:24.04 ping -c 3 8.8.8.8"
echo "4. View Docker logs: sudo journalctl -u docker -f"
echo ""
echo "✨ Done!"
