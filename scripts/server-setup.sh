#!/bin/bash

# OlloLifestyle API Server Setup Script
# Run this script on Ubuntu 20.04 LTS server to set up the production environment

set -e

# Configuration
SERVER_NAME="OLLO-PROD-SVR"
APP_USER="olloadmin"
APP_DIR="/opt/ollo-api"
SSL_DIR="$APP_DIR/ssl"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   log_error "This script should not be run as root. Please run as $APP_USER user."
   exit 1
fi

log_info "Starting OlloLifestyle API Server Setup..."

# Update system packages
log_info "Updating system packages..."
sudo apt update && sudo apt upgrade -y

# Install required packages
log_info "Installing required packages..."
sudo apt install -y \
    curl \
    wget \
    git \
    unzip \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    openssl \
    nginx \
    htop \
    ufw

# Configure firewall
log_info "Configuring firewall..."
sudo ufw --force reset
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 8081/tcp  # Internal HTTP for testing
sudo ufw --force enable

# Install Docker if not already installed
if ! command -v docker &> /dev/null; then
    log_info "Installing Docker..."
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt update
    sudo apt install -y docker-ce docker-ce-cli containerd.io
    sudo usermod -aG docker $APP_USER
    log_info "Docker installed. Please log out and back in for group changes to take effect."
else
    log_info "Docker is already installed."
fi

# Install Docker Compose if not already installed
if ! command -v docker-compose &> /dev/null; then
    log_info "Installing Docker Compose..."
    sudo curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
else
    log_info "Docker Compose is already installed."
fi

# Create application directory structure
log_info "Creating application directory structure..."
sudo mkdir -p $APP_DIR/{nginx/{conf.d,logs},logs,ssl/{certs,private},backups}
sudo chown -R $APP_USER:$APP_USER $APP_DIR
chmod 755 $APP_DIR
chmod 700 $SSL_DIR/private
chmod 755 $SSL_DIR/certs

# Generate SSL certificates (self-signed for internal use)
if [ ! -f "$SSL_DIR/certs/ollo-api.crt" ]; then
    log_info "Generating self-signed SSL certificates..."
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout $SSL_DIR/private/ollo-api.key \
        -out $SSL_DIR/certs/ollo-api.crt \
        -subj "/C=US/ST=Production/L=Server/O=OlloLifestyle/CN=192.168.50.98" \
        -addext "subjectAltName=DNS:OLLO-PROD-SVR,DNS:localhost,IP:192.168.50.98,IP:127.0.0.1"
    
    chmod 644 $SSL_DIR/certs/ollo-api.crt
    chmod 600 $SSL_DIR/private/ollo-api.key
    
    # Copy certificates for system-wide use
    sudo cp $SSL_DIR/certs/ollo-api.crt /etc/ssl/certs/
    sudo cp $SSL_DIR/private/ollo-api.key /etc/ssl/private/
    sudo chmod 644 /etc/ssl/certs/ollo-api.crt
    sudo chmod 600 /etc/ssl/private/ollo-api.key
    
    log_info "SSL certificates generated and installed."
else
    log_info "SSL certificates already exist."
fi

