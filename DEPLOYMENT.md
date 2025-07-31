# OlloLifestyle API - Production Deployment Guide

## Overview
This guide covers deploying the OlloLifestyle API to Windows Server 2025 using Docker with Nginx reverse proxy, SSL certificates, and production-ready configurations.

## Prerequisites
- Windows Server 2025
- Docker Desktop/Community Edition installed
- SQL Server running locally (not containerized)
- Domain `api.ollolifestyle.com` pointing to server IP
- Port 80 and 443 open on firewall

## Quick Deployment

### 1. Transfer Files to Server
```powershell
# Copy entire project directory to server
# Recommended location: C:\OlloLifestyleAPI
```

### 2. Initial Setup
```powershell
# Navigate to project directory
cd C:\OlloLifestyleAPI

# Copy environment template
copy .env.example .env

# Edit .env with your actual values
notepad .env
```

### 3. Configure Database Connection
Update `.env` file:
```env
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!
SQL_SERVER=host.docker.internal
SQL_DATABASE=OlloLifestyleAPI_Master
SQL_USER=your_sql_username
SQL_PASSWORD=your_sql_password
SSL_EMAIL=admin@ollolifestyle.com
```

### 4. Initial Deployment with SSL
```powershell
# Run initial setup (includes SSL certificate generation)
.\scripts\deploy.ps1 -InitialSetup

# OR manual deployment
docker-compose up -d
```

## Architecture Overview

### Container Stack
- **olloapi**: .NET 9.0 Web API container
- **nginx**: Reverse proxy with SSL termination
- **certbot**: Automatic SSL certificate management

### Data Flow
```
Internet → Nginx (SSL) → Docker Network → API Container → Host SQL Server
```

### Networking
- Nginx: Ports 80, 443 (external)
- API: Port 8080 (internal only)
- Database: Host SQL Server via `host.docker.internal`

## Configuration Details

### Environment Variables (.env)
```env
JWT_SECRET_KEY=<32+ character secret>
SQL_SERVER=host.docker.internal
SQL_DATABASE=OlloLifestyleAPI_Master
SQL_USER=<your_sql_user>
SQL_PASSWORD=<your_sql_password>
ASPNETCORE_ENVIRONMENT=Production
API_DOMAIN=api.ollolifestyle.com
SSL_EMAIL=<your_email>
```

### SSL Certificate Management
- Automatic certificate generation via Let's Encrypt
- Auto-renewal every 12 hours
- Certificates stored in `./certbot/conf`

### Security Features
- HTTPS redirect (HTTP → HTTPS)
- Security headers (HSTS, XSS Protection, etc.)
- Rate limiting by endpoint type
- Non-root container user
- Secure SSL configuration (TLS 1.2+)

## Maintenance Operations

### Log Management
```powershell
# View live logs
docker-compose logs -f

# API logs only
docker-compose logs -f olloapi

# Nginx logs
docker-compose logs -f nginx
```

### Backup
```powershell
# Create backup
.\scripts\backup.ps1

# Custom backup location
.\scripts\backup.ps1 -BackupPath "D:\Backups" -RetentionDays 14
```

### SSL Certificate Renewal
```powershell
# Manual renewal
.\scripts\deploy.ps1 -RenewSSL

# OR using docker-compose
docker-compose run --rm certbot renew
docker-compose exec nginx nginx -s reload
```

### Updates/Redeployment
```powershell
# Pull latest code and rebuild
git pull origin main
.\scripts\deploy.ps1

# OR manual steps
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Monitoring & Health Checks

### Health Endpoints
- `https://api.ollolifestyle.com/health` - API health
- `https://api.ollolifestyle.com/swagger` - API documentation (if enabled)

### Log Locations
- API logs: `./logs/olloapi-YYYY-MM-DD.log`
- Nginx access: `./nginx/logs/access.log`
- Nginx error: `./nginx/logs/error.log`

### Container Status
```powershell
# Check running containers
docker-compose ps

# Resource usage
docker stats

# Container logs
docker-compose logs --tail=100 olloapi
```

## Troubleshooting

### Common Issues

#### 1. SSL Certificate Issues
```powershell
# Check certificate status
docker-compose run --rm certbot certificates

# Regenerate certificate
docker-compose run --rm certbot certonly --webroot --webroot-path /var/www/certbot --email your@email.com --agree-tos --no-eff-email -d api.ollolifestyle.com --force-renewal
```

#### 2. Database Connection Issues
- Verify SQL Server is running
- Check SQL Server allows connections
- Verify credentials in `.env`
- Test connection from container: `docker-compose exec olloapi ping host.docker.internal`

#### 3. Port Conflicts
```powershell
# Check port usage
netstat -an | findstr ":80\|:443\|:8080"

# Stop conflicting services
net stop http  # IIS
```

#### 4. Container Won't Start
```powershell
# Check detailed logs
docker-compose logs olloapi

# Rebuild without cache
docker-compose build --no-cache olloapi
docker-compose up -d
```

### Performance Tuning

#### 1. Resource Limits
Add to `docker-compose.yml`:
```yaml
services:
  olloapi:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: "1.0"
```

#### 2. Nginx Optimization
- Increase worker processes for high traffic
- Adjust rate limiting as needed
- Enable caching for static content

## Security Best Practices

### 1. Regular Updates
- Keep Docker images updated
- Update SSL certificates
- Monitor security advisories

### 2. Access Control
- Use strong JWT secrets
- Implement proper API authentication
- Monitor access logs

### 3. Network Security
- Use Windows Firewall
- Consider VPN for admin access
- Monitor failed authentication attempts

## Scaling Considerations

### Horizontal Scaling
- Use Docker Swarm or Kubernetes
- Implement load balancer
- Separate database server

### Database Scaling
- Consider SQL Server clustering
- Implement read replicas
- Database connection pooling

## Support

### Log Analysis
```powershell
# Search for errors
Select-String -Path ".\logs\*.log" -Pattern "ERROR|FATAL"

# API response times
Select-String -Path ".\nginx\logs\access.log" -Pattern "rt=" | Select-Object -Last 100
```

### Performance Monitoring
- Monitor container resource usage
- Watch database connection counts
- Track API response times
- Monitor SSL certificate expiry

### Backup Strategy
- Daily automated backups
- Test restore procedures
- Store backups off-server
- Include database backups in routine