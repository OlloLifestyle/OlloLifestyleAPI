# OlloLifestyle API Deployment Script for Windows Server 2025
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",
    
    [Parameter(Mandatory=$false)]
    [switch]$InitialSetup = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$RenewSSL = $false
)

Write-Host "=== OlloLifestyle API Deployment Script ===" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Create necessary directories
$directories = @(
    "logs",
    "nginx/logs", 
    "nginx/ssl",
    "certbot/conf",
    "certbot/www"
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
    Write-Host "IMPORTANT: Please edit .env file with your actual values!" -ForegroundColor Red
    Read-Host "Press Enter after editing .env file"
}

# Initial SSL certificate setup
if ($InitialSetup) {
    Write-Host "Setting up initial SSL certificate..." -ForegroundColor Yellow
    
    # Create temporary nginx config for initial certificate
    $tempConfig = @"
server {
    listen 80;
    server_name api.ollolifestyle.com;
    
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    location / {
        return 200 'Server is setting up SSL...';
        add_header Content-Type text/plain;
    }
}
"@
    $tempConfig | Out-File -FilePath "nginx/conf.d/temp.conf" -Encoding utf8
    
    # Start nginx for certificate generation
    docker-compose up -d nginx
    
    Write-Host "Generating SSL certificate..." -ForegroundColor Yellow
    $domain = "api.ollolifestyle.com"
    $email = Read-Host "Enter your email for SSL certificate"
    
    docker-compose run --rm certbot certonly --webroot --webroot-path /var/www/certbot --email $email --agree-tos --no-eff-email -d $domain
    
    # Remove temporary config
    Remove-Item "nginx/conf.d/temp.conf" -Force
    
    Write-Host "SSL certificate generated successfully!" -ForegroundColor Green
}

# Renew SSL certificate
if ($RenewSSL) {
    Write-Host "Renewing SSL certificate..." -ForegroundColor Yellow
    docker-compose run --rm certbot renew
    docker-compose exec nginx nginx -s reload
    Write-Host "SSL certificate renewed!" -ForegroundColor Green
    exit
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
    Invoke-RestMethod -Uri "http://localhost:8080/health" -TimeoutSec 10
    "Healthy"
} catch {
    "Unhealthy: $_"
}

Write-Host "API Health: $apiHealth" -ForegroundColor $(if($apiHealth -eq "Healthy") {"Green"} else {"Red"})

# Show running containers
Write-Host "`nRunning containers:" -ForegroundColor Yellow
docker-compose ps

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "API URL: https://api.ollolifestyle.com" -ForegroundColor Cyan
Write-Host "Logs location: ./logs" -ForegroundColor Cyan
Write-Host "Nginx logs: ./nginx/logs" -ForegroundColor Cyan

if ($InitialSetup) {
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "1. Point your domain api.ollolifestyle.com to this server's IP" -ForegroundColor White
    Write-Host "2. Test your API at https://api.ollolifestyle.com/health" -ForegroundColor White
    Write-Host "3. Monitor logs with: docker-compose logs -f" -ForegroundColor White
}