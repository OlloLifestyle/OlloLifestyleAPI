# OlloLifestyle API Deployment Script for Internal Docker Communication
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production"
)

Write-Host "=== OlloLifestyle API Internal Deployment Script ===" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Create necessary directories
$directories = @(
    "logs",
    "nginx/logs"
)

foreach ($dir in $directories) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force
        Write-Host "Created directory: $dir" -ForegroundColor Green
    }
}

# Check if .env file exists
if (!(Test-Path ".env")) {
    Write-Host "Creating .env file from .env.example..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "IMPORTANT: Please edit .env file with your JWT secret!" -ForegroundColor Red
    Write-Host "SQL Server credentials are already configured for OLLO-DB-SVR" -ForegroundColor Green
    Read-Host "Press Enter after editing .env file"
}

# Test SQL Server connectivity
Write-Host "Testing SQL Server connectivity..." -ForegroundColor Yellow
try {
    $connectionTest = Test-NetConnection -ComputerName "OLLO-DB-SVR" -Port 1433 -ErrorAction SilentlyContinue
    if ($connectionTest.TcpTestSucceeded) {
        Write-Host "✓ SQL Server OLLO-DB-SVR is reachable on port 1433" -ForegroundColor Green
    } else {
        Write-Host "⚠ Warning: Cannot reach SQL Server OLLO-DB-SVR on port 1433" -ForegroundColor Yellow
        Write-Host "Please ensure SQL Server is running and allows remote connections" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Warning: Could not test SQL Server connectivity" -ForegroundColor Yellow
}

# Build and deploy
Write-Host "Building Docker image..." -ForegroundColor Yellow
docker-compose build --no-cache

Write-Host "Starting services..." -ForegroundColor Yellow
docker-compose up -d

# Wait for services to start
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check service health
Write-Host "Checking service status..." -ForegroundColor Yellow
$apiHealth = try {
    Invoke-RestMethod -Uri "http://localhost/health" -TimeoutSec 10
    "Healthy"
} catch {
    "Unhealthy: $_"
}

Write-Host "API Health: $apiHealth" -ForegroundColor $(if($apiHealth -eq "Healthy") {"Green"} else {"Red"})

# Show running containers
Write-Host "`nRunning containers:" -ForegroundColor Yellow
docker-compose ps

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Internal API URL: http://localhost (Nginx proxy)" -ForegroundColor Cyan
Write-Host "Direct API URL: http://localhost:8080 (if exposed)" -ForegroundColor Cyan
Write-Host "API in Docker network: http://olloapi:8080" -ForegroundColor Cyan
Write-Host "Logs location: ./logs" -ForegroundColor Cyan
Write-Host "Nginx logs: ./nginx/logs" -ForegroundColor Cyan

Write-Host "`nFor your frontend application:" -ForegroundColor Yellow
Write-Host "- Use 'olloapi:8080' as API endpoint if frontend is in same Docker network" -ForegroundColor White
Write-Host "- Use 'localhost' (port 80) if accessing via Nginx proxy" -ForegroundColor White
Write-Host "- Monitor logs with: docker-compose logs -f" -ForegroundColor White