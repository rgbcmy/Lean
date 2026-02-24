# Database Configuration Guide
# 数据库配置教程

[English](#english) | [中文](#中文)

---

## English

### Table of Contents

- [Overview](#overview)
- [Database Options](#database-options)
- [PostgreSQL Setup](#postgresql-setup)
- [SQLite Setup](#sqlite-setup)
- [Switching Between Databases](#switching-between-databases)
- [Database Migrations](#database-migrations)
- [Backup and Restore](#backup-and-restore)
- [Performance Tuning](#performance-tuning)
- [Troubleshooting](#troubleshooting-database)

---

### Overview

Lean WebUI supports two database providers:

| Database | Use Case | Pros | Cons |
|----------|----------|------|------|
| **PostgreSQL** | Production environment | High performance, scalability, concurrent connections, ACID compliance | Requires separate server |
| **SQLite** | Development/testing | Lightweight, no server required, single file | Limited concurrency, not suitable for high load |

**Recommendation**:
- **Development**: SQLite (quick setup, no configuration)
- **Production**: PostgreSQL (better performance and reliability)

---

### Database Options

#### PostgreSQL

**Best for**:
- Production deployments
- Multiple concurrent users
- Large datasets (>10GB)
- High transaction volume

**Requirements**:
- PostgreSQL 14+ installed
- ~100MB RAM per connection
- Disk space: 500MB+ (grows with data)

#### SQLite

**Best for**:
- Local development
- Single-user deployments
- Testing and prototyping
- Embedded systems

**Requirements**:
- No separate server needed
- Minimal RAM (~50MB)
- Disk space: Database file size (50MB - 5GB typical)

---

### PostgreSQL Setup

#### Installation

##### Windows

1. **Download PostgreSQL**:
   - Visit [PostgreSQL Downloads](https://www.postgresql.org/download/windows/)
   - Download the installer (PostgreSQL 15 recommended)
   - Run the installer

2. **Installation Steps**:
   - Choose installation directory (default: `C:\Program Files\PostgreSQL\15`)
   - Select components: PostgreSQL Server, pgAdmin 4, Command Line Tools
   - Set data directory (default: `C:\Program Files\PostgreSQL\15\data`)
   - Set superuser password (remember this!)
   - Port: `5432` (default)
   - Locale: `Default locale`
   - Complete installation

3. **Verify Installation**:
   ```powershell
   # Check if PostgreSQL service is running
   Get-Service -Name postgresql-x64-15

   # Connect to PostgreSQL
   psql -U postgres
   ```

##### Linux (Ubuntu/Debian)

```bash
# Update package list
sudo apt update

# Install PostgreSQL
sudo apt install postgresql postgresql-contrib

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Verify installation
sudo systemctl status postgresql

# Connect to PostgreSQL
sudo -u postgres psql
```

##### macOS

```bash
# Install using Homebrew
brew install postgresql@15

# Start PostgreSQL service
brew services start postgresql@15

# Connect to PostgreSQL
psql postgres
```

#### Create Database and User

Once PostgreSQL is installed and running:

```sql
-- Connect as superuser
-- Linux/macOS: sudo -u postgres psql
-- Windows: psql -U postgres

-- Create database
CREATE DATABASE leanui;

-- Create user
CREATE USER leanuser WITH PASSWORD 'your_secure_password';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;

-- Grant schema privileges (PostgreSQL 15+)
\c leanui
GRANT ALL ON SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO leanuser;

-- Verify
\l  -- List databases
\du -- List users

-- Exit
\q
```

#### Configure WebUI to Use PostgreSQL

Edit `WebUI.API/appsettings.json`:

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=your_secure_password"
  }
}
```

**Connection String Parameters**:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Host` | Database server hostname/IP | `localhost`, `192.168.1.100` |
| `Port` | PostgreSQL port | `5432` (default) |
| `Database` | Database name | `leanui` |
| `Username` | Database user | `leanuser` |
| `Password` | User password | `mySecureP@ssw0rd` |
| `Pooling` | Enable connection pooling | `true` (default) |
| `Minimum Pool Size` | Min connections | `1` |
| `Maximum Pool Size` | Max connections | `20` |

**Example with connection pooling**:

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=your_secure_password;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20"
  }
}
```

#### Test Connection

```bash
# Test PostgreSQL connection
psql -h localhost -p 5432 -U leanuser -d leanui

# In psql, run:
SELECT version();

# Exit
\q
```

---

### SQLite Setup

#### Installation

SQLite requires no separate installation - the library is included with .NET.

#### Configure WebUI to Use SQLite

Edit `WebUI.API/appsettings.json`:

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

**Connection String Options**:

```json
// Relative path (current directory)
"ConnectionString": "Data Source=lean.db"

// Absolute path
"ConnectionString": "Data Source=C:/data/lean.db"

// In-memory database (for testing, data lost on restart)
"ConnectionString": "Data Source=:memory:"

// With additional options
"ConnectionString": "Data Source=lean.db;Cache=Shared;Mode=ReadWriteCreate"
```

#### Database File Location

By default, `lean.db` is created in the WebUI API working directory:

- **Docker**: `/app/data/lean.db`
- **Local**: `WebUI.API/lean.db`

To specify a custom location:

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=/var/lib/leanui/lean.db"
  }
}
```

#### Verify SQLite Database

```bash
# Install SQLite command-line tool (if not already installed)
# Windows: Download from https://www.sqlite.org/download.html
# Linux: sudo apt install sqlite3
# macOS: brew install sqlite3

# Open database
sqlite3 lean.db

# List tables
.tables

# Show schema
.schema

# Exit
.quit
```

---

### Switching Between Databases

#### From SQLite to PostgreSQL

1. **Backup SQLite data** (optional):
   ```bash
   sqlite3 lean.db .dump > backup.sql
   ```

2. **Change configuration**:
   ```json
   {
     "Database": {
       "Provider": "PostgreSQL",
       "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password"
     }
   }
   ```

3. **Run migrations**:
   ```bash
   cd WebUI.API
   dotnet ef database update
   ```

4. **(Optional) Migrate data**:
   - Export data from SQLite
   - Import into PostgreSQL
   - Or start fresh (migrations create empty tables)

#### From PostgreSQL to SQLite

1. **Backup PostgreSQL data** (optional):
   ```bash
   pg_dump -U leanuser leanui > backup.sql
   ```

2. **Change configuration**:
   ```json
   {
     "Database": {
       "Provider": "SQLite",
       "ConnectionString": "Data Source=lean.db"
     }
   }
   ```

3. **Delete existing SQLite file** (if any):
   ```bash
   rm lean.db
   ```

4. **Run migrations**:
   ```bash
   cd WebUI.API
   dotnet ef database update
   ```

---

### Database Migrations

Lean WebUI uses Entity Framework Core migrations to manage database schema.

#### Apply Migrations

When first starting or after updates:

```bash
cd WebUI.API
dotnet ef database update
```

**Docker**:

```bash
docker-compose exec api dotnet ef database update
```

#### Check Migration Status

```bash
# List all migrations
dotnet ef migrations list

# View pending migrations
dotnet ef migrations has-pending-model-changes
```

#### Create New Migration (for developers)

```bash
# After modifying entity models
dotnet ef migrations add YourMigrationName

# Preview SQL
dotnet ef migrations script

# Apply migration
dotnet ef database update
```

#### Rollback Migration

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Rollback all migrations (reset database)
dotnet ef database update 0
```

---

### Backup and Restore

#### PostgreSQL

##### Backup

```bash
# Full database backup
pg_dump -U leanuser -h localhost -d leanui -F c -f leanui_backup.dump

# Plain SQL backup
pg_dump -U leanuser -h localhost -d leanui > leanui_backup.sql

# Backup specific tables
pg_dump -U leanuser -h localhost -d leanui -t orders -t positions > backup_trades.sql
```

##### Restore

```bash
# Restore from custom format
pg_restore -U leanuser -h localhost -d leanui -c leanui_backup.dump

# Restore from SQL file
psql -U leanuser -h localhost -d leanui < leanui_backup.sql

# Restore to new database
createdb -U postgres leanui_restored
psql -U leanuser -h localhost -d leanui_restored < leanui_backup.sql
```

##### Automated Backups (Linux)

Create a backup script `/usr/local/bin/backup-leanui.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/var/backups/leanui"
DATE=$(date +%Y%m%d_%H%M%S)
FILENAME="leanui_$DATE.dump"

mkdir -p $BACKUP_DIR
pg_dump -U leanuser -h localhost -d leanui -F c -f $BACKUP_DIR/$FILENAME

# Keep only last 7 days of backups
find $BACKUP_DIR -name "leanui_*.dump" -mtime +7 -delete

echo "Backup completed: $FILENAME"
```

Make executable and add to cron:

```bash
chmod +x /usr/local/bin/backup-leanui.sh

# Add to crontab (daily at 2 AM)
crontab -e
0 2 * * * /usr/local/bin/backup-leanui.sh
```

#### SQLite

##### Backup

```bash
# Simple file copy (WebUI must be stopped)
cp lean.db lean_backup_$(date +%Y%m%d).db

# Online backup (WebUI can be running)
sqlite3 lean.db ".backup lean_backup.db"

# Export to SQL
sqlite3 lean.db .dump > lean_backup.sql
```

##### Restore

```bash
# Stop WebUI
# Replace database file
cp lean_backup.db lean.db

# Or restore from SQL
rm lean.db
sqlite3 lean.db < lean_backup.sql
```

---

### Performance Tuning

#### PostgreSQL Optimization

Edit `/etc/postgresql/15/main/postgresql.conf` (Linux) or `data/postgresql.conf` (Windows):

```conf
# Memory settings (adjust based on RAM)
shared_buffers = 256MB           # 25% of RAM
effective_cache_size = 1GB       # 50-75% of RAM
maintenance_work_mem = 64MB
work_mem = 16MB

# Connection settings
max_connections = 100
shared_preload_libraries = 'pg_stat_statements'

# Logging (for debugging slow queries)
log_min_duration_statement = 1000  # Log queries slower than 1 second
```

Restart PostgreSQL after changes:

```bash
# Linux
sudo systemctl restart postgresql

# Windows
Restart-Service postgresql-x64-15
```

##### Create Indexes

```sql
-- Connect to database
psql -U leanuser -d leanui

-- Create index on frequently queried columns
CREATE INDEX idx_orders_symbol ON orders(symbol);
CREATE INDEX idx_orders_created_at ON orders(created_at);
CREATE INDEX idx_positions_symbol ON positions(symbol);
CREATE INDEX idx_strategy_executions_strategy_id ON strategy_executions(strategy_id);

-- View existing indexes
\di

-- Exit
\q
```

#### SQLite Optimization

Add to connection string:

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db;Cache=Shared;Pooling=True;Journal Mode=WAL"
  }
}
```

**Performance options**:
- `Cache=Shared`: Share cache between connections
- `Journal Mode=WAL`: Write-Ahead Logging for better concurrency
- `Pooling=True`: Enable connection pooling

---

### Troubleshooting (Database)

#### PostgreSQL: "Connection Refused"

**Symptoms**: Cannot connect to PostgreSQL

**Causes & Solutions**:

1. **PostgreSQL not running**:
   ```bash
   # Linux
   sudo systemctl status postgresql
   sudo systemctl start postgresql

   # Windows
   Get-Service postgresql-x64-15
   Start-Service postgresql-x64-15
   ```

2. **Wrong port**:
   - Check PostgreSQL is listening on 5432:
     ```bash
     netstat -an | grep 5432
     ```

3. **Firewall blocking**:
   ```bash
   # Linux
   sudo ufw allow 5432

   # Windows: Add firewall rule for port 5432
   ```

4. **Wrong host in connection string**:
   - Use `localhost` or `127.0.0.1` for local connections
   - Check `pg_hba.conf` for allowed hosts

#### PostgreSQL: "Authentication Failed"

**Causes & Solutions**:

1. **Wrong password**:
   - Reset password:
     ```sql
     ALTER USER leanuser WITH PASSWORD 'new_password';
     ```

2. **User doesn't exist**:
   ```sql
   -- List users
   \du
   -- Create user if needed
   CREATE USER leanuser WITH PASSWORD 'password';
   ```

3. **Wrong authentication method in `pg_hba.conf`**:
   - Edit `/etc/postgresql/15/main/pg_hba.conf`
   - Change `peer` to `md5` for local connections:
     ```
     local   all             all                                     md5
     ```
   - Restart PostgreSQL

#### SQLite: "Database is Locked"

**Causes & Solutions**:

1. **Multiple writers**:
   - SQLite doesn't handle concurrent writes well
   - Solution: Use PostgreSQL for production

2. **Long-running transaction**:
   - Restart WebUI to release locks

3. **Enable WAL mode**:
   ```json
   "ConnectionString": "Data Source=lean.db;Journal Mode=WAL"
   ```

#### Migration Errors

**Error**: "The migration 'xxx' has already been applied to the database"

**Solution**:
```bash
# Check migration status
dotnet ef migrations list

# If needed, remove and reapply
dotnet ef database update 0
dotnet ef database update
```

**Error**: "Could not load type 'Microsoft.EntityFrameworkCore.Design'"

**Solution**:
```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Or update
dotnet tool update --global dotnet-ef
```

---

## 中文

### 目录

- [概述](#概述-1)
- [数据库选项](#数据库选项-1)
- [PostgreSQL 设置](#postgresql-设置)
- [SQLite 设置](#sqlite-设置)
- [数据库切换](#数据库切换)
- [数据库迁移](#数据库迁移)
- [备份和恢复](#备份和恢复)
- [性能调优](#性能调优)
- [故障排查](#故障排查数据库)

---

### 概述

Lean WebUI 支持两种数据库提供程序：

| 数据库 | 使用场景 | 优点 | 缺点 |
|--------|----------|------|------|
| **PostgreSQL** | 生产环境 | 高性能、可扩展性、并发连接、ACID 合规 | 需要单独服务器 |
| **SQLite** | 开发/测试 | 轻量级、无需服务器、单文件 | 并发性有限、不适合高负载 |

**推荐**：
- **开发环境**：SQLite（快速设置，无需配置）
- **生产环境**：PostgreSQL（更好的性能和可靠性）

---

### 数据库选项

#### PostgreSQL

**最适合**：
- 生产部署
- 多用户并发
- 大数据集（>10GB）
- 高交易量

**要求**：
- 已安装 PostgreSQL 14+
- 每个连接约 100MB RAM
- 磁盘空间：500MB+（随数据增长）

#### SQLite

**最适合**：
- 本地开发
- 单用户部署
- 测试和原型
- 嵌入式系统

**要求**：
- 无需单独服务器
- 最小 RAM（约 50MB）
- 磁盘空间：数据库文件大小（典型 50MB - 5GB）

---

### PostgreSQL 设置

#### 安装

##### Windows

1. **下载 PostgreSQL**：
   - 访问 [PostgreSQL 下载](https://www.postgresql.org/download/windows/)
   - 下载安装程序（推荐 PostgreSQL 15）
   - 运行安装程序

2. **安装步骤**：
   - 选择安装目录（默认：`C:\Program Files\PostgreSQL\15`）
   - 选择组件：PostgreSQL Server、pgAdmin 4、命令行工具
   - 设置数据目录（默认：`C:\Program Files\PostgreSQL\15\data`）
   - 设置超级用户密码（记住此密码！）
   - 端口：`5432`（默认）
   - 区域设置：`Default locale`
   - 完成安装

3. **验证安装**：
   ```powershell
   # 检查 PostgreSQL 服务是否运行
   Get-Service -Name postgresql-x64-15

   # 连接到 PostgreSQL
   psql -U postgres
   ```

##### Linux (Ubuntu/Debian)

```bash
# 更新包列表
sudo apt update

# 安装 PostgreSQL
sudo apt install postgresql postgresql-contrib

# 启动 PostgreSQL 服务
sudo systemctl start postgresql
sudo systemctl enable postgresql

# 验证安装
sudo systemctl status postgresql

# 连接到 PostgreSQL
sudo -u postgres psql
```

##### macOS

```bash
# 使用 Homebrew 安装
brew install postgresql@15

# 启动 PostgreSQL 服务
brew services start postgresql@15

# 连接到 PostgreSQL
psql postgres
```

#### 创建数据库和用户

PostgreSQL 安装并运行后：

```sql
-- 以超级用户身份连接
-- Linux/macOS: sudo -u postgres psql
-- Windows: psql -U postgres

-- 创建数据库
CREATE DATABASE leanui;

-- 创建用户
CREATE USER leanuser WITH PASSWORD 'your_secure_password';

-- 授予权限
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;

-- 授予架构权限（PostgreSQL 15+）
\c leanui
GRANT ALL ON SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO leanuser;

-- 验证
\l  -- 列出数据库
\du -- 列出用户

-- 退出
\q
```

#### 配置 WebUI 使用 PostgreSQL

编辑 `WebUI.API/appsettings.json`：

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=your_secure_password"
  }
}
```

**连接字符串参数**：

| 参数 | 说明 | 示例 |
|------|------|------|
| `Host` | 数据库服务器主机名/IP | `localhost`、`192.168.1.100` |
| `Port` | PostgreSQL 端口 | `5432`（默认） |
| `Database` | 数据库名称 | `leanui` |
| `Username` | 数据库用户 | `leanuser` |
| `Password` | 用户密码 | `mySecureP@ssw0rd` |
| `Pooling` | 启用连接池 | `true`（默认） |
| `Minimum Pool Size` | 最小连接数 | `1` |
| `Maximum Pool Size` | 最大连接数 | `20` |

**带连接池的示例**：

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=your_secure_password;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20"
  }
}
```

#### 测试连接

```bash
# 测试 PostgreSQL 连接
psql -h localhost -p 5432 -U leanuser -d leanui

# 在 psql 中运行：
SELECT version();

# 退出
\q
```

---

### SQLite 设置

#### 安装

SQLite 无需单独安装 - 库已包含在 .NET 中。

#### 配置 WebUI 使用 SQLite

编辑 `WebUI.API/appsettings.json`：

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

**连接字符串选项**：

```json
// 相对路径（当前目录）
"ConnectionString": "Data Source=lean.db"

// 绝对路径
"ConnectionString": "Data Source=C:/data/lean.db"

// 内存数据库（用于测试，重启后数据丢失）
"ConnectionString": "Data Source=:memory:"

// 带附加选项
"ConnectionString": "Data Source=lean.db;Cache=Shared;Mode=ReadWriteCreate"
```

#### 数据库文件位置

默认情况下，`lean.db` 在 WebUI API 工作目录中创建：

- **Docker**：`/app/data/lean.db`
- **本地**：`WebUI.API/lean.db`

指定自定义位置：

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=/var/lib/leanui/lean.db"
  }
}
```

#### 验证 SQLite 数据库

```bash
# 安装 SQLite 命令行工具（如果尚未安装）
# Windows: 从 https://www.sqlite.org/download.html 下载
# Linux: sudo apt install sqlite3
# macOS: brew install sqlite3

# 打开数据库
sqlite3 lean.db

# 列出表
.tables

# 显示架构
.schema

# 退出
.quit
```

---

### 数据库切换

#### 从 SQLite 切换到 PostgreSQL

1. **备份 SQLite 数据**（可选）：
   ```bash
   sqlite3 lean.db .dump > backup.sql
   ```

2. **更改配置**：
   ```json
   {
     "Database": {
       "Provider": "PostgreSQL",
       "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password"
     }
   }
   ```

3. **运行迁移**：
   ```bash
   cd WebUI.API
   dotnet ef database update
   ```

4. **（可选）迁移数据**：
   - 从 SQLite 导出数据
   - 导入到 PostgreSQL
   - 或从头开始（迁移创建空表）

#### 从 PostgreSQL 切换到 SQLite

1. **备份 PostgreSQL 数据**（可选）：
   ```bash
   pg_dump -U leanuser leanui > backup.sql
   ```

2. **更改配置**：
   ```json
   {
     "Database": {
       "Provider": "SQLite",
       "ConnectionString": "Data Source=lean.db"
     }
   }
   ```

3. **删除现有 SQLite 文件**（如果有）：
   ```bash
   rm lean.db
   ```

4. **运行迁移**：
   ```bash
   cd WebUI.API
   dotnet ef database update
   ```

---

### 数据库迁移

Lean WebUI 使用 Entity Framework Core 迁移管理数据库架构。

#### 应用迁移

首次启动或更新后：

```bash
cd WebUI.API
dotnet ef database update
```

**Docker**：

```bash
docker-compose exec api dotnet ef database update
```

#### 检查迁移状态

```bash
# 列出所有迁移
dotnet ef migrations list

# 查看待处理的迁移
dotnet ef migrations has-pending-model-changes
```

#### 创建新迁移（开发者）

```bash
# 修改实体模型后
dotnet ef migrations add YourMigrationName

# 预览 SQL
dotnet ef migrations script

# 应用迁移
dotnet ef database update
```

#### 回滚迁移

```bash
# 回滚到特定迁移
dotnet ef database update PreviousMigrationName

# 回滚所有迁移（重置数据库）
dotnet ef database update 0
```

---

### 备份和恢复

#### PostgreSQL

##### 备份

```bash
# 完整数据库备份
pg_dump -U leanuser -h localhost -d leanui -F c -f leanui_backup.dump

# 纯 SQL 备份
pg_dump -U leanuser -h localhost -d leanui > leanui_backup.sql

# 备份特定表
pg_dump -U leanuser -h localhost -d leanui -t orders -t positions > backup_trades.sql
```

##### 恢复

```bash
# 从自定义格式恢复
pg_restore -U leanuser -h localhost -d leanui -c leanui_backup.dump

# 从 SQL 文件恢复
psql -U leanuser -h localhost -d leanui < leanui_backup.sql

# 恢复到新数据库
createdb -U postgres leanui_restored
psql -U leanuser -h localhost -d leanui_restored < leanui_backup.sql
```

##### 自动备份（Linux）

创建备份脚本 `/usr/local/bin/backup-leanui.sh`：

```bash
#!/bin/bash
BACKUP_DIR="/var/backups/leanui"
DATE=$(date +%Y%m%d_%H%M%S)
FILENAME="leanui_$DATE.dump"

mkdir -p $BACKUP_DIR
pg_dump -U leanuser -h localhost -d leanui -F c -f $BACKUP_DIR/$FILENAME

# 仅保留最近 7 天的备份
find $BACKUP_DIR -name "leanui_*.dump" -mtime +7 -delete

echo "备份完成：$FILENAME"
```

使其可执行并添加到 cron：

```bash
chmod +x /usr/local/bin/backup-leanui.sh

# 添加到 crontab（每天凌晨 2 点）
crontab -e
0 2 * * * /usr/local/bin/backup-leanui.sh
```

#### SQLite

##### 备份

```bash
# 简单文件复制（WebUI 必须停止）
cp lean.db lean_backup_$(date +%Y%m%d).db

# 在线备份（WebUI 可以运行）
sqlite3 lean.db ".backup lean_backup.db"

# 导出为 SQL
sqlite3 lean.db .dump > lean_backup.sql
```

##### 恢复

```bash
# 停止 WebUI
# 替换数据库文件
cp lean_backup.db lean.db

# 或从 SQL 恢复
rm lean.db
sqlite3 lean.db < lean_backup.sql
```

---

### 性能调优

#### PostgreSQL 优化

编辑 `/etc/postgresql/15/main/postgresql.conf`（Linux）或 `data/postgresql.conf`（Windows）：

```conf
# 内存设置（根据 RAM 调整）
shared_buffers = 256MB           # RAM 的 25%
effective_cache_size = 1GB       # RAM 的 50-75%
maintenance_work_mem = 64MB
work_mem = 16MB

# 连接设置
max_connections = 100
shared_preload_libraries = 'pg_stat_statements'

# 日志记录（用于调试慢查询）
log_min_duration_statement = 1000  # 记录超过 1 秒的查询
```

更改后重启 PostgreSQL：

```bash
# Linux
sudo systemctl restart postgresql

# Windows
Restart-Service postgresql-x64-15
```

##### 创建索引

```sql
-- 连接到数据库
psql -U leanuser -d leanui

-- 为频繁查询的列创建索引
CREATE INDEX idx_orders_symbol ON orders(symbol);
CREATE INDEX idx_orders_created_at ON orders(created_at);
CREATE INDEX idx_positions_symbol ON positions(symbol);
CREATE INDEX idx_strategy_executions_strategy_id ON strategy_executions(strategy_id);

-- 查看现有索引
\di

-- 退出
\q
```

#### SQLite 优化

添加到连接字符串：

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db;Cache=Shared;Pooling=True;Journal Mode=WAL"
  }
}
```

**性能选项**：
- `Cache=Shared`：在连接之间共享缓存
- `Journal Mode=WAL`：预写日志用于更好的并发性
- `Pooling=True`：启用连接池

---

### 故障排查（数据库）

#### PostgreSQL："连接被拒绝"

**症状**：无法连接到 PostgreSQL

**原因和解决方法**：

1. **PostgreSQL 未运行**：
   ```bash
   # Linux
   sudo systemctl status postgresql
   sudo systemctl start postgresql

   # Windows
   Get-Service postgresql-x64-15
   Start-Service postgresql-x64-15
   ```

2. **端口错误**：
   - 检查 PostgreSQL 是否在 5432 上监听：
     ```bash
     netstat -an | grep 5432
     ```

3. **防火墙阻止**：
   ```bash
   # Linux
   sudo ufw allow 5432

   # Windows：为端口 5432 添加防火墙规则
   ```

4. **连接字符串中的主机错误**：
   - 本地连接使用 `localhost` 或 `127.0.0.1`
   - 检查 `pg_hba.conf` 允许的主机

#### PostgreSQL："身份验证失败"

**原因和解决方法**：

1. **密码错误**：
   - 重置密码：
     ```sql
     ALTER USER leanuser WITH PASSWORD 'new_password';
     ```

2. **用户不存在**：
   ```sql
   -- 列出用户
   \du
   -- 如果需要，创建用户
   CREATE USER leanuser WITH PASSWORD 'password';
   ```

3. **`pg_hba.conf` 中的身份验证方法错误**：
   - 编辑 `/etc/postgresql/15/main/pg_hba.conf`
   - 将本地连接的 `peer` 更改为 `md5`：
     ```
     local   all             all                                     md5
     ```
   - 重启 PostgreSQL

#### SQLite："数据库已锁定"

**原因和解决方法**：

1. **多个写入者**：
   - SQLite 不能很好地处理并发写入
   - 解决：生产环境使用 PostgreSQL

2. **长时间运行的事务**：
   - 重启 WebUI 以释放锁

3. **启用 WAL 模式**：
   ```json
   "ConnectionString": "Data Source=lean.db;Journal Mode=WAL"
   ```

#### 迁移错误

**错误**："迁移 'xxx' 已应用到数据库"

**解决**：
```bash
# 检查迁移状态
dotnet ef migrations list

# 如果需要，删除并重新应用
dotnet ef database update 0
dotnet ef database update
```

**错误**："无法加载类型 'Microsoft.EntityFrameworkCore.Design'"

**解决**：
```bash
# 安装 EF Core 工具
dotnet tool install --global dotnet-ef

# 或更新
dotnet tool update --global dotnet-ef
```

---

**配置完成！祝您数据管理顺利！** 🗄️
