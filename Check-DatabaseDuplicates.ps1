# Database Duplicate Checker
# Checks for duplicate entries that cause import failures

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Duplicate Data Checker" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

$dbPath = "NonProfitFinance.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Database not found at $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "`nChecking for duplicate categories..." -ForegroundColor Yellow

# You'll need to install System.Data.SQLite if not already installed
Add-Type -Path "C:\Program Files\PackageManagement\NuGet\Packages\System.Data.SQLite.Core.1.0.118\lib\net46\System.Data.SQLite.dll" -ErrorAction SilentlyContinue

try {
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    # Check Categories
    $query = @"
    SELECT 
        LOWER(Name) as LowerName,
        COUNT(*) as Count,
        GROUP_CONCAT(Id || ':' || Name) as Details
    FROM Categories
    GROUP BY LOWER(Name)
    HAVING COUNT(*) > 1
    ORDER BY Count DESC
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()
    
    $hasDuplicates = $false
    Write-Host "`nDuplicate Categories Found:" -ForegroundColor Red
    Write-Host "----------------------------" -ForegroundColor Gray
    
    while ($reader.Read()) {
        $hasDuplicates = $true
        $name = $reader["LowerName"]
        $count = $reader["Count"]
        $details = $reader["Details"]
        Write-Host "  '$name' appears $count times" -ForegroundColor Yellow
        Write-Host "    Details: $details" -ForegroundColor DarkGray
    }
    $reader.Close()
    
    if (-not $hasDuplicates) {
        Write-Host "  No duplicate categories found!" -ForegroundColor Green
    } else {
        Write-Host "`n[ACTION REQUIRED]" -ForegroundColor Red
        Write-Host "Run fix_duplicate_categories.sql to fix these issues" -ForegroundColor Yellow
    }
    
    # Check Funds
    Write-Host "`nChecking for duplicate funds..." -ForegroundColor Yellow
    $query = @"
    SELECT 
        LOWER(Name) as LowerName,
        COUNT(*) as Count
    FROM Funds
    GROUP BY LOWER(Name)
    HAVING COUNT(*) > 1
"@
    
    $command.CommandText = $query
    $reader = $command.ExecuteReader()
    
    $hasDuplicates = $false
    while ($reader.Read()) {
        $hasDuplicates = $true
        $name = $reader["LowerName"]
        $count = $reader["Count"]
        Write-Host "  '$name' appears $count times" -ForegroundColor Yellow
    }
    $reader.Close()
    
    if (-not $hasDuplicates) {
        Write-Host "  No duplicate funds found!" -ForegroundColor Green
    }
    
    # Check Donors
    Write-Host "`nChecking for duplicate donors..." -ForegroundColor Yellow
    $query = @"
    SELECT 
        LOWER(Name) as LowerName,
        COUNT(*) as Count
    FROM Donors
    GROUP BY LOWER(Name)
    HAVING COUNT(*) > 1
"@
    
    $command.CommandText = $query
    $reader = $command.ExecuteReader()
    
    $hasDuplicates = $false
    while ($reader.Read()) {
        $hasDuplicates = $true
        $name = $reader["LowerName"]
        $count = $reader["Count"]
        Write-Host "  '$name' appears $count times" -ForegroundColor Yellow
    }
    $reader.Close()
    
    if (-not $hasDuplicates) {
        Write-Host "  No duplicate donors found!" -ForegroundColor Green
    }
    
    $connection.Close()
    
} catch {
    Write-Host "`nERROR: Could not query database" -ForegroundColor Red
    Write-Host "Alternative: Use DB Browser for SQLite" -ForegroundColor Yellow
    Write-Host "  1. Open NonProfitFinance.db in DB Browser" -ForegroundColor Gray
    Write-Host "  2. Execute SQL tab" -ForegroundColor Gray
    Write-Host "  3. Run queries from fix_duplicate_categories.sql" -ForegroundColor Gray
}

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "Check Complete" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan
