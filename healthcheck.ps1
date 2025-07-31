# Health Check Script for OlloLifestyle API
# Run this script to verify all components are working correctly

param(
    [Parameter(Mandatory=$false)]
    [string]$Domain = "api.ollolifestyle.com",
    
    [Parameter(Mandatory=$false)]
    [switch]$Detailed = $false
)

Write-Host "=== OlloLifestyle API Health Check ===" -ForegroundColor Green
Write-Host "Domain: $Domain" -ForegroundColor Yellow
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Yellow

$results = @()

# 1. Check Docker containers
Write-Host "`nChecking Docker containers..." -ForegroundColor Cyan
try {
    $containers = docker-compose ps --format json | ConvertFrom-Json
    foreach ($container in $containers) {
        $status = if ($container.State -eq "running") { "✓" } else { "✗" }
        Write-Host "$status $($container.Name): $($container.State)" -ForegroundColor $(if($container.State -eq "running") {"Green"} else {"Red"})
        $results += @{Component="Container-$($container.Name)"; Status=$container.State; Details="Docker container"}
    }
} catch {
    Write-Host "✗ Docker containers: Error - $_" -ForegroundColor Red
    $results += @{Component="Docker"; Status="Error"; Details=$_.Exception.Message}
}

# 2. Check API health endpoint (local)
Write-Host "`nChecking local API health..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "http://localhost:8080/health" -TimeoutSec 10
    Write-Host "✓ Local API health: OK" -ForegroundColor Green
    $results += @{Component="API-Local"; Status="OK"; Details="Health endpoint responsive"}
} catch {
    Write-Host "✗ Local API health: Failed - $_" -ForegroundColor Red
    $results += @{Component="API-Local"; Status="Failed"; Details=$_.Exception.Message}
}

# 3. Check SSL certificate
Write-Host "`nChecking SSL certificate..." -ForegroundColor Cyan
try {
    $request = [System.Net.WebRequest]::Create("https://$Domain/health")
    $request.Timeout = 10000
    $response = $request.GetResponse()
    $cert = $request.ServicePoint.Certificate
    $cert2 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($cert)
    $daysUntilExpiry = ($cert2.NotAfter - (Get-Date)).Days
    
    if ($daysUntilExpiry -gt 30) {
        Write-Host "✓ SSL Certificate: Valid (expires in $daysUntilExpiry days)" -ForegroundColor Green
        $results += @{Component="SSL"; Status="Valid"; Details="Expires in $daysUntilExpiry days"}
    } elseif ($daysUntilExpiry -gt 0) {
        Write-Host "⚠ SSL Certificate: Expires soon ($daysUntilExpiry days)" -ForegroundColor Yellow
        $results += @{Component="SSL"; Status="Warning"; Details="Expires in $daysUntilExpiry days"}
    } else {
        Write-Host "✗ SSL Certificate: Expired" -ForegroundColor Red
        $results += @{Component="SSL"; Status="Expired"; Details="Certificate has expired"}
    }
    $response.Close()
} catch {
    Write-Host "✗ SSL Certificate: Error - $_" -ForegroundColor Red
    $results += @{Component="SSL"; Status="Error"; Details=$_.Exception.Message}
}

# 4. Check external API access
Write-Host "`nChecking external API access..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "https://$Domain/health" -TimeoutSec 15
    Write-Host "✓ External API access: OK" -ForegroundColor Green
    $results += @{Component="API-External"; Status="OK"; Details="HTTPS endpoint accessible"}
} catch {
    Write-Host "✗ External API access: Failed - $_" -ForegroundColor Red
    $results += @{Component="API-External"; Status="Failed"; Details=$_.Exception.Message}
}

