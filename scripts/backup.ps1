# Backup Script for OlloLifestyle API
param(
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = ".\backups",
    
    [Parameter(Mandatory=$false)]
    [int]$RetentionDays = 7
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = Join-Path $BackupPath $timestamp

Write-Host "Creating backup: $backupDir" -ForegroundColor Green

# Create backup directory
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

# Backup logs
Write-Host "Backing up logs..." -ForegroundColor Yellow
if (Test-Path "logs") {
    Copy-Item -Path "logs" -Destination (Join-Path $backupDir "logs") -Recurse -Force
}

# Backup nginx logs
Write-Host "Backing up nginx logs..." -ForegroundColor Yellow
if (Test-Path "nginx/logs") {
    Copy-Item -Path "nginx/logs" -Destination (Join-Path $backupDir "nginx-logs") -Recurse -Force
}

# Backup SSL certificates
Write-Host "Backing up SSL certificates..." -ForegroundColor Yellow
if (Test-Path "certbot/conf") {
    Copy-Item -Path "certbot/conf" -Destination (Join-Path $backupDir "ssl-certs") -Recurse -Force
}

# Backup configuration files
Write-Host "Backing up configuration..." -ForegroundColor Yellow
$configs = @(
    "docker-compose.yml",
    "nginx/nginx.conf",
    "nginx/conf.d",
    ".env"
)

foreach ($config in $configs) {
    if (Test-Path $config) {
        $destPath = Join-Path $backupDir (Split-Path $config -Leaf)
        Copy-Item -Path $config -Destination $destPath -Recurse -Force
    }
}

Write-Host "Backup completed: $backupDir" -ForegroundColor Green

# Cleanup old backups
Write-Host "Cleaning up old backups (older than $RetentionDays days)..." -ForegroundColor Yellow
$cutoffDate = (Get-Date).AddDays(-$RetentionDays)
Get-ChildItem -Path $BackupPath -Directory | Where-Object { $_.CreationTime -lt $cutoffDate } | Remove-Item -Recurse -Force

Write-Host "Backup process completed!" -ForegroundColor Green