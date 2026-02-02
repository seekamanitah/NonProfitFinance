#!/bin/bash

# NonProfit Finance Docker Deployment Script
# Usage: ./deploy-docker.sh

echo "🚀 NonProfit Finance Docker Deployment"
echo "======================================"

# Configuration
DOCKER_HOST="192.168.100.107"
DOCKER_PORT="7171"
IMAGE_NAME="nonprofit-finance"
IMAGE_TAG="latest"

echo ""
echo "📦 Step 1: Building Docker image..."
docker build -t ${IMAGE_NAME}:${IMAGE_TAG} .

if [ $? -ne 0 ]; then
    echo "❌ Docker build failed!"
    exit 1
fi

echo ""
echo "✅ Image built successfully!"
echo ""
echo "💾 Step 2: Saving image to tar file..."
docker save ${IMAGE_NAME}:${IMAGE_TAG} -o ${IMAGE_NAME}.tar

echo ""
echo "📤 Step 3: Copying image to Docker server..."
scp ${IMAGE_NAME}.tar docker-compose.yml tech@${DOCKER_HOST}:~/

if [ $? -ne 0 ]; then
    echo "❌ Failed to copy files to server!"
    echo "⚠️  Make sure you have SSH access to tech@${DOCKER_HOST}"
    echo "⚠️  You may need to set up SSH keys: ssh-copy-id tech@${DOCKER_HOST}"
    exit 1
fi

echo ""
echo "🔄 Step 4: Loading image on remote server..."
ssh tech@${DOCKER_HOST} "docker load -i ~/${IMAGE_NAME}.tar"

echo ""
echo "🚀 Step 5: Starting container..."
ssh tech@${DOCKER_HOST} "cd ~ && docker-compose down && docker-compose up -d"

echo ""
echo "🧹 Step 6: Cleaning up..."
rm ${IMAGE_NAME}.tar
ssh tech@${DOCKER_HOST} "rm ~/${IMAGE_NAME}.tar"

echo ""
echo "✅ Deployment complete!"
echo ""
echo "📊 Application Status:"
ssh tech@${DOCKER_HOST} "docker ps | grep nonprofit"

echo ""
echo "🌐 Access your application at:"
echo "   http://${DOCKER_HOST}:${DOCKER_PORT}"
echo ""
echo "📝 To view logs:"
echo "   ssh tech@${DOCKER_HOST} 'docker logs -f nonprofit-finance'"
echo ""
echo "🔄 To restart:"
echo "   ssh tech@${DOCKER_HOST} 'docker-compose restart'"
echo ""
