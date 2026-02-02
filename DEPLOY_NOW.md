# 🎯 EXECUTE THESE COMMANDS NOW

## 1️⃣ COMMIT TO GIT (Run in PowerShell/Terminal)
```powershell
git add .
git commit -m "feat: Add Docker deployment, fix global search and OCR form prefill"
git push origin master
```

## 2️⃣ BUILD & DEPLOY (Choose ONE method)

### ✨ EASY WAY (Automated Script)
```powershell
.\deploy-docker.ps1
```

### 🔧 MANUAL WAY (Step by Step)
```powershell
# Step 1: Build Docker image
docker build -t nonprofit-finance:latest .

# Step 2: Save image to file
docker save nonprofit-finance:latest -o nonprofit-finance.tar

# Step 3: Copy to your Docker server
scp nonprofit-finance.tar docker-compose.yml tech@192.168.100.107:~/

# Step 4: SSH into server and deploy
ssh tech@192.168.100.107

# On the server:
docker load -i ~/nonprofit-finance.tar
docker-compose down
docker-compose up -d
docker logs -f nonprofit-finance

# Press Ctrl+C to exit logs, then type 'exit' to leave SSH
```

## 3️⃣ VERIFY DEPLOYMENT
```powershell
# Check if container is running
ssh tech@192.168.100.107 "docker ps | grep nonprofit"

# View logs
ssh tech@192.168.100.107 "docker logs --tail 50 nonprofit-finance"
```

## 4️⃣ ACCESS APPLICATION
Open browser and go to:
```
http://192.168.100.107:7171
```

---

## ⚠️ TROUBLESHOOTING

### If SSH asks for password
```powershell
# Set up SSH key (one-time setup)
ssh-copy-id tech@192.168.100.107
```

### If port 7171 is busy
Edit `docker-compose.yml` line 7:
```yaml
ports:
  - "7171:8080"  # Change 7171 to another port like 8080
```

### If Docker isn't running on server
```powershell
ssh tech@192.168.100.107 "sudo systemctl start docker"
```

### If container fails to start
```powershell
# View full error logs
ssh tech@192.168.100.107 "docker logs nonprofit-finance"

# Check Docker server status
ssh tech@192.168.100.107 "docker info"
```

---

## 📊 WHAT HAPPENS DURING DEPLOYMENT

1. ✅ Git commits your code changes
2. ✅ Docker builds a containerized version of your app
3. ✅ Image is transferred to your Docker server
4. ✅ Old container is stopped (if exists)
5. ✅ New container starts on port 7171
6. ✅ Application is accessible at http://192.168.100.107:7171

---

## 🎉 SUCCESS INDICATORS

You'll know it worked when you see:
- ✅ "Deployment complete!" message
- ✅ Container shows as "Up" in `docker ps`
- ✅ Health check returns `Healthy`
- ✅ Browser loads the application at http://192.168.100.107:7171

---

**Ready to start? Run the commands in order!**

**Start with:** `git add .`
