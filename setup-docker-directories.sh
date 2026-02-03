#!/bin/bash
# Setup script for Docker bind mount directories

echo "🔧 Setting up Docker bind mount directories"
echo "==========================================="
echo ""

# Create directories if they don't exist
echo "📁 Creating directories in /media/AppDatabases/NonProfitFinance/..."
sudo mkdir -p /media/AppDatabases/NonProfitFinance/data
sudo mkdir -p /media/AppDatabases/NonProfitFinance/backups
sudo mkdir -p /media/AppDatabases/NonProfitFinance/documents

# Set permissions (allow Docker to write)
echo "🔐 Setting permissions..."
sudo chmod -R 777 /media/AppDatabases/NonProfitFinance/data
sudo chmod -R 777 /media/AppDatabases/NonProfitFinance/backups
sudo chmod -R 777 /media/AppDatabases/NonProfitFinance/documents

# Verify
echo ""
echo "✅ Directories created and configured:"
ls -lah /media/AppDatabases/NonProfitFinance/

echo ""
echo "📊 Disk space available:"
df -h /media/AppDatabases

echo ""
echo "✨ Setup complete! Now run:"
echo "   docker-compose up -d"
