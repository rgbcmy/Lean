# Docker Deployment Guide for Lean WebUI

This guide covers deploying Lean WebUI using Docker and Docker Compose for production environments.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Production Deployment](#production-deployment)
- [Configuration](#configuration)
- [Monitoring and Logging](#monitoring-and-logging)
- [Backup and Recovery](#backup-and-recovery)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Install Docker and Docker Compose

**Ubuntu/Debian:**
```bash
# Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo apt install docker-compose-plugin

# Verify installation
docker --version
docker compose version
```

**RHEL/CentOS:**
```bash
sudo yum install -y docker docker-compose-plugin
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker $USER
```

**Windows/macOS:**
Install [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Quick Start (Development)

```bash
cd WebUI

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

Access:
- API: http://localhost:5000
- Swagger: http://localhost:5000/scalar/v1

---

## Production Deployment

### 1. Prepare Environment

Create `.env.production`:

```bash
# Database
POSTGRES_PASSWORD=your-strong-password-here

# JWT Authentication
JWT_SECRET=$(openssl rand -base64 64)

# HTTPS Certificate
CERT_PASSWORD=your-cert-password

# IBKR Configuration
IBKR_TWS_HOST=127.0.0.1
IBKR_TWS_PORT=7496
IBKR_CLIENT_ID=1

# Monitoring (Grafana)
GRAFANA_USER=admin
GRAFANA_PASSWORD=your-grafana-password

# Backup
BACKUP_SCHEDULE=0 2 * * *
BACKUP_RETENTION_DAYS=30
```

**⚠️ Important**: Never commit `.env.production` to version control!

### 2. Generate HTTPS Certificate

```bash
cd scripts
./generate-cert.sh yourdomain.com ../certs
```

Or use Let's Encrypt (see HTTPS_SETUP.md).

### 3. Build Frontend

```bash
cd WebUI.Frontend
npm install
npm run build:prod
```

### 4. Deploy with Docker Compose

```bash
cd WebUI

# Load environment variables
export $(cat .env.production | xargs)

# Start production stack
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Check service status
docker-compose -f docker-compose.prod.yml ps
```

### 5. Verify Deployment

```bash
# Check health
curl http://localhost/health

# Check API
curl http://localhost/api/v1/health

# Check metrics
curl http://localhost:9090  # Prometheus
curl http://localhost:3000  # Grafana
curl http://localhost:5341  # Seq
```

---

## Configuration

### Services Overview

| Service | Port | Description |
|---------|------|-------------|
| nginx | 80, 443 | Reverse proxy & static files |
| webui-api | 5000, 5001 | Backend API |
| postgres | 5432 | PostgreSQL database |
| seq | 5341 | Centralized logging |
| prometheus | 9090 | Metrics collection |
| grafana | 3000 | Metrics visualization |
| db-backup | - | Automated database backups |

### Resource Limits

Edit `docker-compose.prod.yml` to adjust resource limits:

```yaml
deploy:
  resources:
    limits:
      cpus: '4'
      memory: 4G
    reservations:
      cpus: '2'
      memory: 2G
```

### Scaling

Scale specific services:

```bash
# Scale API to 3 instances
docker-compose -f docker-compose.prod.yml up -d --scale webui-api=3
```

---

## Monitoring and Logging

### Access Monitoring Tools

**Grafana** (http://localhost:3000):
- Default user: admin
- Password: from `GRAFANA_PASSWORD`
- Pre-configured dashboards for WebUI metrics

**Prometheus** (http://localhost:9090):
- Query metrics directly
- View targets and health

**Seq** (http://localhost:5341):
- Structured log search
- Real-time log tailing
- Alerts and dashboards

### View Logs

```bash
# All services
docker-compose -f docker-compose.prod.yml logs -f

# Specific service
docker-compose -f docker-compose.prod.yml logs -f webui-api

# Last 100 lines
docker-compose -f docker-compose.prod.yml logs --tail=100 webui-api
```

### Metrics Endpoints

- WebUI API: http://localhost:5000/metrics
- Node metrics: http://localhost:9100/metrics (if node-exporter enabled)
- PostgreSQL: http://localhost:9187/metrics (if postgres-exporter enabled)

---

## Backup and Recovery

### Automated Backups

Backups run daily at 2 AM (configurable via `BACKUP_SCHEDULE`).

Backups are stored in `./backups/` directory.

### Manual Backup

```bash
# Backup database
docker exec lean-webui-db pg_dump \
  -U leanuser -d leanwebui \
  | gzip > backups/manual_$(date +%Y%m%d_%H%M%S).sql.gz

# Backup volumes
docker run --rm \
  -v lean-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/lean-data_$(date +%Y%m%d_%H%M%S).tar.gz -C /data .
```

### Restore from Backup

```bash
# Stop services
docker-compose -f docker-compose.prod.yml down

# Restore database
gunzip < backups/leanwebui_20240219_020000.sql.gz | \
  docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# Restore volumes
docker run --rm \
  -v lean-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar xzf /backup/lean-data_20240219_020000.tar.gz -C /data

# Start services
docker-compose -f docker-compose.prod.yml up -d
```

---

## Troubleshooting

### Check Service Health

```bash
# Check all containers
docker ps

# Check specific service logs
docker logs lean-webui-api

# Check service health
docker inspect --format='{{json .State.Health}}' lean-webui-api | jq
```

### Common Issues

#### Database Connection Errors

```bash
# Check if PostgreSQL is healthy
docker exec lean-webui-db pg_isready -U leanuser

# Check connection from API container
docker exec lean-webui-api nc -zv postgres 5432
```

#### Certificate Errors

```bash
# Verify certificate exists
ls -lh certs/leanwebui.pfx

# Check certificate password
docker exec lean-webui-api ls -la /app/certs
```

#### Out of Memory

```bash
# Check memory usage
docker stats

# Increase Docker memory limit (Docker Desktop)
# Settings → Resources → Memory → Increase limit
```

#### Port Conflicts

```bash
# Check which process is using port
sudo netstat -tlnp | grep :80

# Change port in docker-compose.prod.yml
ports:
  - "8080:80"  # Use 8080 instead of 80
```

### Restart Services

```bash
# Restart specific service
docker-compose -f docker-compose.prod.yml restart webui-api

# Restart all services
docker-compose -f docker-compose.prod.yml restart

# Full rebuild
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml up -d
```

### Clean Up

```bash
# Remove stopped containers
docker-compose -f docker-compose.prod.yml down

# Remove volumes (⚠️ deletes all data)
docker-compose -f docker-compose.prod.yml down -v

# Remove images
docker-compose -f docker-compose.prod.yml down --rmi all

# Full cleanup (⚠️ removes everything)
docker system prune -a --volumes
```

---

## Production Checklist

### Before Deployment

- [ ] Environment variables configured in `.env.production`
- [ ] HTTPS certificate generated
- [ ] Frontend built (`npm run build:prod`)
- [ ] Database password is strong
- [ ] JWT secret is strong (64+ characters)
- [ ] Grafana password configured
- [ ] Backup schedule configured

### After Deployment

- [ ] All services healthy (`docker ps`)
- [ ] API health check passes (`curl /health`)
- [ ] HTTPS works (visit https://yourdomain.com)
- [ ] Can log in to WebUI
- [ ] Grafana accessible and showing metrics
- [ ] Seq receiving logs
- [ ] Database backup running
- [ ] Monitoring alerts configured

### Ongoing Maintenance

- [ ] Monitor logs daily
- [ ] Check Grafana dashboards weekly
- [ ] Verify backups weekly
- [ ] Update Docker images monthly
- [ ] Review security vulnerabilities monthly
- [ ] Test disaster recovery quarterly

---

## Performance Tuning

### PostgreSQL Optimization

Edit `docker-compose.prod.yml`:

```yaml
postgres:
  command:
    - "postgres"
    - "-c"
    - "max_connections=200"
    - "-c"
    - "shared_buffers=1GB"
    - "-c"
    - "effective_cache_size=3GB"
    - "-c"
    - "work_mem=16MB"
```

### Nginx Caching

Add caching to `configs/nginx.conf`:

```nginx
proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=api_cache:10m max_size=1g inactive=60m;

location /api/v1/market/ {
    proxy_cache api_cache;
    proxy_cache_valid 200 10s;
    proxy_cache_key "$scheme$request_method$host$request_uri";
}
```

---

## References

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [Nginx Docker Image](https://hub.docker.com/_/nginx)
- [Prometheus Docker Image](https://hub.docker.com/r/prom/prometheus)
- [Grafana Docker Image](https://hub.docker.com/r/grafana/grafana)

For more help, see [Operations Manual](./OPERATIONS_MANUAL.md) or open an issue.
