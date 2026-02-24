# Docker Deployment Guide
# Docker 部署教程

[English](#english) | [中文](#中文)

---

## English

### Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Docker Compose Configuration](#docker-compose-configuration)
- [Environment Variables](#environment-variables)
- [Production Deployment](#production-deployment)
- [SSL/TLS Configuration](#ssltls-configuration)
- [Monitoring and Logging](#monitoring-and-logging)
- [Backup and Maintenance](#backup-and-maintenance)
- [Troubleshooting](#troubleshooting-docker)

---

### Overview

Docker Compose provides the easiest and most reliable way to deploy Lean WebUI. This guide covers:

- **Development Setup**: Quick local testing
- **Production Setup**: Secure, scalable deployment with HTTPS, monitoring, and backups
- **Multi-Container Architecture**: API, Frontend, Database, Nginx

**Architecture**:

```
┌─────────────────────────────────────────────────────┐
│                   Docker Host                        │
│                                                       │
│  ┌──────────┐    ┌──────────┐    ┌──────────────┐  │
│  │  Nginx   │───▶│ Frontend │    │  PostgreSQL  │  │
│  │  (Proxy) │    │ (React)  │    │  (Database)  │  │
│  └────┬─────┘    └──────────┘    └──────┬───────┘  │
│       │                                  │          │
│       │          ┌──────────┐            │          │
│       └─────────▶│ Backend  │────────────┘          │
│                  │ (API)    │                       │
│                  └────┬─────┘                       │
│                       │                             │
│                       │ (IPC/Process)               │
│                       ▼                             │
│                  ┌──────────┐                       │
│                  │  Lean    │                       │
│                  │ Engine   │                       │
│                  └──────────┘                       │
└─────────────────────────────────────────────────────┘
         │
         │ (TWS API)
         ▼
    ┌─────────┐
    │  IBKR   │
    │ TWS/GW  │
    └─────────┘
```

---

### Prerequisites

#### System Requirements

- **OS**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 11+
- **CPU**: 2 cores minimum (4 cores recommended)
- **RAM**: 4GB minimum (8GB recommended)
- **Disk**: 20GB available space

#### Software Requirements

1. **Docker Engine**: 20.10+
2. **Docker Compose**: 2.0+

#### Installation

##### Windows

1. Download [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
2. Run installer
3. Enable WSL 2 backend (recommended)
4. Restart computer
5. Verify:
   ```powershell
   docker --version
   docker-compose --version
   ```

##### Linux (Ubuntu)

```bash
# Uninstall old versions
sudo apt remove docker docker-engine docker.io containerd runc

# Install dependencies
sudo apt update
sudo apt install apt-transport-https ca-certificates curl gnupg lsb-release

# Add Docker GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine
sudo apt update
sudo apt install docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Add user to docker group (avoid sudo)
sudo usermod -aG docker $USER
newgrp docker

# Verify
docker --version
docker compose version
```

##### macOS

```bash
# Install using Homebrew
brew install --cask docker

# Or download Docker Desktop from https://www.docker.com/products/docker-desktop

# Verify
docker --version
docker-compose --version
```

---

### Quick Start

#### 1. Clone Repository

```bash
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI
```

#### 2. Create Environment File

Create `.env` in the `WebUI` directory:

```env
# Database Configuration
DB_PASSWORD=changeme123!
POSTGRES_DB=leanui
POSTGRES_USER=leanuser

# JWT Secret (generate with: openssl rand -base64 32)
JWT_SECRET=your_jwt_secret_key_at_least_32_characters_long_here

# IBKR Configuration
IBKR_HOST=host.docker.internal
IBKR_PORT=7497
IBKR_CLIENT_ID=1

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:5001;http://+:5000
```

#### 3. Create Docker Compose File

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: lean-postgres
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: ./WebUI.API
      dockerfile: Dockerfile
    container_name: lean-api
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      Database__Provider: PostgreSQL
      Database__ConnectionString: Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${DB_PASSWORD}
      JWT__SecretKey: ${JWT_SECRET}
      IBKR__TWS__Host: ${IBKR_HOST}
      IBKR__TWS__Port: ${IBKR_PORT}
      IBKR__TWS__ClientId: ${IBKR_CLIENT_ID}
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}
      ASPNETCORE_URLS: ${ASPNETCORE_URLS}
    ports:
      - "5000:5000"
      - "5001:5001"
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  frontend:
    build:
      context: ./WebUI.Frontend
      dockerfile: Dockerfile
    container_name: lean-frontend
    depends_on:
      - api
    environment:
      REACT_APP_API_URL: http://api:5000/api
      REACT_APP_SIGNALR_URL: http://api:5000/hubs
    ports:
      - "3000:80"
    restart: unless-stopped

volumes:
  postgres_data:
```

#### 4. Start Services

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Check status
docker-compose ps
```

#### 5. Access WebUI

- Frontend: http://localhost:3000
- API: http://localhost:5000
- API Documentation: http://localhost:5000/swagger

Default credentials:
- Username: `admin`
- Password: `ChangeMe123!`

---

### Docker Compose Configuration

#### Service Definitions

##### PostgreSQL Service

```yaml
postgres:
  image: postgres:15-alpine
  container_name: lean-postgres
  environment:
    POSTGRES_DB: leanui
    POSTGRES_USER: leanuser
    POSTGRES_PASSWORD: ${DB_PASSWORD}
  volumes:
    - postgres_data:/var/lib/postgresql/data
    - ./init.sql:/docker-entrypoint-initdb.d/init.sql  # Optional: initial setup
  ports:
    - "5432:5432"
  restart: unless-stopped
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U leanuser"]
    interval: 10s
    timeout: 5s
    retries: 5
```

##### API Service (Backend)

```yaml
api:
  build:
    context: ./WebUI.API
    dockerfile: Dockerfile
    args:
      BUILD_CONFIGURATION: Release
  container_name: lean-api
  depends_on:
    postgres:
      condition: service_healthy
  environment:
    Database__Provider: PostgreSQL
    Database__ConnectionString: Host=postgres;Database=leanui;Username=leanuser;Password=${DB_PASSWORD}
    JWT__SecretKey: ${JWT_SECRET}
    IBKR__TWS__Host: host.docker.internal
    IBKR__TWS__Port: 7497
  ports:
    - "5000:5000"
    - "5001:5001"
  volumes:
    - ./data:/app/data
    - ./logs:/app/logs
    - ./strategies:/app/strategies  # Strategy files
  restart: unless-stopped
  networks:
    - leanui-network
```

##### Frontend Service

```yaml
frontend:
  build:
    context: ./WebUI.Frontend
    dockerfile: Dockerfile
    args:
      REACT_APP_API_URL: https://api.example.com/api
  container_name: lean-frontend
  depends_on:
    - api
  ports:
    - "3000:80"
  restart: unless-stopped
  networks:
    - leanui-network
```

#### Dockerfile for API

Create `WebUI.API/Dockerfile`:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["WebUI.API/WebUI.API.csproj", "WebUI.API/"]
COPY ["WebUI.Core/WebUI.Core.csproj", "WebUI.Core/"]
COPY ["WebUI.Data/WebUI.Data.csproj", "WebUI.Data/"]
RUN dotnet restore "WebUI.API/WebUI.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WebUI.API"
RUN dotnet build "WebUI.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "WebUI.API.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000 5001

# Install required tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create data and logs directories
RUN mkdir -p /app/data /app/logs

# Entry point
ENTRYPOINT ["dotnet", "WebUI.API.dll"]
```

#### Dockerfile for Frontend

Create `WebUI.Frontend/Dockerfile`:

```dockerfile
# Stage 1: Build
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
ARG REACT_APP_API_URL
ARG REACT_APP_SIGNALR_URL
RUN npm run build

# Stage 2: Runtime (Nginx)
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy built app
COPY --from=build /app/build .

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

### Environment Variables

#### Complete .env File

```env
# ===================================
# Database Configuration
# ===================================
DB_PASSWORD=your_secure_postgres_password_here
POSTGRES_DB=leanui
POSTGRES_USER=leanuser

# ===================================
# JWT Configuration
# ===================================
# Generate with: openssl rand -base64 32
JWT_SECRET=your_jwt_secret_key_at_least_32_characters_long_here
JWT_ISSUER=LeanWebUI
JWT_AUDIENCE=LeanWebUIClient
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=15
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7

# ===================================
# IBKR Configuration
# ===================================
IBKR_HOST=host.docker.internal
IBKR_PORT=7497
IBKR_CLIENT_ID=1
IBKR_ACCOUNT_ID=your_ibkr_account_id

# ===================================
# Application Configuration
# ===================================
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5001;http://+:5000

# ===================================
# Frontend Configuration
# ===================================
REACT_APP_API_URL=https://yourdomain.com/api
REACT_APP_SIGNALR_URL=https://yourdomain.com/hubs

# ===================================
# Logging
# ===================================
Serilog__MinimumLevel=Information
Serilog__WriteTo__Console__Enabled=true
Serilog__WriteTo__File__Enabled=true
Serilog__WriteTo__File__Path=/app/logs/log-.txt

# ===================================
# Resource Limits
# ===================================
# These can be set in docker-compose.yml
POSTGRES_MAX_CONNECTIONS=100
API_MAX_REQUESTS_PER_SECOND=1000
```

---

### Production Deployment

#### Production docker-compose.yml

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: lean-postgres
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups  # Backup directory
    ports:
      - "127.0.0.1:5432:5432"  # Only localhost
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 512M

  api:
    build:
      context: ./WebUI.API
      dockerfile: Dockerfile
      args:
        BUILD_CONFIGURATION: Release
    container_name: lean-api
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      Database__Provider: PostgreSQL
      Database__ConnectionString: Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${DB_PASSWORD};Pooling=true;Maximum Pool Size=20
      JWT__SecretKey: ${JWT_SECRET}
      IBKR__TWS__Host: ${IBKR_HOST}
      IBKR__TWS__Port: ${IBKR_PORT}
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://+:5001;http://+:5000
      Serilog__MinimumLevel: Warning
    ports:
      - "127.0.0.1:5000:5000"
      - "127.0.0.1:5001:5001"
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
      - ./strategies:/app/strategies
      - ./certificates:/app/certificates:ro  # SSL certificates
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "https://localhost:5001/health", "-k"]
      interval: 30s
      timeout: 10s
      retries: 3
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 512M

  frontend:
    build:
      context: ./WebUI.Frontend
      dockerfile: Dockerfile
      args:
        REACT_APP_API_URL: https://yourdomain.com/api
        REACT_APP_SIGNALR_URL: https://yourdomain.com/hubs
    container_name: lean-frontend
    depends_on:
      - api
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M

  nginx:
    image: nginx:alpine
    container_name: lean-nginx
    depends_on:
      - api
      - frontend
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
      - ./nginx/logs:/var/log/nginx
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M

volumes:
  postgres_data:

networks:
  default:
    name: leanui-network
```

---

### SSL/TLS Configuration

#### Generate Self-Signed Certificate (Development)

```bash
# Create directory
mkdir -p ./certificates

# Generate certificate
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ./certificates/lean-webui.key \
  -out ./certificates/lean-webui.crt \
  -subj "/C=US/ST=State/L=City/O=Organization/OU=IT/CN=localhost"

# Set permissions
chmod 600 ./certificates/lean-webui.key
```

#### Nginx Configuration with SSL

Create `nginx/nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    upstream api {
        server api:5001;
    }

    upstream frontend {
        server frontend:80;
    }

    # Redirect HTTP to HTTPS
    server {
        listen 80;
        server_name _;
        return 301 https://$host$request_uri;
    }

    # HTTPS Server
    server {
        listen 443 ssl http2;
        server_name _;

        # SSL Configuration
        ssl_certificate /etc/nginx/ssl/lean-webui.crt;
        ssl_certificate_key /etc/nginx/ssl/lean-webui.key;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;

        # Security Headers
        add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;

        # API Proxy
        location /api/ {
            proxy_pass https://api/api/;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;
        }

        # SignalR Hubs
        location /hubs/ {
            proxy_pass https://api/hubs/;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
        }

        # Frontend
        location / {
            proxy_pass http://frontend/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
```

#### Let's Encrypt (Production)

Use Certbot for free SSL certificates:

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal (already set up by Certbot)
sudo certbot renew --dry-run
```

---

### Monitoring and Logging

#### Docker Logs

```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View specific service logs
docker-compose logs api
docker-compose logs postgres

# View logs with timestamps
docker-compose logs -t -f api

# Save logs to file
docker-compose logs > logs.txt
```

#### Prometheus + Grafana (Optional)

Add to `docker-compose.yml`:

```yaml
  prometheus:
    image: prom/prometheus:latest
    container_name: lean-prometheus
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    ports:
      - "9090:9090"
    restart: unless-stopped

  grafana:
    image: grafana/grafana:latest
    container_name: lean-grafana
    depends_on:
      - prometheus
    ports:
      - "3001:3000"
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    restart: unless-stopped

volumes:
  prometheus_data:
  grafana_data:
```

---

### Backup and Maintenance

#### Automated Backup Script

Create `backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
CONTAINER_NAME="lean-postgres"
DB_NAME="leanui"
DB_USER="leanuser"

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup database
docker exec $CONTAINER_NAME pg_dump -U $DB_USER $DB_NAME | gzip > $BACKUP_DIR/backup_$DATE.sql.gz

# Keep only last 7 days
find $BACKUP_DIR -name "backup_*.sql.gz" -mtime +7 -delete

echo "Backup completed: backup_$DATE.sql.gz"
```

Make executable and schedule:

```bash
chmod +x backup.sh

# Add to crontab (daily at 2 AM)
crontab -e
0 2 * * * /path/to/Lean/WebUI/backup.sh
```

#### Update Services

```bash
# Pull latest images
docker-compose pull

# Rebuild and restart
docker-compose up -d --build

# Clean up old images
docker image prune -a -f
```

---

### Troubleshooting (Docker)

#### Container Won't Start

```bash
# Check logs
docker-compose logs api

# Check container status
docker-compose ps

# Inspect container
docker inspect lean-api

# Check resource usage
docker stats
```

#### Database Connection Issues

```bash
# Test database connection from API container
docker exec -it lean-api bash
apt update && apt install postgresql-client
psql -h postgres -U leanuser -d leanui

# Check PostgreSQL logs
docker-compose logs postgres
```

#### Port Already in Use

```bash
# Find process using port
# Linux/macOS
lsof -i :5000
# Windows
netstat -ano | findstr :5000

# Kill process or change port in docker-compose.yml
```

#### Permission Issues

```bash
# Fix volume permissions
sudo chown -R $USER:$USER ./data ./logs

# Or run containers as current user
docker-compose up --user $(id -u):$(id -g)
```

---

## 中文

### 目录

- [概述](#概述-2)
- [前置条件](#前置条件-2)
- [快速开始](#快速开始-2)
- [Docker Compose 配置](#docker-compose-配置-1)
- [环境变量](#环境变量-1)
- [生产部署](#生产部署-1)
- [SSL/TLS 配置](#ssltls-配置-1)
- [监控和日志](#监控和日志-1)
- [备份和维护](#备份和维护-1)
- [故障排查](#故障排查docker-1)

### 概述

Docker Compose 提供了部署 Lean WebUI 最简单、最可靠的方式。本指南涵盖：

- **开发设置**：快速本地测试
- **生产设置**：带 HTTPS、监控和备份的安全可扩展部署
- **多容器架构**：API、前端、数据库、Nginx

**架构**：

```
┌─────────────────────────────────────────────────────┐
│                   Docker 主机                         │
│                                                       │
│  ┌──────────┐    ┌──────────┐    ┌──────────────┐  │
│  │  Nginx   │───▶│  前端    │    │  PostgreSQL  │  │
│  │  (代理)  │    │ (React)  │    │  (数据库)    │  │
│  └────┬─────┘    └──────────┘    └──────┬───────┘  │
│       │                                  │          │
│       │          ┌──────────┐            │          │
│       └─────────▶│ 后端     │────────────┘          │
│                  │ (API)    │                       │
│                  └────┬─────┘                       │
│                       │                             │
│                       │ (IPC/进程)                  │
│                       ▼                             │
│                  ┌──────────┐                       │
│                  │  Lean    │                       │
│                  │  引擎    │                       │
│                  └──────────┘                       │
└─────────────────────────────────────────────────────┘
         │
         │ (TWS API)
         ▼
    ┌─────────┐
    │  IBKR   │
    │ TWS/GW  │
    └─────────┘
```

### 快速开始

#### 1. 克隆仓库

```bash
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI
```

#### 2. 创建环境文件

在 `WebUI` 目录中创建 `.env`：

```env
# 数据库配置
DB_PASSWORD=changeme123!
POSTGRES_DB=leanui
POSTGRES_USER=leanuser

# JWT 密钥（生成命令：openssl rand -base64 32）
JWT_SECRET=your_jwt_secret_key_at_least_32_characters_long_here

# IBKR 配置
IBKR_HOST=host.docker.internal
IBKR_PORT=7497
IBKR_CLIENT_ID=1

# 应用程序设置
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:5001;http://+:5000
```

#### 3. 启动服务

```bash
# 构建并启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f

# 检查状态
docker-compose ps
```

#### 4. 访问 WebUI

- 前端：http://localhost:3000
- API：http://localhost:5000
- API 文档：http://localhost:5000/swagger

默认凭证：
- 用户名：`admin`
- 密码：`ChangeMe123!`

---

详细的生产部署、SSL 配置、监控设置等内容请参考英文版 above部分。

---

**部署完成！祝您使用愉快！** 🐳
