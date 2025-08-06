#!/bin/bash

# Manual Deployment Script for OlloLifestyle API
# This script can be run manually for deployments or troubleshooting

set -e

# Configuration
APP_DIR="/opt/ollo-api"
CONTAINER_NAME="olloapi"
NGINX_CONTAINER="nginx-proxy"
BACKUP_DIR="$APP_DIR/backups"
LOG_FILE="$APP_DIR/logs/deploy-$(date +%Y%m%d_%H%M%S).log"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1" | tee -a "$LOG_FILE"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1" | tee -a "$LOG_FILE"
}

# Function to check if container is healthy
check_health() {
    local container_name=$1
    local max_attempts=30
    local attempt=1
    
    log_info "Checking health of container: $container_name"
    
    while [ $attempt -le $max_attempts ]; do
        if [ "$(docker inspect --format='{{.State.Health.Status}}' $container_name 2>/dev/null)" == "healthy" ]; then
            log_info "Container $container_name is healthy"
            return 0
        fi
        
        log_warn "Health check attempt $attempt/$max_attempts for $container_name"
        sleep 10
        attempt=$((attempt + 1))
    done
    
    log_error "Container $container_name failed health check after $max_attempts attempts"
    return 1
}

# Function to backup current deployment
backup_deployment() {
    log_step "Creating backup of current deployment..."
    
    mkdir -p "$BACKUP_DIR"
    
    local backup_file="$BACKUP_DIR/pre-deploy-backup-$(date +%Y%m%d_%H%M%S).tar.gz"
    
    # Create backup excluding logs and temporary files
    tar -czf "$backup_file" \
        --exclude="$BACKUP_DIR" \
        --exclude="*.log" \
        --exclude="logs/" \
        -C "$APP_DIR" .
    
    log_info "Backup created: $backup_file"
    
    # Keep only last 10 backups
    cd "$BACKUP_DIR"
    ls -t pre-deploy-backup-*.tar.gz 2>/dev/null | tail -n +11 | xargs -r rm
    
    log_info "Backup cleanup completed"
}

# Function to pull latest images
pull_images() {
    log_step "Pulling latest Docker images..."
    
    cd "$APP_DIR"
    
    # Pull images specified in docker-compose.yml
    if ! docker-compose pull; then
        log_error "Failed to pull Docker images"
        return 1
    fi
    
    log_info "Docker images pulled successfully"
}

# Function to stop containers gracefully
stop_containers() {
    log_step "Stopping containers gracefully..."
    
    cd "$APP_DIR"
    
    # Stop containers with timeout
    if ! timeout 60 docker-compose down; then
        log_warn "Graceful shutdown timed out, forcing container stop"
        docker-compose kill
        docker-compose down
    fi
    
    log_info "Containers stopped successfully"
}

# Function to start containers
start_containers() {
    log_step "Starting containers..."
    
    cd "$APP_DIR"
    
    if ! docker-compose up -d; then
        log_error "Failed to start containers"
        return 1
    fi
    
    log_info "Containers started successfully"
}

# Function to cleanup old images
cleanup_images() {
    log_step "Cleaning up old Docker images..."
    
    # Remove unused images
    docker image prune -f
    
    # Keep only the 3 most recent versions of the API image
    local api_images=$(docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.ID}}" | grep "ollo-lifestyle-api" | grep -v latest | tail -n +4 | awk '{print $3}')
    
    if [ ! -z "$api_images" ]; then
        echo "$api_images" | xargs -r docker rmi || true
        log_info "Old API images cleaned up"
    fi
}

# Function to verify deployment
verify_deployment() {
    log_step "Verifying deployment..."
    
    # Check if containers are running
    cd "$APP_DIR"
    
    if ! docker-compose ps | grep -q "Up"; then
        log_error "Some containers are not running"
        docker-compose logs --tail=50
        return 1
    fi
    
    # Check health endpoints
    local endpoints=("http://localhost:8081/health" "https://localhost/health")
    
    for endpoint in "${endpoints[@]}"; do
        log_info "Testing endpoint: $endpoint"
        
        local max_attempts=30
        local attempt=1
        
        while [ $attempt -le $max_attempts ]; do
            if curl -f -k -s "$endpoint" > /dev/null 2>&1; then
                log_info "✅ $endpoint is responding"
                break
            fi
            
            if [ $attempt -eq $max_attempts ]; then
                log_error "❌ $endpoint failed after $max_attempts attempts"
                return 1
            fi
            
            log_warn "Attempt $attempt/$max_attempts for $endpoint"
            sleep 5
            attempt=$((attempt + 1))
        done
    done
    
    log_info "All health checks passed!"
}

