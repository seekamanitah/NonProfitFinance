# Docker DNS Resolution Fix

## 🚨 Problem
Docker build fails with DNS resolution errors:
```
Could not resolve 'archive.ubuntu.com' [IP: 91.189.91.83 80]
E: Unable to fetch some archives
```

## 🔍 Root Cause
Docker containers on your remote server cannot resolve DNS hostnames. This prevents `apt-get` from downloading packages during the build process.

## ✅ Solution Options

### **Option 1: Fix Docker DNS Configuration (RECOMMENDED)**

Run this script on your remote server:

```bash
chmod +x fix-docker-dns.sh
sudo ./fix-docker-dns.sh
```

This script will:
1. Configure Docker daemon with Google DNS (8.8.8.8, 8.8.4.4, 1.1.1.1)
2. Restart Docker service
3. Test DNS resolution
4. Verify the fix

**Manual steps if you prefer:**

```bash
# 1. Edit Docker daemon config
sudo nano /etc/docker/daemon.json

# 2. Add this content:
{
    "dns": ["8.8.8.8", "8.8.4.4", "1.1.1.1"]
}

# 3. Restart Docker
sudo systemctl restart docker

# 4. Test
docker run --rm ubuntu:24.04 bash -c "apt-get update && echo 'DNS Working'"
```

---

### **Option 2: Use Updated Dockerfile with Alternative Mirrors**

I've created an updated Dockerfile that:
- Uses multiple Ubuntu mirrors
- Has retry logic for apt-get
- Falls back to alternative mirrors if primary fails
- More resilient to network issues

**File:** `Dockerfile.dns-fix`

Use it with:
```bash
docker-compose build --no-cache
# or
docker build -f Dockerfile.dns-fix -t nonprofit-finance:latest .
```

---

### **Option 3: Build with Host Network (Quick Fix)**

**⚠️ Warning:** Less secure, but works immediately

```bash
docker build --network=host -t nonprofit-finance:latest .
```

---

### **Option 4: Use Build Arguments for Custom Mirror**

```bash
docker build --build-arg UBUNTU_MIRROR=http://mirrors.kernel.org/ubuntu -t nonprofit-finance:latest .
```

---

## 🧪 Testing the Fix

After applying any solution, test with:

```bash
# Test DNS in Docker
docker run --rm ubuntu:24.04 bash -c "apt-get update && apt-get install -y curl && curl -I https://archive.ubuntu.com"

# Build your app
cd /opt/NonProfitFinance
docker-compose build --no-cache
docker-compose up -d
```

---

## 🔧 Troubleshooting

### Check Docker DNS Configuration
```bash
docker run --rm ubuntu:24.04 cat /etc/resolv.conf
```

Should show:
```
nameserver 8.8.8.8
nameserver 8.8.4.4
nameserver 1.1.1.1
```

### Check Server DNS
```bash
cat /etc/resolv.conf
systemd-resolve --status
```

### Check Firewall
```bash
sudo ufw status
sudo iptables -L
```

### Check Network Connectivity
```bash
# Ping Google DNS
ping -c 3 8.8.8.8

# Test from Docker
docker run --rm ubuntu:24.04 ping -c 3 8.8.8.8
```

### View Docker Logs
```bash
sudo journalctl -u docker -f
```

---

## 📋 Common Causes

1. **Server DNS not configured properly**
   - Fix: Update `/etc/resolv.conf` on host

2. **Firewall blocking DNS**
   - Fix: Allow port 53 (DNS)
   - `sudo ufw allow 53/tcp && sudo ufw allow 53/udp`

3. **Corporate proxy/firewall**
   - Fix: Configure Docker to use proxy
   - Add to `/etc/docker/daemon.json`:
   ```json
   {
       "proxies": {
           "default": {
               "httpProxy": "http://proxy.example.com:80",
               "httpsProxy": "http://proxy.example.com:80"
           }
       }
   }
   ```

4. **Docker network driver issues**
   - Fix: Reset Docker networking
   ```bash
   sudo systemctl stop docker
   sudo ip link delete docker0
   sudo systemctl start docker
   ```

---

## 🚀 Quick Start (Most Common Fix)

```bash
# SSH into remote server
ssh user@your-server

# Navigate to project
cd /opt/NonProfitFinance

# Run the DNS fix script
chmod +x fix-docker-dns.sh
sudo ./fix-docker-dns.sh

# Pull latest code
git pull origin master

# Rebuild with no cache
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Check logs
docker-compose logs -f
```

---

## ✅ Verification Checklist

- [ ] Docker daemon has DNS configured in `/etc/docker/daemon.json`
- [ ] Docker service restarted successfully
- [ ] Test container can resolve hostnames
- [ ] Test container can run `apt-get update`
- [ ] Application builds without errors
- [ ] Application starts and runs correctly

---

## 📞 Still Having Issues?

If problems persist after trying all solutions:

1. **Check if it's a temporary Ubuntu mirror issue:**
   ```bash
   curl -I http://archive.ubuntu.com
   ```

2. **Try a different mirror in Dockerfile:**
   - `http://mirrors.kernel.org/ubuntu`
   - `http://us.archive.ubuntu.com/ubuntu`
   - `http://mirror.math.princeton.edu/pub/ubuntu`

3. **Consider using the updated Dockerfile.dns-fix** which handles these issues automatically

4. **Contact your hosting provider** - they may have DNS or firewall restrictions

---

**Last Updated:** January 2025  
**Status:** Ready to Deploy  
**Tested On:** Ubuntu 24.04 LTS with Docker 24.x
