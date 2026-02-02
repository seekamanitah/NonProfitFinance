# ✅ BACKUP SERVICE FIX - Base64 Encryption Key

## 🔴 The Problem

BackupService is trying to read `Backup:EncryptionKey` from configuration but it's missing or invalid.

Error in `BackupService.cs:line 36`:
```csharp
_encryptionKey = Convert.FromBase64String(configuredKey);
// ERROR: "The input is not a valid Base-64 string"
```

## ✅ The Solution

Add `Backup:EncryptionKey` to your `.env` file with a valid Base64 string.

---

## 🚀 FIX - Run These Commands on Docker Server

### Step 1: Create/Update .env file

```bash
ssh tech@192.168.100.107
cd ~/projects/NonProfitFinance

# Create .env from template
cp .env.example .env

# Edit it
nano .env
```

### Step 2: Add These Lines to .env

Make sure you have BOTH:

```env
# Security key for ASP.NET Data Protection
ASPNETCORE_SECURITY_KEY=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==

# Backup encryption key (use same Base64 value)
Backup:EncryptionKey=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==
```

The key above is pre-configured. You can generate your own with:
```bash
openssl rand -base64 32
```

### Step 3: Save and Restart

```bash
# Save file (Ctrl+O, Enter, Ctrl+X if using nano)

# Restart container
docker-compose down
docker-compose up -d

# Wait 10 seconds
sleep 10

# Check logs
docker logs nonprofit-finance
```

---

## ✅ Expected Result

You should NO LONGER see:
```
fail: NonProfitFinance.Services.BackupHostedService[0]
      Error in backup scheduler
      System.FormatException: The input is not a valid Base-64 string
```

Instead, you should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:8080
```

---

## 🌐 Access Application

```
http://192.168.100.107:7171
```

---

## 📋 Complete .env Example

```env
PORT=7171
COMPOSE_PROJECT_NAME=nonprofit-finance
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DB_CONNECTION_STRING=Data Source=/app/data/NonProfitFinance.db
ASPNETCORE_SECURITY_KEY=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==
Backup:EncryptionKey=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==
SMTP_ENABLED=false
OCR_ENABLED=true
TTS_ENABLED=true
BACKUP_ENABLED=true
BACKUP_INTERVAL=86400
BACKUP_RETENTION_DAYS=30
BACKUP_DIRECTORY=/app/backups
LOGGING_LEVEL=Information
ENABLE_DEMO_DATA=true
ENABLE_INVENTORY_MODULE=true
```

---

## ✅ Verification

```bash
# Check if .env exists
ls -la /opt/NonProfitFinance/.env

# Verify keys are set
cat .env | grep -E "ASPNETCORE_SECURITY_KEY|Backup:EncryptionKey"

# Should show:
# ASPNETCORE_SECURITY_KEY=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==
# Backup:EncryptionKey=QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==
```

---

**Status**: ✅ **READY TO DEPLOY**  
**Next**: Run the commands above and access http://192.168.100.107:7171 🚀
