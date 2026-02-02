# Docker Deployment Guide - NonProfit Finance

## 🚀 Quick Start

### Prerequisites on Your Development Machine
- Git installed
- Docker Desktop installed and running
- SSH access to your Docker server (192.168.100.107)

### Prerequisites on Docker Server (192.168.100.107)
- Docker installed
- Docker Compose installed
- Port 7171 available

---

## 📋 Step-by-Step Deployment

### Step 1: Commit Your Changes to Git

```bash
# Check what files have changed
git status

# Add all changed files
git add .

# Commit with a descriptive message
git commit -m "feat: Add UX fixes - Global search, OCR form prefill, and accessibility improvements"

# Push to GitHub
git push origin master
```

### Step 2: Build Docker Image Locally

```bash
# Build the image
docker build -t nonprofit-finance:latest .

# Verify the image was created
docker images | grep nonprofit-finance
```

### Step 3: Deploy to Docker Server

#### Option A: Using PowerShell (Windows)

```powershell
# Make sure you're in the project directory
cd C:\Users\tech\source\repos\NonProfitFinance

# Run the deployment script
.\deploy-docker.ps1
```

#### Option B: Using Bash (Linux/Mac/WSL)

```bash
# Make the script executable
chmod +x deploy-docker.sh

# Run the deployment script
./deploy-docker.sh
```

#### Option C: Manual Deployment

If the scripts don't work, follow these manual steps:

1. **Build and save image locally:**
   ```bash
   docker build -t nonprofit-finance:latest .
   docker save nonprofit-finance:latest -o nonprofit-finance.tar
   ```

2. **Copy files to server:**
   ```bash
   scp nonprofit-finance.tar docker-compose.yml tech@192.168.100.107:~/
   ```

3. **SSH into server and deploy:**
   ```bash
   ssh tech@192.168.100.107
   
   # Load the image
   docker load -i ~/nonprofit-finance.tar
   
   # Stop existing container (if any)
   docker-compose down
   
   # Start new container
   docker-compose up -d
   
   # Check status
   docker ps
   docker logs -f nonprofit-finance
   ```

---

## 🔍 Verify Deployment

### 1. Check Container Status
```bash
ssh tech@192.168.100.107 "docker ps | grep nonprofit"
```

### 2. View Logs
```bash
ssh tech@192.168.100.107 "docker logs -f nonprofit-finance"
```

### 3. Access Application
Open browser and navigate to:
```
http://192.168.100.107:7171
```

### 4. Check Health Endpoint
```bash
curl http://192.168.100.107:7171/health
```

---

## 📊 Docker Management Commands

### On Your Docker Server (SSH in first)

```bash
# SSH into server
ssh tech@192.168.100.107

# View running containers
docker ps

# View all containers (including stopped)
docker ps -a

# View logs (follow mode)
docker logs -f nonprofit-finance

# View last 100 lines of logs
docker logs --tail 100 nonprofit-finance

# Restart container
docker-compose restart

# Stop container
docker-compose down

# Start container
docker-compose up -d

# Rebuild and restart
docker-compose up -d --build

# View resource usage
docker stats nonprofit-finance

# Execute command in container
docker exec -it nonprofit-finance bash

# View volumes
docker volume ls

# Inspect container
docker inspect nonprofit-finance
```

---

## 💾 Data Persistence

Your application data is stored in Docker volumes:

| Volume | Purpose | Location in Container |
|--------|---------|----------------------|
| `nonprofit-data` | SQLite database | `/app/data/NonProfitFinance.db` |
| `nonprofit-backups` | Automatic backups | `/app/backups/` |
| `nonprofit-documents` | Uploaded files | `/app/documents/` |

### Backup Volumes

```bash
# Backup database
docker cp nonprofit-finance:/app/data/NonProfitFinance.db ./backup-$(date +%Y%m%d).db

# Backup all data
docker run --rm -v nonprofit-data:/data -v $(pwd):/backup alpine tar czf /backup/data-backup.tar.gz /data
```

### Restore Volumes

```bash
# Restore database
docker cp backup-20260129.db nonprofit-finance:/app/data/NonProfitFinance.db
docker-compose restart
```

---

## 🔧 Troubleshooting

### Problem: Can't connect to server
```bash
# Test SSH connection
ssh tech@192.168.100.107

# If prompted for password, set up SSH keys:
ssh-copy-id tech@192.168.100.107
```

### Problem: Port 7171 already in use
```bash
# Check what's using the port
ssh tech@192.168.100.107 "sudo netstat -tlnp | grep 7171"

# Stop conflicting service or change port in docker-compose.yml
```

### Problem: Container won't start
```bash
# View detailed logs
ssh tech@192.168.100.107 "docker logs nonprofit-finance"

# Check container state
ssh tech@192.168.100.107 "docker inspect nonprofit-finance"

# Remove and recreate
ssh tech@192.168.100.107 "docker-compose down && docker-compose up -d"
```

### Problem: Database corruption
```bash
# Restore from backup
ssh tech@192.168.100.107
cd /var/lib/docker/volumes/nonprofit-data/_data
# Copy your backup database here
docker-compose restart
```

---

## 🔄 Update Deployment

When you make code changes:

1. **Commit changes to Git**
   ```bash
   git add .
   git commit -m "Your commit message"
   git push
   ```

2. **Rebuild and redeploy**
   ```bash
   # Run deployment script again
   .\deploy-docker.ps1
   
   # OR manually:
   docker build -t nonprofit-finance:latest .
   # ... follow manual deployment steps
   ```

---

## 🌐 Production Considerations

### 1. Use HTTPS
Consider setting up a reverse proxy (nginx) with SSL:

```yaml
# nginx.conf example
server {
    listen 443 ssl;
    server_name finance.example.com;
    
    ssl_certificate /etc/ssl/certs/cert.pem;
    ssl_certificate_key /etc/ssl/private/key.pem;
    
    location / {
        proxy_pass http://localhost:7171;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 2. Automated Backups
Add to crontab on Docker server:

```bash
# Backup database daily at 2 AM
0 2 * * * docker exec nonprofit-finance sqlite3 /app/data/NonProfitFinance.db ".backup '/app/backups/auto-backup-$(date +\%Y\%m\%d).db'"
```

### 3. Monitoring
Set up monitoring with:
- Uptime Kuma
- Grafana + Prometheus
- Simple health check script

### 4. Firewall
```bash
# Allow only specific IPs to access port 7171
sudo ufw allow from 192.168.100.0/24 to any port 7171
```

---

## 📞 Support

### Common URLs
- Application: `http://192.168.100.107:7171`
- Health Check: `http://192.168.100.107:7171/health`
- GitHub Repo: `https://github.com/seekamanitah/NonProfitFinance`

### Quick Reference
```bash
# Full restart
ssh tech@192.168.100.107 "docker-compose down && docker-compose up -d"

# View live logs
ssh tech@192.168.100.107 "docker logs -f nonprofit-finance"

# Check resource usage
ssh tech@192.168.100.107 "docker stats --no-stream nonprofit-finance"
```

---

**Last Updated:** 2026-01-29  
**Version:** 1.0.0  
**Docker Image:** nonprofit-finance:latest  
**Deployment Target:** 192.168.100.107:7171
