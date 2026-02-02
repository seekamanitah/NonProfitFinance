# Docker Update Quick Reference

## 🚀 Quick Commands

### Local Docker Update
```powershell
.\update-docker-remote.ps1 -Local
```

### Remote Docker Update
```powershell
# Default (localhost)
.\update-docker-remote.ps1

# Specific host
.\update-docker-remote.ps1 -DockerHost "your-server.com" -SshUser "admin"

# Custom SSH port
.\update-docker-remote.ps1 -DockerHost "your-server.com" -SshPort 2222
```

---

## 📋 Full Deployment Process

### Step 1: Commit and Push
```powershell
.\git-commit-fixes.ps1
```

This will:
- Add all changes
- Create detailed commit message
- Push to GitHub master branch
- Display commit SHA

### Step 2: Update Docker Container

**Option A: Local Docker**
```powershell
.\update-docker-remote.ps1 -Local
```

**Option B: Remote Docker via SSH**
```powershell
.\update-docker-remote.ps1 -DockerHost "your.docker.host" -SshUser "root"
```

**Option C: Manual Remote Update**
SSH into your Docker host and run:
```bash
cd /opt/nonprofit-finance
docker-compose down
git pull origin master
rm -f NonProfitFinance.db*
docker-compose up -d --build
```

---

## 🔍 What Gets Updated

### Database Changes
- ✅ New schema with fixed RowVersion configuration
- ✅ Automatic schema detection and recreation
- ✅ Fresh seed data (categories, funds, locations)
- ⚠️ **All existing data will be lost** (fresh start)

### Code Changes
- ✅ Transaction filter auto-apply
- ✅ Fund form input fixes
- ✅ Import error handling improvements
- ✅ Global search enhancements
- ✅ Error page fixes

---

## 🐛 Troubleshooting

### Issue: "Database locked" error
```powershell
# Stop containers first
docker-compose down

# Remove database
Remove-Item NonProfitFinance.db* -Force

# Restart
docker-compose up -d --build
```

### Issue: SSH connection failed
```powershell
# Test SSH connection
ssh -p 22 user@your-server.com

# Check SSH key
ssh-add -l

# Add SSH key if needed
ssh-add ~/.ssh/id_rsa
```

### Issue: Port already in use
```powershell
# Check what's using port 5000
netstat -ano | findstr :5000

# Kill the process or change port in docker-compose.yml
```

### Issue: Container won't start
```powershell
# View logs
docker-compose logs -f

# Check container status
docker-compose ps

# Rebuild from scratch
docker-compose down -v
docker-compose up -d --build
```

---

## 📊 Verify Deployment

### Check Container Status
```powershell
docker-compose ps
```

### View Live Logs
```powershell
docker-compose logs -f
```

### Test Health Endpoint
```powershell
# Local
curl http://localhost:5000/health

# Remote
curl http://your-server.com:5000/health
```

### Access Application
- **Local:** http://localhost:5000
- **Remote:** http://your-server-ip:5000

---

## ⏱️ Expected Timing

1. **Git commit & push:** ~10 seconds
2. **Docker rebuild:** ~2-3 minutes (first time), ~30 seconds (cached)
3. **Database creation:** ~5 seconds
4. **Total time:** ~3-5 minutes

---

## 🔐 Security Notes

- Database is recreated = all old data removed
- SSH keys recommended over passwords
- Use firewall rules to restrict port 5000 access
- Consider using reverse proxy (nginx) for production

---

## 💾 Backup Strategy

**Before updating, backup existing data:**

```powershell
# Backup database
docker cp nonprofit-finance:/app/NonProfitFinance.db ./backup-$(Get-Date -Format 'yyyy-MM-dd').db

# Backup documents
docker cp nonprofit-finance:/app/documents ./documents-backup
```

**Restore if needed:**
```powershell
docker cp ./backup-2025-01-29.db nonprofit-finance:/app/NonProfitFinance.db
docker-compose restart
```

---

## 📝 Post-Update Checklist

- [ ] Container is running (`docker-compose ps`)
- [ ] No errors in logs (`docker-compose logs`)
- [ ] Application accessible via browser
- [ ] Login page loads
- [ ] Dashboard displays with seed data
- [ ] Can create test transaction
- [ ] Import/Export works
- [ ] No console errors in browser

---

## 🆘 Emergency Rollback

If the new version has issues:

```powershell
# Stop current version
docker-compose down

# Checkout previous commit
git log --oneline -5  # Find previous commit SHA
git checkout <previous-sha>

# Rebuild
docker-compose up -d --build
```

Or restore from backup:
```powershell
docker cp ./backup-<date>.db nonprofit-finance:/app/NonProfitFinance.db
docker-compose restart
```
