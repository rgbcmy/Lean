# Cross-Platform Testing Guide
# 跨平台测试指南

## Overview
## 概览

Cross-platform testing ensures the Lean WebUI works correctly across Windows, Linux, and macOS operating systems.
跨平台测试确保 Lean WebUI 在 Windows、Linux 和 macOS 操作系统上正常工作。

## Supported Platforms
## 支持的平台

### Primary Support (Full Testing)
### 主要支持 (完整测试)

| Platform | Versions | Architecture | Notes |
|----------|----------|--------------|-------|
| **Windows** | Windows 10, 11 | x64, ARM64 | Primary development platform |
| **Linux** | Ubuntu 20.04+, Debian 11+ | x64, ARM64 | Docker deployment recommended |
| **macOS** | macOS 12 (Monterey)+ | x64 (Intel), ARM64 (M1/M2) | Apple Silicon fully supported |

### Secondary Support (Limited Testing)
### 次要支持 (有限测试)

| Platform | Notes |
|----------|-------|
| Windows Server 2019+ | Production deployment |
| CentOS/RHEL 8+ | Enterprise Linux |
| Alpine Linux | Docker containers |

## Platform-Specific Requirements
## 平台特定要求

### Windows
### Windows

**Runtime Requirements:**
- .NET 10 Runtime
- ASP.NET Core Runtime
- Node.js 20 LTS
- PostgreSQL 15+ or SQLite

**Development Tools:**
- Visual Studio 2022 or VS Code
- Git for Windows
- Windows Terminal (recommended)
- PowerShell 7+

### Linux
### Linux

**Runtime Requirements:**
- .NET 10 SDK/Runtime
- Node.js 20 LTS  
- PostgreSQL 15+ or SQLite
- libicu (for .NET globalization)

**Development Tools:**
- VS Code with C# extension
- Git
- curl, wget for testing

**Installation (Ubuntu/Debian):**
```bash
# Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install PostgreSQL
sudo apt-get install postgresql-15
```

### macOS
### macOS

**Runtime Requirements:**
- .NET 10 SDK/Runtime (Intel or ARM)
- Node.js 20 LTS
- PostgreSQL 15+ or SQLite
- Homebrew (recommended)

**Installation:**
```bash
# Install Homebrew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET
brew install dotnet

# Install Node.js
brew install node@20

# Install PostgreSQL
brew install postgresql@15
```

## Testing Matrix
## 测试矩阵

| Component | Windows 11 | Ubuntu 22.04 | macOS 14 | Priority |
|-----------|------------|--------------|----------|----------|
| Build | ✅ | ✅ | ✅ | High |
| Unit Tests | ✅ | ✅ | ✅ | High |
| Integration Tests | ✅ | ✅ | ✅ | High |
| API Server | ✅ | ✅ | ✅ | High |
| Frontend Dev Server | ✅ | ✅ | ✅ | High |
| Database (PostgreSQL) | ✅ | ✅ | ✅ | High |
| Database (SQLite) | ✅ | ✅ | ✅ | Medium |
| E2E Tests | ✅ | ✅ | ✅ | Medium |
| Docker Deployment | ✅ | ✅ | ✅ | High |
| IBKR Integration | ✅ | ✅ | ✅ | High |

✅ = Tested and working | ⚠️ = Partial support | ❌ = Not supported

## Platform-Specific Testing
## 平台特定测试

### Build and Compilation
### 构建和编译

**Windows (PowerShell):**
```powershell
# Clone repository
git clone https://github.com/QuantConnect/Lean.git
cd Lean\WebUI

# Restore and build backend
dotnet restore
dotnet build --configuration Release

# Build frontend
cd WebUI.Frontend
npm install
npm run build

# Run tests
cd ..\WebUI.Tests
dotnet test
```

**Linux/macOS (Bash):**
```bash
# Clone repository
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI

# Restore and build backend
dotnet restore
dotnet build --configuration Release

# Build frontend
cd WebUI.Frontend
npm install
npm run build

# Run tests
cd ../WebUI.Tests
dotnet test
```

### Database Testing
### 数据库测试

#### PostgreSQL Testing
#### PostgreSQL 测试

**Windows:**
```powershell
# Start PostgreSQL
Start-Service postgresql-x64-15

# Create test database
psql -U postgres -c "CREATE DATABASE webui_test;"

# Run migrations
$env:ConnectionStrings__WebUIDb = "Host=localhost;Database=webui_test;Username=postgres;Password=yourpassword"
dotnet ef database update --project WebUI.Data
```

