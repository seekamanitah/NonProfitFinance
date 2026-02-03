# 🗑️ Complete Docker Cleanup and Rebuild Guide

## ⚠️ WARNING - DATA LOSS
This will **PERMANENTLY DELETE**:
- All Docker containers
- All Docker images
- All Docker volumes (**DATABASE DATA LOST**)
- All Docker networks
- ALL application data (transactions, accounts, users, etc.)

## When to Use This
- Docker environment is corrupted
- Need a completely fresh start
- Testing from scratch
- Preparing for clean deployment
- Resolving persistent Docker issues

## Quick Start

### Windows (PowerShell)
```powershell
# Navigate to project directory
cd C:\Users\tech\source\repos\NonProfitFinance

# Run cleanup and rebuild script
.\docker-clean-rebuild.ps1
```

### Linux/Remote Server (Bash)
```bash
# Navigate to project directory
cd /opt/NonProfitFinance

# Make script executable
chmod +x docker-clean-rebuild.sh

# Run cleanup and rebuild (needs sudo for some operations)
sudo ./docker-clean-rebuild.sh
```

## Manual Step-by-Step

### Step 1: Stop Everything
```bash
# Stop containers via docker-compose
docker-compose down

# Stop all running containers
docker stop $(docker ps -aq)
```

### Step 2: Remove All Containers
```bash
# Remove all containers (running and stopped)
docker rm -f $(docker ps -aq)
```

### Step 3: Remove Images
```bash
# Remove all NonProfit Finance images
docker rmi -f $(docker images "nonprofit-finance*" -q)
docker rmi -f nonprofit-finance:latest

# Or remove ALL images (nuclear option)
docker rmi -f $(docker images -q)
```

### Step 4: Remove Volumes (⚠️ DATA LOSS)
```bash
# Remove all volumes (THIS DELETES DATABASE)
docker volume rm $(docker volume ls -q)

# Or use prune
docker volume prune -f
```

### Step 5: Remove Networks
```bash
# Remove all unused networks
docker network prune -f
```

### Step 6: System Prune (Everything)
```bash
# Remove everything unused
docker system prune -af --volumes

# Explanation:
# -a = remove all unused images (not just dangling)
# -f = force (no confirmation)
# --volumes = remove volumes too
```

### Step 7: Verify Clean State
```bash
# Check for remaining containers
docker ps -a

# Check for remaining images
docker images

# Check for remaining volumes
docker volume ls

# Check for remaining networks
docker network ls
```

### Step 8: Rebuild from Scratch
```bash
# Build with no cache
docker-compose build --no-cache

# Start containers
docker-compose up -d

# Watch logs
docker-compose logs -f
```

## Verification Commands

### Check Everything is Removed
```bash
# Should show nothing (or only system containers)
docker ps -a

# Should show only base images (ubuntu, etc.)
docker images

# Should be empty
docker volume ls

# Should only show default networks
docker network ls
```

### Check Rebuild Success
```bash
# Should show container running
docker-compose ps

# Should show healthy status
docker-compose ps

# Test application
curl http://localhost:8080/health
```

## Troubleshooting

### Volumes Won't Delete
```bash
# Stop all containers first
docker stop $(docker ps -aq)

# Try removing volumes again
docker volume prune -f

# Force remove specific volume
docker volume rm -f volumename
```

### Images Won't Delete
```bash
# Check what's using the image
docker ps -a --filter ancestor=nonprofit-finance:latest

# Remove containers using the image
docker rm -f $(docker ps -a --filter ancestor=nonprofit-finance:latest -q)

# Try removing image again
docker rmi -f nonprofit-finance:latest
```

### Permission Denied (Linux)
```bash
# Run with sudo
sudo docker-compose down
sudo docker system prune -af --volumes
```

### Containers Keep Restarting
```bash
# Stop with force
docker kill $(docker ps -q)

# Or stop Docker service
sudo systemctl stop docker
sudo systemctl start docker
```

## Post-Cleanup Steps

After complete cleanup and rebuild:

### 1. Access Application
```
http://localhost:8080
```

### 2. Create New Account
- No existing users (database wiped)
- Register new admin account
- Set up organization profile

