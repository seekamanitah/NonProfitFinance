#!/bin/bash
# Git Commit Script for Docker & Environment Files

# Configure Git identity (one-time setup)
git config --global user.email "tech@firefighter.local"
git config --global user.name "Tech Admin"

# Navigate to project
cd /path/to/NonProfitFinance

# Stage the recreated files
git add docker-compose.yml
git add .env.example
git add .gitignore

# Commit changes
git commit -m "refactor: Recreate docker-compose with .env support and improve environment variable handling

- Updated docker-compose.yml with env_file support for .env
- Added PORT and DB_CONNECTION_STRING environment variable substitution
- Recreated .env.example with comprehensive variable documentation
- Updated .gitignore to properly exclude .env files and secrets
- Added database file exclusions (*.db, *.db-journal, etc.)
- Added docker build artifacts to gitignore
- Implements production-ready secrets management"

# Push to GitHub
git push origin master

echo "✅ Git commit complete!"
echo "📊 Changes pushed to: https://github.com/seekamanitah/NonProfitFinance"