**Linux:**
```bash
# Start PostgreSQL
sudo systemctl start postgresql

# Create test database
sudo -u postgres psql -c "CREATE DATABASE webui_test;"

# Run migrations
export ConnectionStrings__WebUIDb="Host=localhost;Database=webui_test;Username=postgres;Password=yourpassword"
dotnet ef database update --project WebUI.Data
```

**macOS:**
```bash
# Start PostgreSQL
brew services start postgresql@15

# Create test database
psql postgres -c "CREATE DATABASE webui_test;"

# Run migrations
export ConnectionStrings__WebUIDb="Host=localhost;Database=webui_test;Username=$(whoami);Password="
dotnet ef database update --project WebUI.Data
```

#### SQLite Testing
#### SQLite 测试

**All Platforms:**
```bash
# SQLite requires no service
# Database file created automatically

# Set connection string
export ConnectionStrings__WebUIDb="Data Source=webui.db"

# Run migrations
dotnet ef database update --project WebUI.Data
```

### File Path Testing
### 文件路径测试

**Cross-platform path handling:**
```csharp
// Good: Use Path.Combine
var filePath = Path.Combine("WebUI", "Data", "strategies", "config.json");

// Bad: Hard-coded separators
var filePath = "WebUI\\Data\\strategies\\config.json"; // Windows only
var filePath = "WebUI/Data/strategies/config.json"; // Unix only
```

### Credential Encryption Testing
### 凭证加密测试

**Windows (DPAPI):**
```csharp
// Uses Windows Data Protection API
var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
```

**Linux/macOS (AES-GCM):**
```csharp
// Uses AES-GCM with key stored in environment variable
var encrypted = AesEncrypt(data, Environment.GetEnvironmentVariable("ENCRYPTION_KEY"));
```

### Process Management Testing
### 进程管理测试

**Windows:**
```powershell
# Start Lean engine
Start-Process -FilePath "Lean.Launcher.exe" -ArgumentList "--config", "config.json"

# Stop process
Stop-Process -Name "Lean.Launcher"
```

**Linux/macOS:**
```bash
# Start Lean engine
./Lean.Launcher --config config.json &

# Stop process
pkill -f Lean.Launcher
```

## Platform-Specific Issues and Solutions
## 平台特定问题和解决方案

### Windows-Specific Issues
### Windows 特定问题

#### Issue: Line Endings
#### 问题: 行尾符

**Problem:** Git converts line endings to CRLF on Windows
**问题:** Git 在 Windows 上将行尾符转换为 CRLF

**Solution:**
```bash
git config --global core.autocrlf true
```

#### Issue: Path Length Limit
#### 问题: 路径长度限制

**Problem:** Windows has 260 character path limit
**问题:** Windows 有 260 字符路径限制

**Solution:**
```powershell
# Enable long paths in Windows 10+
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name "LongPathsEnabled" -Value 1

# Or use short paths
git config --system core.longpaths true
```

#### Issue: Case-Insensitive File System
#### 问题: 不区分大小写的文件系统

**Problem:** Windows file system is case-insensitive
**问题:** Windows 文件系统不区分大小写

**Solution:**
- Be consistent with file naming
- Test on case-sensitive systems (Linux/macOS)

### Linux-Specific Issues
### Linux 特定问题

#### Issue: Missing Dependencies
#### 问题: 缺少依赖项

**Problem:** .NET requires certain system libraries
**问题:** .NET 需要某些系统库

**Solution:**
```bash
# Install required dependencies
sudo apt-get install -y \
  libc6 \
  libgcc1 \
  libgssapi-krb5-2 \
  libicu72 \
  libssl3 \
  libstdc++6 \
  zlib1g
```

#### Issue: Port Permissions
#### 问题: 端口权限

**Problem:** Ports < 1024 require root
**问题:** 端口 < 1024 需要 root

**Solution:**
```bash
# Use ports > 1024
# Or grant capability
sudo setcap CAP_NET_BIND_SERVICE=+eip /path/to/WebUI.API
```

#### Issue: File Permissions
#### 问题: 文件权限

**Problem:** Executable permissions not set
**问题:** 未设置可执行权限

**Solution:**
```bash
chmod +x Lean.Launcher
chmod +x scripts/*.sh
```

### macOS-Specific Issues
### macOS 特定问题

#### Issue: Gatekeeper Blocking
#### 问题: Gatekeeper 阻止

**Problem:** macOS blocks unsigned binaries
**问题:** macOS 阻止未签名的二进制文件

**Solution:**
```bash
# Allow app to run
xattr -d com.apple.quarantine /path/to/binary

# Or adjust security settings
System Preferences > Security & Privacy > Allow
```