### 3. Configure Application
- Set organization name/details
- Configure fiscal year
- Set tax rates
- Configure backup settings

### 4. Set Up Data Structure
- Create accounts/funds
- Set up categories
- Add donors/grants (if applicable)
- Configure budgets

### 5. Load Data
- **Option A**: Load demo data from Settings
- **Option B**: Import data from CSV/Excel
- **Option C**: Restore from backup (if available)

## Automated Scripts Comparison

### PowerShell Script (`docker-clean-rebuild.ps1`)
- ✅ Windows-friendly
- ✅ Colored output
- ✅ Progress indicators
- ✅ Automatic rebuild
- ✅ Health check wait
- ✅ Final status report

### Bash Script (`docker-clean-rebuild.sh`)
- ✅ Linux/Unix compatible
- ✅ Remote server ready
- ✅ Sudo-friendly
- ✅ Automatic rebuild
- ✅ Health check wait
- ✅ Final status report

Both scripts:
- ⚠️ Require typing "DELETE" to confirm
- 🛑 Stop all containers
- 🗑️ Remove containers, images, volumes, networks
- 🧹 Run system prune
- 📊 Verify cleanup
- 🏗️ Rebuild with --no-cache
- 🚀 Start containers
- ⏳ Wait for startup
- ✅ Show final status

## Quick Reference Commands

### Just Remove Containers
```bash
docker-compose down
docker rm $(docker ps -aq)
```

### Just Remove Images
```bash
docker rmi $(docker images "nonprofit-finance*" -q)
```

### Just Remove Volumes (⚠️ DATA LOSS)
```bash
docker volume prune -f
```

### Nuclear Option (Everything)
```bash
docker system prune -af --volumes
```

### Rebuild Only
```bash
docker-compose build --no-cache
docker-compose up -d
```

## Safety Tips

### Before Complete Cleanup

1. **Backup Data** (if needed):
   ```bash
   # Export data from application
   # Settings > Backup & Restore > Create Backup
   
   # Or copy database file
   docker cp <container-id>:/app/data/app.db ./backup.db
   ```

2. **Save Configuration**:
   - Export organization settings
   - Save import presets
   - Note category structure

3. **Document Custom Setup**:
   - Special accounts
   - Custom categories
   - Budget configurations

### After Rebuild

1. **Verify Health**:
   ```bash
   docker-compose ps
   docker-compose logs --tail=50
   curl http://localhost:8080/health
   ```

2. **Test Basic Functions**:
   - Create account
   - Add transaction
   - Run report
   - Test import/export

3. **Restore Data** (if backed up):
   - Use application backup restore
   - Or import CSV data

## Common Use Cases

### 1. Fix Corrupted Database
```bash
./docker-clean-rebuild.ps1
# Then restore from backup
```

### 2. Test Fresh Install
```bash
./docker-clean-rebuild.ps1
# Test with demo data
```

### 3. Before Production Deployment
```bash
./docker-clean-rebuild.sh
# Deploy clean environment
```

### 4. Development Reset
```bash
docker-compose down -v
docker-compose up -d --build
```

## Alternative: Soft Reset (Keep Images)

If you just want to reset data but keep images:

```bash
# Stop and remove containers
docker-compose down -v

# Remove volumes only
docker volume prune -f

# Start fresh (uses existing image)
docker-compose up -d
```

## Docker Disk Space

### Check Disk Usage
```bash
docker system df
```

### Free Up Space
```bash
# Remove unused data
docker system prune -a

# Remove with volumes
docker system prune -af --volumes
```

## Success Indicators

After cleanup and rebuild, you should see:

✅ No old containers: `docker ps -a` shows only new container  
✅ Fresh image: `docker images` shows recently built image  
✅ Clean volumes: `docker volume ls` shows only new volumes  
✅ Container running: `docker-compose ps` shows "Up" status  
✅ Health check passes: Curl to `/health` returns 200  
✅ Application accessible: Can access login page  
✅ No errors in logs: `docker-compose logs` clean  

---

**Last Updated:** January 2025  
**Risk Level:** 🔴 HIGH - DATA LOSS  
**Backup Required:** Yes (if preserving data)  
**Time Required:** 5-10 minutes  
**Difficulty:** Easy (automated scripts)
