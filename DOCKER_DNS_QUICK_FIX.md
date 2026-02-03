# 🚨 Docker DNS Fix - Quick Reference Card

## Problem
```
Could not resolve 'archive.ubuntu.com'
E: Unable to fetch some archives
```

## Quick Fix (Copy-Paste on Server)

```bash
# 1. SSH to server
ssh user@your-server

# 2. Navigate to project
cd /opt/NonProfitFinance

# 3. Configure Docker DNS
sudo tee /etc/docker/daemon.json > /dev/null <<EOF
{
    "dns": ["8.8.8.8", "8.8.4.4", "1.1.1.1"]
}
EOF

# 4. Restart Docker
sudo systemctl restart docker

# 5. Test DNS
docker run --rm ubuntu:24.04 bash -c "apt-get update && echo 'DNS Working'"

# 6. Deploy application
git pull origin master
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# 7. Check status
docker-compose logs -f
```

## Alternative: Use Auto-Fix Script

```bash
# Pull latest code with fix scripts
cd /opt/NonProfitFinance
git pull origin master

# Run automated DNS fix
chmod +x fix-docker-dns.sh
sudo ./fix-docker-dns.sh

# Deploy with automated script
chmod +x deploy-remote-dns-fix.sh
sudo ./deploy-remote-dns-fix.sh
```

## Emergency: Build with Host Network

```bash
# Less secure but works immediately
docker build --network=host -t nonprofit-finance:latest .
docker-compose up -d
```

## Verify Fix

```bash
# Check Docker DNS config
cat /etc/docker/daemon.json

# Test from Docker
docker run --rm ubuntu:24.04 cat /etc/resolv.conf

# Test network
docker run --rm ubuntu:24.04 ping -c 3 8.8.8.8
```

## Troubleshooting

```bash
# View Docker logs
sudo journalctl -u docker -f

# Check firewall
sudo ufw status

# Check server DNS
cat /etc/resolv.conf

# Reset Docker network
sudo systemctl stop docker
sudo ip link delete docker0
sudo systemctl start docker
```

## Files Created for This Fix
- `fix-docker-dns.sh` - Automated DNS fix script
- `deploy-remote-dns-fix.sh` - Full deployment with DNS fix
- `Dockerfile.dns-fix` - Alternative Dockerfile with mirror fallback
- `DOCKER_DNS_FIX_GUIDE.md` - Complete documentation

## Success Indicators
✅ `daemon.json` has DNS configured  
✅ Docker service restarted without errors  
✅ Test container can run `apt-get update`  
✅ Application builds successfully  
✅ Containers start and pass health checks

---

**Need Help?** Check `DOCKER_DNS_FIX_GUIDE.md` for detailed explanations