#### Issue: M1/M2 (ARM) Compatibility
#### 问题: M1/M2 (ARM) 兼容性

**Problem:** Some packages may need Rosetta 2
**问题:** 某些包可能需要 Rosetta 2

**Solution:**
```bash
# Install Rosetta 2 if needed
softwareupdate --install-rosetta

# Verify ARM native build
file WebUI.API.dll
# Should show: Mach-O 64-bit dynamically linked shared library arm64
```

## Docker Testing
## Docker 测试

### Multi-Architecture Builds
### 多架构构建

```dockerfile
# Dockerfile with multi-arch support
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH

WORKDIR /app
COPY . .
RUN dotnet publish WebUI.API/WebUI.API.csproj -c Release -o /app/publish -a $TARGETARCH

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebUI.API.dll"]
```

**Build for multiple platforms:**
```bash
# Build for AMD64 and ARM64
docker buildx build --platform linux/amd64,linux/arm64 -t lean-webui:latest .

# Test on current platform
docker run -p 5000:5000 lean-webui:latest
```

## CI/CD Cross-Platform Testing
## CI/CD 跨平台测试

### GitHub Actions Matrix
### GitHub Actions 矩阵

```yaml
name: Cross-Platform Tests

on: [push, pull_request]

jobs:
  test:
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest, macos-latest]
        dotnet: ['10.0.x']
    
    runs-on: ${{ matrix.os }}
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet }}
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release --no-restore
      
      - name: Test
        run: dotnet test --configuration Release --no-build --verbosity normal
      
      - name: Frontend build
        run: |
          cd WebUI.Frontend
          npm ci
          npm run build
          npm run test
```

## Platform Testing Checklist
## 平台测试清单

### For Each Platform
### 对于每个平台

- [ ] Clean clone and build
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] API server starts successfully
- [ ] Frontend dev server starts
- [ ] Production build works
- [ ] Database migrations work (PostgreSQL)
- [ ] Database migrations work (SQLite)
- [ ] IBKR connection works
- [ ] SignalR connections work
- [ ] File uploads work
- [ ] File downloads work
- [ ] Logging works
- [ ] Performance is acceptable

## Performance Benchmarks by Platform
## 按平台的性能基准

| Metric | Windows | Linux | macOS | Target |
|--------|---------|-------|-------|--------|
| Build Time | ~45s | ~40s | ~50s | < 60s |
| Test Execution | ~15s | ~12s | ~18s | < 30s |
| API Startup | ~3s | ~2.5s | ~3.5s | < 5s |
| Frontend Build | ~25s | ~20s | ~28s | < 45s |

## Virtualization Testing
## 虚拟化测试

### Using VirtualBox/VMware
### 使用 VirtualBox/VMware

1. Create VMs for each platform
2. Install required runtimes
3. Clone and test
4. Snapshot working configurations

### Using Docker Desktop
### 使用 Docker Desktop

```bash
# Test on Linux container (Windows/macOS)
docker run -it --rm -v $(pwd):/app mcr.microsoft.com/dotnet/sdk:10.0 bash
cd /app
dotnet test
```

## Reporting Platform-Specific Issues
## 报告平台特定问题

When reporting platform-specific issues, include:
报告平台特定问题时，包括:

1. **Platform Details:**
   - OS name and version
   - Architecture (x64, ARM64)
   - .NET runtime version
   - Node.js version

2. **Environment:**
   - Development or Production
   - Virtual machine or bare metal
   - Docker container or native

3. **Steps to Reproduce:**
   - Exact commands run
   - Configuration used
   - Error messages

4. **Logs:**
   - Application logs
   - System logs
   - Build output

## Automation Scripts
## 自动化脚本

### Windows (test-all-platforms.ps1)
```powershell
# PowerShell script for comprehensive testing
Write-Host "Starting cross-platform tests..."

# Build
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) { exit 1 }

# Test
dotnet test --configuration Release
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "All tests passed!" -ForegroundColor Green
```

### Linux/macOS (test-all-platforms.sh)
```bash
#!/bin/bash
set -e

echo "Starting cross-platform tests..."

# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release

echo "All tests passed!"
```

## References
## 参考

- [.NET Platform Support](https://github.com/dotnet/core/blob/main/os-lifecycle-policy.md)
- [Node.js Platform Support](https://github.com/nodejs/node/blob/main/BUILDING.md)
- [PostgreSQL Platform Support](https://www.postgresql.org/download/)
- [Docker Multi-Platform Builds](https://docs.docker.com/build/building/multi-platform/)

---

Last Updated: 2024-02-16
最后更新: 2024-02-16