# Function to show deployment status
show_status() {
    log_step "Deployment Status Summary"
    
    echo "======================================" | tee -a "$LOG_FILE"
    echo "OlloLifestyle API Deployment Status" | tee -a "$LOG_FILE"
    echo "Timestamp: $(date)" | tee -a "$LOG_FILE"
    echo "======================================" | tee -a "$LOG_FILE"
    
    # Container status
    echo "" | tee -a "$LOG_FILE"
    echo "🐳 Container Status:" | tee -a "$LOG_FILE"
    docker-compose -f "$APP_DIR/docker-compose.yml" ps | tee -a "$LOG_FILE"
    
    # Image information
    echo "" | tee -a "$LOG_FILE"
    echo "🖼️ Image Information:" | tee -a "$LOG_FILE"
    docker images | grep -E "(ollo-lifestyle-api|nginx)" | tee -a "$LOG_FILE"
    
    # Resource usage
    echo "" | tee -a "$LOG_FILE"
    echo "💾 Resource Usage:" | tee -a "$LOG_FILE"
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}" | tee -a "$LOG_FILE"
    
    # Disk usage
    echo "" | tee -a "$LOG_FILE"
    echo "💿 Disk Usage:" | tee -a "$LOG_FILE"
    df -h "$APP_DIR" | tee -a "$LOG_FILE"
    
    # Recent logs (last 10 lines)
    echo "" | tee -a "$LOG_FILE"
    echo "📋 Recent Application Logs:" | tee -a "$LOG_FILE"
    docker-compose -f "$APP_DIR/docker-compose.yml" logs --tail=10 olloapi | tee -a "$LOG_FILE"
    
    echo "======================================" | tee -a "$LOG_FILE"
    echo "" | tee -a "$LOG_FILE"
}

# Function to rollback deployment
rollback_deployment() {
    log_step "Rolling back deployment..."
    
    local backup_file=$(ls -t "$BACKUP_DIR"/pre-deploy-backup-*.tar.gz 2>/dev/null | head -1)
    
    if [ -z "$backup_file" ]; then
        log_error "No backup file found for rollback"
        return 1
    fi
    
    log_info "Rolling back to: $backup_file"
    
    # Stop current containers
    stop_containers
    
    # Extract backup
    cd "$APP_DIR"
    tar -xzf "$backup_file"
    
    # Start containers
    start_containers
    
    # Verify rollback
    if verify_deployment; then
        log_info "✅ Rollback completed successfully"
    else
        log_error "❌ Rollback verification failed"
        return 1
    fi
}

# Main deployment function
deploy() {
    log_info "Starting OlloLifestyle API deployment..."
    log_info "Log file: $LOG_FILE"
    
    # Ensure we're in the right directory
    if [ ! -f "$APP_DIR/docker-compose.yml" ]; then
        log_error "docker-compose.yml not found in $APP_DIR"
        exit 1
    fi
    
    # Check if .env file exists
    if [ ! -f "$APP_DIR/.env" ]; then
        log_error ".env file not found. Please create it from .env.template"
        exit 1
    fi
    
    # Create backup
    backup_deployment
    
    # Pull latest images
    if ! pull_images; then
        log_error "Failed to pull images, aborting deployment"
        exit 1
    fi
    
    # Stop containers
    if ! stop_containers; then
        log_error "Failed to stop containers, aborting deployment"
        exit 1
    fi
    
    # Start containers
    if ! start_containers; then
        log_error "Failed to start containers, attempting rollback"
        rollback_deployment
        exit 1
    fi
    
    # Cleanup old images
    cleanup_images
    
    # Verify deployment
    if ! verify_deployment; then
        log_error "Deployment verification failed, attempting rollback"
        rollback_deployment
        exit 1
    fi
    
    # Show final status
    show_status
    
    log_info "🎉 Deployment completed successfully!"
    echo ""
    echo "🔗 API Endpoints:"
    echo "   HTTPS: https://192.168.50.98"
    echo "   HTTP (internal): http://192.168.50.98:8081"
    echo "   Health Check: https://192.168.50.98/health"
    echo ""
    echo "📄 Deployment log: $LOG_FILE"
}

# Script usage
usage() {
    echo "Usage: $0 [deploy|rollback|status|health|logs|cleanup]"
    echo ""
    echo "Commands:"
    echo "  deploy     - Deploy the latest version"
    echo "  rollback   - Rollback to previous version"
    echo "  status     - Show current deployment status"
    echo "  health     - Run health checks"
    echo "  logs       - Show recent application logs"
    echo "  cleanup    - Cleanup old images and containers"
    exit 1
}

# Handle command line arguments
case "$1" in
    "deploy")
        deploy
        ;;
    "rollback")
        rollback_deployment
        ;;
    "status")
        show_status
        ;;
    "health")
        verify_deployment
        ;;
    "logs")
        cd "$APP_DIR"
        docker-compose logs --tail=50 -f
        ;;
    "cleanup")
        cleanup_images
        ;;
    *)
        usage
        ;;
esac