# Configure system-wide Nginx (backup configuration)
log_info "Configuring system Nginx as backup..."
sudo cp /etc/nginx/nginx.conf /etc/nginx/nginx.conf.backup
sudo tee /etc/nginx/sites-available/ollo-api-backup > /dev/null <<EOF
server {
    listen 8080;
    server_name localhost 127.0.0.1 192.168.50.98;
    
    location / {
        proxy_pass http://127.0.0.1:8081;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
    
    location /health {
        proxy_pass http://127.0.0.1:8081/health;
        access_log off;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/ollo-api-backup /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl enable nginx && sudo systemctl restart nginx

# Create environment template
log_info "Creating environment template..."
cat > $APP_DIR/.env.template <<EOF
# OlloLifestyle API Environment Configuration
JWT_SECRET_KEY=your-super-secure-jwt-secret-key-here
DB_PASSWORD=your-database-password-here
ASPNETCORE_ENVIRONMENT=Production
IMAGE_TAG=latest

# Database Configuration (SQL Server on host)
DB_SERVER=host.docker.internal,1433
DB_NAME=OlloLifestyleAPI_Master
DB_USER=sa

# Application Configuration
API_PORT=8080
NGINX_HTTP_PORT=80
NGINX_HTTPS_PORT=443
NGINX_INTERNAL_PORT=8081
EOF

# Create backup script
log_info "Creating backup script..."
cat > $APP_DIR/backup.sh <<'EOF'
#!/bin/bash

# OlloLifestyle API Backup Script
BACKUP_DIR="/opt/ollo-api/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="ollo-api-backup-$DATE.tar.gz"

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup application files and logs
tar -czf "$BACKUP_DIR/$BACKUP_FILE" \
    --exclude="$BACKUP_DIR" \
    --exclude="*.log" \
    --exclude="node_modules" \
    --exclude="bin" \
    --exclude="obj" \
    /opt/ollo-api/

# Keep only last 7 backups
cd $BACKUP_DIR
ls -t ollo-api-backup-*.tar.gz | tail -n +8 | xargs -r rm

echo "Backup created: $BACKUP_FILE"
echo "Backups available:"
ls -lah ollo-api-backup-*.tar.gz 2>/dev/null || echo "No backups found"
EOF

chmod +x $APP_DIR/backup.sh

# Create deployment health check script
log_info "Creating health check script..."
cat > $APP_DIR/health-check.sh <<'EOF'
#!/bin/bash

# Health check script for OlloLifestyle API
HEALTH_ENDPOINT="https://localhost/health"
MAX_ATTEMPTS=30
ATTEMPT=1

echo "Starting health check for OlloLifestyle API..."

while [ $ATTEMPT -le $MAX_ATTEMPTS ]; do
    echo "Health check attempt $ATTEMPT/$MAX_ATTEMPTS..."
    
    # Check HTTP health endpoint
    if curl -f -k -s "$HEALTH_ENDPOINT" > /dev/null 2>&1; then
        echo "✅ API is healthy!"
        echo "🔗 API accessible at: https://192.168.50.98"
        echo "🔗 Internal endpoint: http://192.168.50.98:8081"
        exit 0
    fi
    
    echo "❌ Health check failed, waiting 10 seconds..."
    sleep 10
    ATTEMPT=$((ATTEMPT + 1))
done

echo "❌ Health check failed after $MAX_ATTEMPTS attempts"
echo "📋 Checking container status..."
docker-compose ps

echo "📋 Checking logs..."
docker-compose logs --tail=20 olloapi

exit 1
EOF

chmod +x $APP_DIR/health-check.sh

# Create log rotation configuration
log_info "Setting up log rotation..."
sudo tee /etc/logrotate.d/ollo-api > /dev/null <<EOF
$APP_DIR/logs/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 $APP_USER $APP_USER
    postrotate
        docker-compose -f $APP_DIR/docker-compose.yml restart olloapi >/dev/null 2>&1 || true
    endscript
}

$APP_DIR/nginx/logs/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 $APP_USER $APP_USER
    postrotate
        docker-compose -f $APP_DIR/docker-compose.yml restart nginx >/dev/null 2>&1 || true
    endscript
}
EOF

# Create systemd service for auto-start
log_info "Creating systemd service..."
sudo tee /etc/systemd/system/ollo-api.service > /dev/null <<EOF
[Unit]
Description=OlloLifestyle API
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=$APP_DIR
ExecStart=/usr/local/bin/docker-compose up -d
ExecStop=/usr/local/bin/docker-compose down
TimeoutStartSec=0
User=$APP_USER
Group=$APP_USER

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable ollo-api.service

# Set up cron job for automated backups
log_info "Setting up automated backups..."
(crontab -l 2>/dev/null; echo "0 2 * * * $APP_DIR/backup.sh >> $APP_DIR/logs/backup.log 2>&1") | crontab -

# Create monitoring script
cat > $APP_DIR/monitor.sh <<'EOF'
#!/bin/bash

# Simple monitoring script for OlloLifestyle API
echo "=== OlloLifestyle API System Status ==="
echo "Date: $(date)"
echo ""

echo "🐳 Docker Status:"
docker version --format 'Docker Version: {{.Server.Version}}'
echo ""

echo "📦 Container Status:"
cd /opt/ollo-api
docker-compose ps
echo ""

echo "🔍 Health Check:"
if curl -f -k -s https://localhost/health > /dev/null 2>&1; then
    echo "✅ API is healthy"
else
    echo "❌ API health check failed"
fi
echo ""

echo "💾 Disk Usage:"
df -h /opt/ollo-api
echo ""

echo "🧠 Memory Usage:"
free -h
echo ""

echo "📊 System Load:"
uptime
echo ""

echo "🔗 Network Connections:"
ss -tulpn | grep -E ":(80|443|8080|8081)\b"
echo ""
EOF

chmod +x $APP_DIR/monitor.sh

# Final instructions
log_info "Server setup completed successfully! 🎉"
echo ""
log_info "Next steps:"
echo "1. Copy your .env.template to .env and configure with actual values:"
echo "   cp $APP_DIR/.env.template $APP_DIR/.env"
echo "   nano $APP_DIR/.env"
echo ""
echo "2. Ensure your GitHub repository has these secrets configured:"
echo "   - SSH_PRIVATE_KEY (for deployment access)"
echo "   - JWT_SECRET_KEY (your JWT secret)"
echo "   - DB_PASSWORD (your SQL Server password)"
echo "   - GITHUB_TOKEN (for Docker registry access)"
echo ""
echo "3. Test the setup:"
echo "   $APP_DIR/monitor.sh"
echo ""
echo "4. Manual deployment test:"
echo "   cd $APP_DIR"
echo "   docker-compose up -d"
echo "   $APP_DIR/health-check.sh"
echo ""
echo "📂 Application directory: $APP_DIR"
echo "🔒 SSL certificates: $SSL_DIR"
echo "📄 Logs location: $APP_DIR/logs"
echo "📄 Nginx logs: $APP_DIR/nginx/logs"
echo "💾 Backups location: $APP_DIR/backups"
echo ""
log_warn "Please reboot the server to ensure all changes take effect:"
echo "sudo reboot"