# 5. Check database connectivity (if possible)
Write-Host "`nChecking database connectivity..." -ForegroundColor Cyan
try {
    # This assumes we can check logs for database connection status
    $recentLogs = docker-compose logs --tail=50 olloapi 2>$null | Select-String -Pattern "database|sql|connection" -CaseSensitive:$false | Select-Object -Last 5
    if ($recentLogs) {
        $hasErrors = $recentLogs | Select-String -Pattern "error|fail|exception" -CaseSensitive:$false
        if ($hasErrors) {
            Write-Host "⚠ Database: Warning - Check logs for connection issues" -ForegroundColor Yellow
            $results += @{Component="Database"; Status="Warning"; Details="Potential connection issues in logs"}
        } else {
            Write-Host "✓ Database: Likely OK (no recent errors in logs)" -ForegroundColor Green
            $results += @{Component="Database"; Status="OK"; Details="No recent errors in logs"}
        }
    } else {
        Write-Host "? Database: Status unknown (no recent log entries)" -ForegroundColor Yellow
        $results += @{Component="Database"; Status="Unknown"; Details="No recent log entries"}
    }
} catch {
    Write-Host "? Database: Cannot determine status" -ForegroundColor Yellow
    $results += @{Component="Database"; Status="Unknown"; Details="Unable to check logs"}
}

# 6. Check disk space
Write-Host "`nChecking disk space..." -ForegroundColor Cyan
try {
    $disks = Get-WmiObject -Class Win32_LogicalDisk | Where-Object {$_.DriveType -eq 3}
    foreach ($disk in $disks) {
        $freePercent = [math]::Round(($disk.FreeSpace / $disk.Size) * 100, 2)
        if ($freePercent -gt 20) {
            Write-Host "✓ Disk $($disk.DeviceID): $freePercent% free" -ForegroundColor Green
        } elseif ($freePercent -gt 10) {
            Write-Host "⚠ Disk $($disk.DeviceID): $freePercent% free (low space)" -ForegroundColor Yellow
        } else {
            Write-Host "✗ Disk $($disk.DeviceID): $freePercent% free (critical)" -ForegroundColor Red
        }
        $results += @{Component="Disk-$($disk.DeviceID)"; Status=if($freePercent -gt 20){"OK"}elseif($freePercent -gt 10){"Warning"}else{"Critical"}; Details="$freePercent% free"}
    }
} catch {
    Write-Host "✗ Disk space: Error checking - $_" -ForegroundColor Red
    $results += @{Component="Disk"; Status="Error"; Details=$_.Exception.Message}
}

# 7. Check log file sizes
Write-Host "`nChecking log files..." -ForegroundColor Cyan
try {
    $logDirs = @("logs", "nginx/logs")
    foreach ($logDir in $logDirs) {
        if (Test-Path $logDir) {
            $logFiles = Get-ChildItem -Path $logDir -File -Recurse
            $totalSize = ($logFiles | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "✓ $logDir: $([math]::Round($totalSize, 2)) MB ($($logFiles.Count) files)" -ForegroundColor Green
            $results += @{Component="Logs-$logDir"; Status="OK"; Details="$([math]::Round($totalSize, 2)) MB, $($logFiles.Count) files"}
        }
    }
} catch {
    Write-Host "⚠ Log files: Warning - $_" -ForegroundColor Yellow
    $results += @{Component="Logs"; Status="Warning"; Details=$_.Exception.Message}
}

# Summary
Write-Host "`n=== Health Check Summary ===" -ForegroundColor Green
$okCount = ($results | Where-Object {$_.Status -eq "OK"}).Count
$warningCount = ($results | Where-Object {$_.Status -in @("Warning", "Unknown")}).Count
$errorCount = ($results | Where-Object {$_.Status -in @("Failed", "Error", "Critical", "Expired")}).Count

Write-Host "✓ OK: $okCount" -ForegroundColor Green
Write-Host "⚠ Warnings: $warningCount" -ForegroundColor Yellow
Write-Host "✗ Errors: $errorCount" -ForegroundColor Red

if ($Detailed) {
    Write-Host "`n=== Detailed Results ===" -ForegroundColor Cyan
    $results | ForEach-Object {
        Write-Host "$($_.Component): $($_.Status) - $($_.Details)" -ForegroundColor White
    }
}

# Overall status
if ($errorCount -eq 0 -and $warningCount -eq 0) {
    Write-Host "`n🎉 All systems operational!" -ForegroundColor Green
    exit 0
} elseif ($errorCount -eq 0) {
    Write-Host "`n⚠ System operational with warnings" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "`n❌ System has critical issues" -ForegroundColor Red
    exit 2
}