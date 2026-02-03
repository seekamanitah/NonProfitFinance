#!/bin/bash
# Complete Docker cleanup and rebuild script for NonProfit Finance

set +e  # Don't exit on errors during cleanup

echo "🗑️  Complete Docker Cleanup and Rebuild"
echo "========================================"
echo ""
echo "⚠️  WARNING: This will DELETE ALL data in Docker containers!"
echo "   - All containers will be stopped and removed"
echo "   - All images will be removed"
echo "   - All volumes will be removed (DATABASE DATA DELETED)"
echo "   - All networks will be removed"
echo ""

# Ask for confirmation
read -p "Type 'DELETE' to confirm complete cleanup: " confirmation
if [ "$confirmation" != "DELETE" ]; then
    echo "❌ Cleanup cancelled."
    exit 1
fi

echo ""
echo "🛑 Step 1: Stopping all running containers..."
docker-compose down 2>/dev/null || true
docker stop $(docker ps -aq) 2>/dev/null || true
echo "✅ Containers stopped"

echo ""
echo "🗑️  Step 2: Removing all containers..."
docker rm -f $(docker ps -aq) 2>/dev/null || true
echo "✅ Containers removed"

echo ""
echo "🗑️  Step 3: Removing NonProfit Finance images..."
docker rmi -f $(docker images "nonprofit-finance*" -q) 2>/dev/null || true
docker rmi -f nonprofit-finance:latest 2>/dev/null || true
echo "✅ Images removed"

echo ""
echo "🗑️  Step 4: Removing volumes (DATABASE DATA DELETED)..."
docker volume rm $(docker volume ls -q) 2>/dev/null || true
docker volume prune -f 2>/dev/null || true
echo "✅ Volumes removed"

echo ""
echo "🗑️  Step 5: Removing networks..."
docker network prune -f 2>/dev/null || true
echo "✅ Networks removed"

echo ""
echo "🧹 Step 6: Running Docker system prune..."
docker system prune -af --volumes 2>/dev/null || true
echo "✅ System cleaned"

echo ""
echo "📊 Step 7: Verifying cleanup..."
echo ""
echo "Remaining containers:"
docker ps -a
echo ""
echo "Remaining images:"
docker images
echo ""
echo "Remaining volumes:"
docker volume ls
echo ""

echo "✅ Complete cleanup finished!"
echo ""
echo "🏗️  Step 8: Rebuilding from scratch..."
echo ""

# Set strict mode for build
set -e

# Rebuild
echo "Building fresh Docker image..."
docker-compose build --no-cache

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    
    echo ""
    echo "🚀 Step 9: Starting containers..."
    docker-compose up -d
    
    if [ $? -eq 0 ]; then
        echo "✅ Containers started!"
        
        echo ""
        echo "⏳ Waiting for application to start (30 seconds)..."
        sleep 30
        
        echo ""
        echo "📊 Final Status:"
        docker-compose ps
        
        echo ""
        echo "✨ Complete rebuild finished!"
        echo ""
        echo "📋 Next steps:"
        echo "  1. Access application: http://localhost:8080"
        echo "  2. View logs: docker-compose logs -f"
        echo "  3. Check health: curl http://localhost:8080/health"
        echo ""
        echo "⚠️  Note: All data has been reset. You'll need to:"
        echo "     - Create a new account"
        echo "     - Configure organization settings"
        echo "     - Set up categories/accounts"
        echo "     - Or load demo data"
    else
        echo "❌ Failed to start containers"
        echo "Check logs: docker-compose logs"
        exit 1
    fi
else
    echo "❌ Build failed"
    echo "Check the output above for errors"
    exit 1
fi

echo ""
echo "🎉 All done! Fresh Docker environment ready."
