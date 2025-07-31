# Production Security Setup Script
# Configures additional security measures for production deployment

param(
    [Parameter(Mandatory=$false)]
    [switch]$ConfigureFirewall = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SetupMonitoring = $false
)

Write-Host "=== Production Security Setup ===" -ForegroundColor Green

# Configure Windows Firewall rules
if ($ConfigureFirewall) {
    Write-Host "Configuring Windows Firewall..." -ForegroundColor Yellow
    
    # Allow HTTP and HTTPS
    New-NetFirewallRule -DisplayName "Allow HTTP (80)" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow -ErrorAction SilentlyContinue
    New-NetFirewallRule -DisplayName "Allow HTTPS (443)" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow -ErrorAction SilentlyContinue
    
    # Block direct access to API port from external networks
    New-NetFirewallRule -DisplayName "Block External API Access" -Direction Inbound -Protocol TCP -LocalPort 8080 -RemoteAddress Internet -Action Block -ErrorAction SilentlyContinue
    
    Write-Host "Firewall rules configured." -ForegroundColor Green
}

# Setup basic monitoring
if ($SetupMonitoring) {
    Write-Host "Setting up monitoring scripts..." -ForegroundColor Yellow
    
    # Create monitoring script
    $monitorScript = @'
# System Monitoring Script
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

# Check Docker containers
$containers = docker ps --format "table {{.Names}}\t{{.Status}}" | Out-String

# Check disk space
$diskSpace = Get-WmiObject -Class Win32_LogicalDisk | Where-Object {$_.DriveType -eq 3} | 
    Select-Object DeviceID, @{Name="Size(GB)";Expression={[math]::Round($_.Size/1GB,2)}}, 
    @{Name="Free(GB)";Expression={[math]::Round($_.FreeSpace/1GB,2)}}, 
    @{Name="Free(%)";Expression={[math]::Round(($_.FreeSpace/$_.Size)*100,2)}}

# Check SSL certificate expiry
$certInfo = ""
if (Test-Path "certbot/conf/live/api.ollolifestyle.com/fullchain.pem") {
    try {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("certbot/conf/live/api.ollolifestyle.com/fullchain.pem")
        $daysUntilExpiry = ($cert.NotAfter - (Get-Date)).Days
        $certInfo = "SSL Certificate expires in $daysUntilExpiry days"
    } catch {
        $certInfo = "Unable to read SSL certificate"
    }
}

# Log monitoring info
$logEntry = @"
=== System Health Check - $timestamp ===
Docker Containers:
$containers

Disk Space:
$($diskSpace | Format-Table | Out-String)

$certInfo

================================
"@

Add-Content -Path "monitoring.log" -Value $logEntry
Write-Host "System health logged to monitoring.log" -ForegroundColor Green
'@

    $monitorScript | Out-File -FilePath "scripts/monitor.ps1" -Encoding utf8
    
    # Create scheduled task for monitoring (every 6 hours)
    $action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File `"$(Get-Location)\scripts\monitor.ps1`""
    $trigger = New-ScheduledTaskTrigger -Daily -At "06:00" -RepetitionInterval (New-TimeSpan -Hours 6) -RepetitionDuration (New-TimeSpan -Hours 23 -Minutes 59)
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
    
    try {
        Register-ScheduledTask -TaskName "OlloAPI-Monitoring" -Action $action -Trigger $trigger -Settings $settings -Force
        Write-Host "Monitoring scheduled task created." -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not create scheduled task. Please create manually." -ForegroundColor Yellow
    }
}

# Security recommendations
Write-Host "`n=== Security Recommendations ===" -ForegroundColor Yellow
Write-Host "1. Change default SQL Server SA password" -ForegroundColor White
Write-Host "2. Create dedicated SQL user for API with minimal permissions" -ForegroundColor White
Write-Host "3. Enable SQL Server encryption" -ForegroundColor White
Write-Host "4. Configure log shipping for database backups" -ForegroundColor White
Write-Host "5. Set up automated backup retention" -ForegroundColor White
Write-Host "6. Monitor failed login attempts" -ForegroundColor White
Write-Host "7. Consider implementing fail2ban equivalent for Windows" -ForegroundColor White
Write-Host "8. Regular security updates for Windows Server" -ForegroundColor White

# Production checklist
Write-Host "`n=== Production Deployment Checklist ===" -ForegroundColor Cyan
Write-Host "□ Environment variables configured (.env)" -ForegroundColor White
Write-Host "□ SQL Server connection tested" -ForegroundColor White
Write-Host "□ Domain DNS pointed to server" -ForegroundColor White
Write-Host "□ SSL certificate generated and valid" -ForegroundColor White
Write-Host "□ Firewall rules configured" -ForegroundColor White
Write-Host "□ Backup strategy implemented" -ForegroundColor White
Write-Host "□ Monitoring setup complete" -ForegroundColor White
Write-Host "□ Rate limiting tested" -ForegroundColor White
Write-Host "□ Health checks verified" -ForegroundColor White
Write-Host "□ Log rotation configured" -ForegroundColor White

Write-Host "`nProduction setup script completed!" -ForegroundColor Green