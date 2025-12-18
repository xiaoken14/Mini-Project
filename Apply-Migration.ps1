# PowerShell script to apply the migration
Write-Host "Applying database migration..." -ForegroundColor Cyan

$databasePath = "healthcare.db"

if (-not (Test-Path $databasePath)) {
    Write-Host "Error: Database file not found at $databasePath" -ForegroundColor Red
    exit 1
}

# Load SQLite assembly from the project
Add-Type -Path "bin\Debug\net8.0\Microsoft.Data.Sqlite.dll"
Add-Type -Path "bin\Debug\net8.0\SQLitePCLRaw.core.dll"
Add-Type -Path "bin\Debug\net8.0\SQLitePCLRaw.provider.e_sqlite3.dll"
Add-Type -Path "bin\Debug\net8.0\SQLitePCLRaw.batteries_v2.dll"

# Initialize SQLite
[SQLitePCL.Batteries_V2]::Init()

$connectionString = "Data Source=$databasePath"
$connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)

try {
    $connection.Open()
    Write-Host "✓ Connected to database" -ForegroundColor Green
    
    # Check if columns already exist
    $checkCmd = $connection.CreateCommand()
    $checkCmd.CommandText = "PRAGMA table_info(AspNetUsers)"
    $reader = $checkCmd.ExecuteReader()
    
    $hasDoctorId = $false
    $hasPatientId = $false
    
    while ($reader.Read()) {
        $columnName = $reader.GetString(1)
        if ($columnName -eq "DoctorId") { $hasDoctorId = $true }
        if ($columnName -eq "PatientId") { $hasPatientId = $true }
    }
    $reader.Close()
    
    # Add DoctorId column
    if (-not $hasDoctorId) {
        Write-Host "Adding DoctorId column..." -ForegroundColor Yellow
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN DoctorId INTEGER NULL"
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "✓ DoctorId column added" -ForegroundColor Green
    } else {
        Write-Host "✓ DoctorId column already exists" -ForegroundColor Green
    }
    
    # Add PatientId column
    if (-not $hasPatientId) {
        Write-Host "Adding PatientId column..." -ForegroundColor Yellow
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN PatientId INTEGER NULL"
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "✓ PatientId column added" -ForegroundColor Green
    } else {
        Write-Host "✓ PatientId column already exists" -ForegroundColor Green
    }
    
    # Check if migration history exists
    $checkMigrationCmd = $connection.CreateCommand()
    $checkMigrationCmd.CommandText = @"
        SELECT COUNT(*) FROM __EFMigrationsHistory 
        WHERE MigrationId = '20251218025302_AddLegacyIdLinksToApplicationUser'
"@
    $migrationExists = [int]$checkMigrationCmd.ExecuteScalar() -gt 0
    
    if (-not $migrationExists) {
        Write-Host "Adding migration history record..." -ForegroundColor Yellow
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = @"
            INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
            VALUES ('20251218025302_AddLegacyIdLinksToApplicationUser', '8.0.0')
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "✓ Migration history record added" -ForegroundColor Green
    } else {
        Write-Host "✓ Migration history record already exists" -ForegroundColor Green
    }
    
    Write-Host "`n✓ Migration applied successfully!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Link existing users to Doctor/Patient tables (see DOCTOR_SCHEDULE_FIX_SUMMARY.md)"
    Write-Host "2. Restart your application"
    Write-Host "`nYour app is running at: http://localhost:5000" -ForegroundColor Yellow
    
} catch {
    Write-Host "`n✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
