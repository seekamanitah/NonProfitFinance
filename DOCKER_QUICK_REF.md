# 🚀 Docker Deployment - Quick Commands

## ONE-COMMAND DEPLOYMENT (PowerShell)
```powershell
.\deploy-docker.ps1
```

## GIT COMMANDS
```bash
git add .
git commit -m "feat: UX fixes and Docker deployment"
git push origin master
```

## DOCKER BUILD (Local)
```bash
docker build -t nonprofit-finance:latest .
```

## DOCKER DEPLOY (Manual)
```bash
# 1. Save image
docker save nonprofit-finance:latest -o nonprofit-finance.tar

# 2. Copy to server
scp nonprofit-finance.tar docker-compose.yml tech@192.168.100.107:~/

# 3. Deploy on server
ssh tech@192.168.100.107
docker load -i ~/nonprofit-finance.tar
docker-compose down
docker-compose up -d
docker logs -f nonprofit-finance
```

## ACCESS APPLICATION
```
http://192.168.100.107:7171
```

## COMMON MANAGEMENT
```bash
# View logs
ssh tech@192.168.100.107 "docker logs -f nonprofit-finance"

# Restart
ssh tech@192.168.100.107 "docker-compose restart"

# Stop
ssh tech@192.168.100.107 "docker-compose down"

# Status
ssh tech@192.168.100.107 "docker ps | grep nonprofit"
```

## BACKUP DATABASE
```bash
ssh tech@192.168.100.107 "docker cp nonprofit-finance:/app/data/NonProfitFinance.db ~/backup-$(date +%Y%m%d).db"
```

---
**Server:** 192.168.100.107:7171  
**User:** tech  
**Image:** nonprofit-finance:latest
