# Lean WebUI 开发者完整指南

本文档是 Lean WebUI 的完整开发者指南,涵盖项目架构、开发环境设置、编码规范、调试技巧和最佳实践。

> **相关文档**: 
> - 用户指南: [WebUI用户指南.md](./WebUI用户指南.md)  
> - API 参考: [WebUI-API-Reference.md](./WebUI-API-Reference.md)
> - 部署指南: [Docker-Deployment-Guide.md](./Docker-Deployment-Guide.md)

---

## 目录

1. [项目概述](#1-项目概述)
2. [技术栈](#2-技术栈)
3. [项目结构](#3-项目结构)
4. [开发环境设置](#4-开发环境设置)
5. [架构设计](#5-架构设计)
6. [后端开发](#6-后端开发)
7. [前端开发](#7-前端开发)
8. [数据库开发](#8-数据库开发)
9. [测试指南](#9-测试指南)
10. [代码规范](#10-代码规范)
11. [调试技巧](#11-调试技巧)
12. [部署流程](#12-部署流程)
13. [贡献指南](#13-贡献指南)

---

## 1. 项目概述

Lean WebUI 是 QuantConnect Lean 量化交易引擎的现代化 Web 管理界面,采用前后端分离架构,专为个人量化交易者设计。

### 核心功能模块

- **实盘交易**: 通过 IBKR 进行美股股票和 ETF 交易
- **策略管理**: 可视化管理和监控 Lean 策略执行  
- **回测系统**: 历史数据回测、参数优化、结果对比
- **实时监控**: 账户、持仓、订单状态实时推送
- **风险控制**: 止损止盈、仓位限制、交易频率控制
- **数据可视化**: K线图、技术指标、收益曲线、持仓分布

### 设计理念

- **安全第一**: HTTPS 加密、JWT 认证、操作审计
- **性能优先**: 异步 I/O、连接池、缓存策略、查询优化
- **可扩展性**: 模块化设计、依赖注入、插件架构
- **用户友好**: 响应式设计、中文支持、直观操作流程

---

## 2. 技术栈

### 后端技术

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0+ | 运行时框架 |
| ASP.NET Core | 8.0+ | Web API 框架 |
| Entity Framework Core | 8.0+ | ORM 框架 |
| SignalR | 8.0+ | 实时双向通信 |
| PostgreSQL | 15+ | 生产环境数据库 |
| SQLite | 3.35+ | 开发环境数据库 |
| Serilog | 3.0+ | 结构化日志 |
| FluentValidation | 11.0+ | 数据验证 |
| JWT Bearer | 8.0+ | 身份认证 |
| Swagger | 6.0+ | API 文档 |

### 前端技术

| 技术 | 版本 | 用途 |
|------|------|------|
| React | 18+ | UI 框架 |
| TypeScript | 5.0+ | 类型系统 |
| Ant Design | 5.x | UI 组件库 |
| Zustand | 4.0+ | 轻量级状态管理 |
| ECharts | 5.4+ | 数据可视化 |
| Lightweight Charts | 4.0+ | 金融图表 |
| Axios | 1.6+ | HTTP 客户端 |
| @microsoft/signalr | 8.0+ | SignalR 客户端 |
| React Router | 6.x | 路由管理 |
| dayjs | 1.11+ | 日期处理 |

### 开发工具

- **IDE**: Visual Studio 2022 / VS Code / JetBrains Rider
- **数据库工具**: pgAdmin (PostgreSQL) / DB Browser (SQLite)
- **API 测试**: Postman / Thunder Client / Swagger UI
- **版本控制**: Git
- **容器化**: Docker Desktop
- **包管理**: NuGet / npm

---

## 3. 项目结构

```
Lean/
├── WebUI/
│   ├── WebUI.API/                    # ASP.NET Core Web API 项目
│   │   ├── Controllers/              # API 控制器
│   │   │   ├── AuthController.cs    # 认证控制器
│   │   │   ├── TradingController.cs # 交易控制器
│   │   │   ├── StrategyController.cs# 策略控制器
│   │   │   └── ...
│   │   ├── Hubs/                     # SignalR Hubs
│   │   │   ├── MarketDataHub.cs     # 行情推送
│   │   │   ├── OrderHub.cs          # 订单推送
│   │   │   └── StrategyHub.cs       # 策略日志推送
│   │   ├── Middleware/               # 自定义中间件
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── RateLimitMiddleware.cs
│   │   ├── Configuration/            # 配置类
│   │   │   ├── DatabaseSettings.cs
│   │   │   ├── JwtSettings.cs
│   │   │   └── IbkrSettings.cs
│   │   ├── DTOs/                     # 数据传输对象
│   │   ├── Program.cs                # 应用入口点
│   │   ├── appsettings.json          # 配置文件
│   │   ├── appsettings.Development.json
│   │   └── Dockerfile
│   │
│   ├── WebUI.Core/                   # 核心业务逻辑库
│   │   ├── Services/                 # 业务服务
│   │   │   ├── Trading/
│   │   │   │   ├── ITradingService.cs
│   │   │   │   ├── TradingService.cs
│   │   │   │   └── OrderExecutionService.cs
│   │   │   ├── Strategy/
│   │   │   │   ├── IStrategyService.cs
│   │   │   │   ├── StrategyService.cs
│   │   │   │   └── StrategyExecutionService.cs
│   │   │   ├── Risk/
│   │   │   │   ├── IRiskService.cs
│   │   │   │   ├── RiskService.cs
│   │   │   │   └── PositionLimitService.cs
│   │   │   └── MarketData/
│   │   │       ├── IMarketDataService.cs
│   │   │       └── MarketDataService.cs
│   │   ├── Models/                   # 业务模型
│   │   │   ├── Order.cs
│   │   │   ├── Position.cs
│   │   │   ├── Strategy.cs
│   │   │   └── ...
│   │   ├── Interfaces/               # 服务接口
│   │   ├── Exceptions/               # 自定义异常
│   │   │   ├── BusinessException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   └── Helpers/                  # 工具类
│   │
│   ├── WebUI.Data/                   # 数据访问层
│   │   ├── Context/                  # DbContext
│   │   │   └── LeanDbContext.cs
│   │   ├── Entities/                 # 实体类 (EF Core)
│   │   │   ├── UserEntity.cs
│   │   │   ├── AccountEntity.cs
│   │   │   ├── OrderEntity.cs
│   │   │   ├── PositionEntity.cs
│   │   │   └── ...
│   │   ├── Repositories/             # 仓储接口和实现
│   │   │   ├── IRepository.cs
│   │   │   ├── Repository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   ├── OrderRepository.cs
│   │   │   └── ...
│   │   ├── Migrations/               # EF Core 数据库迁移
│   │   ├── Configurations/           # 实体配置 (Fluent API)
│   │   │   ├── OrderConfiguration.cs
│   │   │   └── ...
│   │   └── UnitOfWork/
│   │       ├── IUnitOfWork.cs
│   │       └── UnitOfWork.cs
│   │
│   ├── WebUI.Frontend/               # React 前端项目
│   │   ├── public/                   # 静态资源
│   │   │   ├── index.html
│   │   │   └── favicon.ico
│   │   ├── src/
│   │   │   ├── components/           # UI 组件
│   │   │   │   ├── Common/           # 通用组件
│   │   │   │   ├── Trading/          # 交易组件
│   │   │   │   ├── Strategy/         # 策略组件
│   │   │   │   └── Charts/           # 图表组件
│   │   │   ├── pages/                # 页面组件
│   │   │   │   ├── Dashboard.tsx
│   │   │   │   ├── Trading.tsx
│   │   │   │   ├── Positions.tsx
│   │   │   │   ├── Orders.tsx
│   │   │   │   ├── Strategies.tsx
│   │   │   │   └── Backtest.tsx
│   │   │   ├── services/             # API 服务层
│   │   │   │   ├── api.ts            # Axios 配置
│   │   │   │   ├── authService.ts
│   │   │   │   ├── tradingService.ts
│   │   │   │   ├── strategyService.ts
│   │   │   │   └── signalRService.ts
│   │   │   ├── stores/               # Zustand 状态管理
│   │   │   │   ├── useAuthStore.ts
│   │   │   │   ├── useTradingStore.ts
│   │   │   │   └── useMarketDataStore.ts
│   │   │   ├── hooks/                # 自定义 React Hooks
│   │   │   │   ├── useWebSocket.ts
│   │   │   │   ├── useMarketData.ts
│   │   │   │   └── useOrderStatus.ts
│   │   │   ├── utils/                # 工具函数
│   │   │   │   ├── formatters.ts
│   │   │   │   ├── validators.ts
│   │   │   │   └── constants.ts
│   │   │   ├── types/                # TypeScript 类型定义
│   │   │   │   ├── trading.ts
│   │   │   │   ├── strategy.ts
│   │   │   │   └── user.ts
│   │   │   ├── App.tsx               # 应用根组件
│   │   │   ├── main.tsx              # 应用入口
│   │   │   └── routes.tsx            # 路由配置
│   │   ├── .env                      # 环境变量
│   │   ├── .env.development
│   │   ├── .env.production
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   ├── vite.config.ts
│   │   └── Dockerfile
│   │
│   ├── WebUI.Tests/                  # 测试项目
│   │   ├── Unit/                     # 单元测试
│   │   │   ├── Services/
│   │   │   └── Controllers/
│   │   ├── Integration/              # 集成测试
│   │   │   └── Api/
│   │   └── E2E/                      # 端到端测试
│   │       └── Scenarios/
│   │
│   ├── docker-compose.yml            # Docker Compose 配置
│   ├── docker-compose.dev.yml        # 开发环境配置
│   └── docker-compose.prod.yml       # 生产环境配置
│
└── docs/                              # 文档目录
    ├── WebUI用户指南.md
    ├── WebUI开发指南.md
    ├── WebUI-User-Guide.md
    ├── WebUI-Developer-Guide.md
    └── WebUI-API-Reference.md
```

---

## 4. 开发环境设置

### 4.1 前置条件

#### 系统要求

- **操作系统**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 11+
- **CPU**: 2核心及以上 (推荐 4核心)
- **内存**: 8GB 及以上 (推荐 16GB)
- **存储**: 20GB 可用空间 (SSD 推荐)

#### 软件要求

1. **.NET 8 SDK**
   - Windows: 从[官网](https://dotnet.microsoft.com/download)下载安装
   - Linux: `sudo apt install dotnet-sdk-8.0`
   - macOS: `brew install dotnet-sdk`

2. **Node.js 18+ 和 npm**
   - Windows: 从[官网](https://nodejs.org/)下载安装
   - Linux: `curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - && sudo apt install -y nodejs`
   - macOS: `brew install node`

3. **Git**
   - Windows: 从[官网](https://git-scm.com/download/win)下载安装
   - Linux: `sudo apt install git`
   - macOS: `brew install git`

4. **PostgreSQL 15+** (可选,开发环境可用 SQLite)
   - Windows: 从[官网](https://www.postgresql.org/download/windows/)下载安装
   - Linux: `sudo apt install postgresql postgresql-contrib`
   - macOS: `brew install postgresql@15`

5. **Docker Desktop** (可选,用于容器化部署)
   - 从[官网](https://www.docker.com/products/docker-desktop)下载安装

### 4.2 克隆仓库

```bash
# 克隆 Lean 主仓库
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI

# 或者如果你 fork 了仓库
git clone https://github.com/your-username/Lean.git
cd Lean/WebUI
```

### 4.3 配置数据库

#### 选项 A: 使用 SQLite (推荐用于开发)

无需额外配置,程序会自动创建 `lean.db` 文件。

```json
// WebUI.API/appsettings.Development.json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

#### 选项 B: 使用 PostgreSQL (推荐用于生产)

1. **创建数据库和用户**:

```bash
# Linux/macOS
sudo -u postgres psql

# Windows (以管理员身份运行 PowerShell)
psql -U postgres
```

```sql
-- 在 psql 中执行
CREATE DATABASE leanui;
CREATE USER leanuser WITH PASSWORD 'dev_password_123';
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;

-- PostgreSQL 15+ 需要额外授权
\c leanui
GRANT ALL ON SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO leanuser;

-- 验证
\l  -- 列出所有数据库
\du -- 列出所有用户
\q  -- 退出
```

2. **配置连接字符串**:

```json
// WebUI.API/appsettings.Development.json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=dev_password_123"
  }
}
```

### 4.4 配置后端 API

1. **编辑配置文件** `WebUI.API/appsettings.Development.json`:

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  },
  "JWT": {
    "SecretKey": "dev_secret_key_for_development_environment_min_32_chars",
    "Issuer": "LeanWebUI",
    "Audience": "LeanWebUIClient",
    "AccessTokenExpirationMinutes": 120,
    "RefreshTokenExpirationDays": 30
  },
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    },
    "Account": {
      "AccountId": ""
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

2. **安装依赖包**:

```bash
cd WebUI.API
dotnet restore
```

3. **运行数据库迁移**:

```bash
# 确保已安装 EF Core 工具
dotnet tool install --global dotnet-ef

# 应用迁移 (创建数据库表结构)
dotnet ef database update --project ../WebUI.Data --startup-project .

# 查看迁移历史
dotnet ef migrations list --project ../WebUI.Data
```

4. **启动后端**:

```bash
dotnet run
```

API 启动后,访问:
- **API 端点**: http://localhost:5000
- **HTTPS 端点**: https://localhost:5001
- **Swagger 文档**: http://localhost:5000/swagger

### 4.5 配置前端

1. **安装依赖**:

```bash
cd WebUI.Frontend
npm install
```

2. **配置环境变量**:

创建 `.env.development` 文件:

```env
# API 配置
VITE_API_URL=http://localhost:5000/api
VITE_SIGNALR_URL=http://localhost:5000/hubs

# 其他配置
VITE_APP_TITLE=Lean WebUI
VITE_APP_VERSION=1.0.0
```

创建 `.env.production` 文件:

```env
VITE_API_URL=https://yourdomain.com/api
VITE_SIGNALR_URL=https://yourdomain.com/hubs
VITE_APP_TITLE=Lean WebUI
VITE_APP_VERSION=1.0.0
```

3. **启动前端开发服务器**:

```bash
npm run dev
```

前端启动后,浏览器自动打开: http://localhost:5173

### 4.6 验证安装

#### 后端验证

```bash
# 测试 API 健康检查
curl http://localhost:5000/health

# 预期输出: {"status":"Healthy"}
```

#### 前端验证

1. 打开浏览器访问 http://localhost:5173
2. 应该看到登录页面
3. 使用默认凭证登录:
   - 用户名: `admin`
   - 密码: `ChangeMe123!`

#### 数据库验证

**SQLite**:
```bash
sqlite3 WebUI.API/lean.db
.tables  # 应该看到: Users, Accounts, Orders, Positions 等表
.quit
```

**PostgreSQL**:
```bash
psql -U leanuser -d leanui -c "\dt"
# 应该看到所有表列表
```

---

## 5. 架构设计

### 5.1 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                       浏览器 (Browser)                       │
│                    React + TypeScript                         │
│   ┌─────────────────────────────────────────────────────┐   │
│   │  Components  │  Pages  │  Stores  │  Services       │   │
│   └─────────────────────────────────────────────────────┘   │
└────────┬──────────────────────────────────────────┬──────────┘
         │ HTTPS/WSS                                │
         │ (Axios)                                  │ (SignalR)
    ┌────▼────────────────────┐           ┌────────▼──────────┐
    │   REST API              │           │   SignalR Hubs    │
    │  (Controllers)          │           │  (Real-time Push) │
    └────┬────────────────────┘           └────────┬──────────┘
         │                                          │
    ┌────▼──────────────────────────────────────────▼────────┐
    │              Business Logic Layer (Services)             │
    │  Trading │ Strategy │ Risk │ MarketData │ Backtest      │
    └────┬────────────────────┬───────────────────────────────┘
         │                    │
    ┌────▼────────────────┐  │  ┌────────────────────────────┐
    │  Data Access Layer  │  │  │  External Integrations     │
    │  (Repositories)     │  │  │                            │
    │  - UnitOfWork       │  │  │  ┌──────────────────────┐  │
    │  - EF Core          │  └──│─▶│  Lean Engine (IPC)   │  │
    └────┬────────────────┘     │  └──────────────────────┘  │
         │                      │  ┌──────────────────────┐  │
    ┌────▼────────────────┐     └─▶│  IBKR TWS/Gateway   │  │
    │   PostgreSQL        │        └──────────────────────┘  │
    │   or SQLite         │                                  │
    └─────────────────────┘        └──────────────────────────┘
```

### 5.2 分层架构详解

#### Presentation Layer (表示层)

**职责**: 
- 处理 HTTP 请求和响应
- 输入验证和参数绑定
- 认证和授权检查
- 响应格式化 (JSON)
- 实时推送 (SignalR)

**组件**:
- `Controllers`: RESTful API 端点
- `Hubs`: SignalR 实时通信
- `Middleware`: 自定义中间件 (异常处理、JWT 验证、日志)
- `DTOs`: 数据传输对象

**原则**:
- **薄控制器**: 仅负责路由和参数转换,不包含业务逻辑
- **DTO 转换**: 使用 DTOs 而非实体类进行数据传输
- **统一错误处理**: 通过 ExceptionMiddleware 统一处理异常
- **输入验证**: 使用 DataAnnotations 和 FluentValidation

#### Business Logic Layer (业务逻辑层)

**职责**:
- 核心业务逻辑实现
- 业务规则验证
- 事务管理
- 跨服务协调

**组件**:
- `Services`: 业务服务接口和实现
- `Models`: 业务模型 (领域模型)
- `Validators`: 业务规则验证器
- `Mappers`: 对象映射 (Entity ↔ Model ↔ DTO)

**原则**:
- **接口优先**: 所有服务都定义接口 (便于测试和替换)
- **单一职责**: 每个服务专注于一个业务领域
- **事务管理**: 使用 UnitOfWork 模式管理事务边界
- **领域模型**: 业务逻辑封装在领域模型中

#### Data Access Layer (数据访问层)

**职责**:
- 数据持久化
- 数据查询和检索
- 数据库事务管理
- 数据库迁移

**组件**:
- `DbContext`: Entity Framework Core 上下文
- `Entities`: 数据库实体类 (映射到表)
- `Repositories`: 仓储接口和实现
- `Configurations`: Fluent API实体配置
- `UnitOfWork`: 工作单元模式实现

**原则**:
- **Repository 模式**: 抽象数据访问逻辑
- **UnitOfWork 模式**: 管理多个仓储的事务
- **延迟加载**: 谨慎使用,避免 N+1 查询问题
- **查询优化**: 使用 `AsNoTracking()` 只读查询,合理使用索引

### 5.3 设计模式

#### 依赖注入 (Dependency Injection)

所有服务通过构造函数注入依赖,便于单元测试和实现替换。

```csharp
// 注册服务 (Program.cs)
services.AddScoped<ITradingService, TradingService>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// 使用服务 (Controller)
public class TradingController : ControllerBase
{
    private readonly ITradingService _tradingService;
    private readonly ILogger<TradingController> _logger;
    
    public TradingController(
        ITradingService tradingService,
        ILogger<TradingController> logger)
    {
        _tradingService = tradingService;
        _logger = logger;
    }
    
    [HttpPost("orders")]
    public async Task<ActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _tradingService.PlaceOrderAsync(request);
        return Ok(order);
    }
}
```

#### Repository 模式

封装数据访问逻辑,提供统一的数据操作接口。

```csharp
// 接口定义
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}

// 通用实现
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}

// 特定仓储
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByAccountIdAsync(string accountId);
    Task<IEnumerable<Order>> GetPendingOrdersAsync();
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(LeanDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Order>> GetByAccountIdAsync(string accountId)
    {
        return await _dbSet
            .Where(o => o.AccountId == accountId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled)
            .ToListAsync();
    }
}
```

#### Unit of Work 模式

管理事务边界,确保数据一致性。

```csharp
// 接口定义
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPositionRepository Positions { get; }
    IAccountRepository Accounts { get; }
    IStrategyRepository Strategies { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// 实现
public class UnitOfWork : IUnitOfWork
{
    private readonly LeanDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public IOrderRepository Orders { get; }
    public IPositionRepository Positions { get; }
    public IAccountRepository Accounts { get; }
    public IStrategyRepository Strategies { get; }
    
    public UnitOfWork(
        LeanDbContext context,
        IOrderRepository orders,
        IPositionRepository positions,
        IAccountRepository accounts,
        IStrategyRepository strategies)
    {
        _context = context;
        Orders = orders;
        Positions = positions;
        Accounts = accounts;
        Strategies = strategies;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            await _transaction?.CommitAsync()!;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        await _transaction?.RollbackAsync()!;
        _transaction?.Dispose();
        _transaction = null;
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

// 使用示例
public class TradingService : ITradingService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // 创建订单
            var order = new Order { /* ... */ };
            await _unitOfWork.Orders.AddAsync(order);
            
            // 更新账户余额
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            account.AvailableCash -= order.TotalCost;
            _unitOfWork.Accounts.Update(account);
            
            // 提交事务
            await _unitOfWork.CommitTransactionAsync();
            
            return MapToDto(order);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## 6. 后端开发

### 6.1 创建新的 API 端点

#### 步骤 1: 定义 DTO

```csharp
// WebUI.API/DTOs/CreateStrategyRequest.cs
using System.ComponentModel.DataAnnotations;

namespace WebUI.API.DTOs;

public class CreateStrategyRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string AlgorithmCode { get; set; } = string.Empty;
    
    [Required]
    public StrategyType Type { get; set; }
    
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum StrategyType
{
    Intraday,
    Swing,
    ScalpTrading,
    Position
}
```

#### 步骤 2: 创建服务接口和实现

```csharp
// WebUI.Core/Interfaces/IStrategyService.cs
namespace WebUI.Core.Interfaces;

public interface IStrategyService
{
    Task<StrategyDto> CreateStrategyAsync(CreateStrategyRequest request, string userId);
    Task<StrategyDto> GetStrategyAsync(int strategyId);
    Task<IEnumerable<StrategyDto>> GetUserStrategiesAsync(string userId);
    Task StartStrategyAsync(int  strategyId);
    Task StopStrategyAsync(int strategyId);
    Task DeleteStrategyAsync(int strategyId);
}
```

```csharp
// WebUI.Core/Services/StrategyService.cs
using WebUI.Core.Interfaces;
using WebUI.Data.Entities;
using WebUI.Data.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace WebUI.Core.Services;

public class StrategyService : IStrategyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StrategyService> _logger;
    
    public StrategyService(
        IUnitOfWork unitOfWork,
        ILogger<StrategyService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<StrategyDto> CreateStrategyAsync(CreateStrategyRequest request, string userId)
    {
        _logger.LogInformation("Creating strategy {Name} for user {UserId}", 
            request.Name, userId);
        
        var strategy = new StrategyEntity
        {
            Name = request.Name,
            Description = request.Description,
            AlgorithmCode = request.AlgorithmCode,
            Type = request.Type,
            UserId = userId,
            Status = StrategyStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Strategies.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Strategy {StrategyId} created successfully", strategy.Id);
        
        return MapToDto(strategy);
    }
    
    public async Task<StrategyDto> GetStrategyAsync(int strategyId)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId);
        
        if (strategy == null)
            throw new NotFoundException($"Strategy {strategyId} not found");
        
        return MapToDto(strategy);
    }
    
    public async Task StartStrategyAsync(int strategyId)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId);
        
        if (strategy == null)
            throw new NotFoundException($"Strategy {strategyId} not found");
        
        if (strategy.Status == StrategyStatus.Running)
            throw new BusinessException("Strategy is already running");
        
        // 启动 Lean 引擎执行策略的逻辑
        // TODO: 调用 Lean Engine 启动策略
        
        strategy.Status = StrategyStatus.Running;
        strategy.StartedAt = DateTime.UtcNow;
        
        _unitOfWork.Strategies.Update(strategy);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Strategy {StrategyId} started", strategyId);
    }
    
    private static StrategyDto MapToDto(StrategyEntity entity)
    {
        return new StrategyDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            StartedAt = entity.StartedAt
        };
    }
}
```

#### 步骤 3: 创建控制器

```csharp
// WebUI.API/Controllers/StrategyController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Interfaces;
using WebUI.API.DTOs;
using System.Security.Claims;

namespace WebUI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StrategyController : ControllerBase
{
    private readonly IStrategyService _strategyService;
    private readonly ILogger<StrategyController> _logger;
    
    public StrategyController(
        IStrategyService strategyService,
        ILogger<StrategyController> logger)
    {
        _strategyService = strategyService;
        _logger = logger;
    }
    
    /// <summary>
    /// 创建新策略
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StrategyDto>> CreateStrategy(
        [FromBody] CreateStrategyRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException();
        
        var strategy = await _strategyService.CreateStrategyAsync(request, userId);
        
        return CreatedAtAction(
            nameof(GetStrategy), 
            new { id = strategy.Id }, 
            strategy);
    }
    
    /// <summary>
    /// 获取策略详情
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategyDto>> GetStrategy(int id)
    {
        var strategy = await _strategyService.GetStrategyAsync(id);
        return Ok(strategy);
    }
    
    /// <summary>
    /// 获取当前用户的所有策略
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StrategyDto>>> GetMyStrategies()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException();
        
        var strategies = await _strategyService.GetUserStrategiesAsync(userId);
        return Ok(strategies);
    }
    
    /// <summary>
    /// 启动策略
    /// </summary>
    [HttpPost("{id}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartStrategy(int id)
    {
        await _strategyService.StartStrategyAsync(id);
        return NoContent();
    }
    
    /// <summary>
    /// 停止策略
    /// </summary>
    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopStrategy(int id)
    {
        await _strategyService.StopStrategyAsync(id);
        return NoContent();
    }
    
    /// <summary>
    /// 删除策略
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStrategy(int id)
    {
        await _strategyService.DeleteStrategyAsync(id);
        return NoContent();
    }
}
```

#### 步骤 4: 注册服务

```csharp
// WebUI.API/Program.cs
builder.Services.AddScoped<IStrategyService, StrategyService>();
```

### 6.2 实现 SignalR 实时推送

```csharp
// WebUI.API/Hubs/StrategyHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.API.Hubs;

[Authorize]
public class StrategyHub : Hub
{
    private readonly ILogger<StrategyHub> _logger;
    
    public StrategyHub(ILogger<StrategyHub> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 订阅策略日志
    /// </summary>
    public async Task SubscribeToStrategy(int strategyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Strategy_{strategyId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to Strategy {StrategyId}", 
            Context.ConnectionId, strategyId);
    }
    
    /// <summary>
    /// 取消订阅策略日志
    /// </summary>
    public async Task UnsubscribeFromStrategy(int strategyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Strategy_{strategyId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from Strategy {StrategyId}", 
            Context.ConnectionId, strategyId);
    }
    
    /// <summary>
    /// 推送策略日志 (由后端服务调用)
    /// </summary>
    public async Task SendStrategyLog(int strategyId, string level, string message)
    {
        await Clients.Group($"Strategy_{strategyId}")
            .SendAsync("ReceiveStrategyLog", new
            {
                StrategyId = strategyId,
                Level = level,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
```

```csharp
// 在 Program.cs 中注册 SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 10;
});

// 映射 Hub 端点
app.MapHub<StrategyHub>("/hubs/strategy");
```

### 6.3 异常处理中间件

```csharp
// WebUI.API/Middleware/ExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using WebUI.Core.Exceptions;

namespace WebUI.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            NotFoundException notFoundEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = notFoundEx.Message
            },
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = validationEx.Message,
                Errors = validationEx.Errors
            },
            BusinessException businessEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = businessEx.Message
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Unauthorized access"
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = _env.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred"
            }
        };
        
        context.Response.StatusCode = response.StatusCode;
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}

// 在 Program.cs 中使用
app.UseMiddleware<ExceptionMiddleware>();
```

---

## 7. 前端开发

### 7.1 API 服务层

```typescript
// WebUI.Frontend/src/services/api.ts
import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../stores/useAuthStore';

class ApiClient {
    private client: AxiosInstance;
    
    constructor() {
        this.client = axios.create({
            baseURL: import.meta.env.VITE_API_URL,
            timeout: 30000,
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        // 请求拦截器 - 添加 JWT Token
        this.client.interceptors.request.use(
            (config: InternalAxiosRequestConfig) => {
                const token = useAuthStore.getState().token;
                if (token && config.headers) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error: AxiosError) => {
                return Promise.reject(error);
            }
        );
        
        // 响应拦截器 - 处理错误和 Token 刷新
        this.client.interceptors.response.use(
            (response) => response,
            async (error: AxiosError) => {
                const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
                
                // Token 过期,尝试刷新
                if (error.response?.status === 401 && !originalRequest._retry) {
                    originalRequest._retry = true;
                    
                    try {
                        const refreshToken = useAuthStore.getState().refreshToken;
                        const response = await this.client.post('/auth/refresh', { refreshToken });
                        
                        const { accessToken } = response.data;
                        useAuthStore.getState().setToken(accessToken);
                        
                        if (originalRequest.headers) {
                            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
                        }
                        
                        return this.client(originalRequest);
                    } catch (refreshError) {
                        // 刷新失败,跳转到登录页
                        useAuthStore.getState().logout();
                        window.location.href = '/login';
                        return Promise.reject(refreshError);
                    }
                }
                
                return Promise.reject(error);
            }
        );
    }
    
    public getClient(): AxiosInstance {
        return this.client;
    }
}

export const apiClient = new ApiClient().getClient();
```

```typescript
// WebUI.Frontend/src/services/strategyService.ts
import { apiClient } from './api';

export interface CreateStrategyRequest {
    name: string;
    description: string;
    algorithmCode: string;
    type: 'Intraday' | 'Swing' | 'ScalpTrading' | 'Position';
    parameters: Record<string, any>;
}

export interface StrategyDto {
    id: number;
    name: string;
    description: string;
    type: string;
    status: 'Inactive' | 'Running' | 'Stopped' | 'Error';
    createdAt: string;
    startedAt?: string;
}

export const strategyService = {
    // 创建策略
    async createStrategy(request: CreateStrategyRequest): Promise<StrategyDto> {
        const response = await apiClient.post<StrategyDto>('/strategy', request);
        return response.data;
    },
    
    // 获取所有策略
    async getStrategies(): Promise<StrategyDto[]> {
        const response = await apiClient.get<StrategyDto[]>('/strategy');
        return response.data;
    },
    
    // 获取策略详情
    async getStrategy(id: number): Promise<StrategyDto> {
        const response = await apiClient.get<StrategyDto>(`/strategy/${id}`);
        return response.data;
    },
    
    // 启动策略
    async startStrategy(id: number): Promise<void> {
        await apiClient.post(`/strategy/${id}/start`);
    },
    
    // 停止策略
    async stopStrategy(id: number): Promise<void> {
        await apiClient.post(`/strategy/${id}/stop`);
    },
    
    // 删除策略
    async deleteStrategy(id: number): Promise<void> {
        await apiClient.delete(`/strategy/${id}`);
    }
};
```

### 7.2 Zustand 状态管理

```typescript
// WebUI.Frontend/src/stores/useAuthStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authService } from '../services/authService';

interface AuthState {
    isAuthenticated: boolean;
    token: string | null;
    refreshToken: string | null;
    user: UserInfo | null;
    login: (username: string, password: string) => Promise<void>;
    logout: () => void;
    setToken: (token: string) => void;
}

interface UserInfo {
    id: string;
    username: string;
    email: string;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            isAuthenticated: false,
            token: null,
            refreshToken: null,
            user: null,
            
            login: async (username, password) => {
                const response = await authService.login(username, password);
                set({
                    isAuthenticated: true,
                    token: response.accessToken,
                    refreshToken: response.refreshToken,
                    user: response.user
                });
            },
            
            logout: () => {
                set({
                    isAuthenticated: false,
                    token: null,
                    refreshToken: null,
                    user: null
                });
            },
            
            setToken: (token) => {
                set({ token, isAuthenticated: true });
            }
        }),
        {
            name: 'auth-storage',
            partialize: (state) => ({
                token: state.token,
                refreshToken: state.refreshToken,
                user: state.user
            })
        }
    )
);
```

```typescript
// WebUI.Frontend/src/stores/useStrategyStore.ts
import { create } from 'zustand';
import { strategyService, StrategyDto } from '../services/strategyService';

interface StrategyState {
    strategies: StrategyDto[];
    selectedStrategy: StrategyDto | null;
    isLoading: boolean;
    error: string | null;
    
    fetchStrategies: () => Promise<void>;
    selectStrategy: (id: number) => Promise<void>;
    startStrategy: (id: number) => Promise<void>;
    stopStrategy: (id: number) => Promise<void>;
    deleteStrategy: (id: number) => Promise<void>;
}

export const useStrategyStore = create<StrategyState>((set, get) => ({
    strategies: [],
    selectedStrategy: null,
    isLoading: false,
    error: null,
    
    fetchStrategies: async () => {
        set({ isLoading: true, error: null });
        try {
            const strategies = await strategyService.getStrategies();
            set({ strategies, isLoading: false });
        } catch (error) {
            set({ 
                error: error instanceof Error ? error.message : 'Failed to fetch strategies',
                isLoading: false 
            });
        }
    },
    
    selectStrategy: async (id) => {
        set({ isLoading: true, error: null });
        try {
            const strategy = await strategyService.getStrategy(id);
            set({ selectedStrategy: strategy, isLoading: false });
        } catch (error) {
            set({ 
                error: error instanceof Error ? error.message : 'Failed to fetch strategy',
                isLoading: false 
            });
        }
    },
    
    startStrategy: async (id) => {
        try {
            await strategyService.startStrategy(id);
            await get().fetchStrategies(); // 刷新列表
        } catch (error) {
            set({ error: error instanceof Error ? error.message : 'Failed to start strategy' });
        }
    },
    
    stopStrategy: async (id) => {
        try {
            await strategyService.stopStrategy(id);
            await get().fetchStrategies();
        } catch (error) {
            set({ error: error instanceof Error ? error.message : 'Failed to stop strategy' });
        }
    },
    
    deleteStrategy: async (id) => {
        try {
            await strategyService.deleteStrategy(id);
            await get().fetchStrategies();
        } catch (error) {
            set({ error: error instanceof Error ? error.message : 'Failed to delete strategy' });
        }
    }
}));
```

### 7.3 SignalR 集成

```typescript
// WebUI.Frontend/src/services/signalRService.ts
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../stores/useAuthStore';

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private subscriptions: Map<string, Set<Function>> = new Map();
    
    async connect(): Promise<void> {
        const token = useAuthStore.getState().token;
        
        if (!token) {
            throw new Error('No authentication token available');
        }
        
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_SIGNALR_URL}/strategy`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    // 渐进式重连: 0s, 2s, 10s, 30s
                    if (retryContext.previousRetryCount === 0) return 0;
                    if (retryContext.previousRetryCount < 3) return 2000;
                    if (retryContext.previousRetryCount < 7) return 10000;
                    return 30000;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        this.connection.onreconnecting((error) => {
            console.warn('SignalR reconnecting...', error);
        });
        
        this.connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected:', connectionId);
        });
        
        this.connection.onclose((error) => {
            console.error('SignalR connection closed:', error);
        });
        
        await this.connection.start();
        console.log('SignalR connected');
    }
    
    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }
    
    async subscribeToStrategy(strategyId: number, callback: (data: any) => void): Promise<void> {
        if (!this.connection) {
            throw new Error('SignalR connection not established');
        }
        
        // 订阅后端推送
        await this.connection.invoke('SubscribeToStrategy', strategyId);
        
        // 注册本地回调
        const eventName = 'ReceiveStrategyLog';
        if (!this.subscriptions.has(eventName)) {
            this.subscriptions.set(eventName, new Set());
            
            this.connection.on(eventName, (data) => {
                const callbacks = this.subscriptions.get(eventName);
                if (callbacks) {
                    callbacks.forEach(cb => cb(data));
                }
            });
        }
        
        this.subscriptions.get(eventName)!.add(callback);
    }
    
    async unsubscribeFromStrategy(strategyId: number, callback: Function): Promise<void> {
        if (!this.connection) return;
        
        await this.connection.invoke('UnsubscribeFromStrategy', strategyId);
        
        const eventName = 'ReceiveStrategyLog';
        const callbacks = this.subscriptions.get(eventName);
        if (callbacks) {
            callbacks.delete(callback);
        }
    }
}

export const signalRService = new SignalRService();
```

### 7.4 React 组件示例

```typescript
// WebUI.Frontend/src/components/Strategy/StrategyList.tsx
import React, { useEffect } from 'react';
import { Table, Button, Space, Tag, message } from 'antd';
import { PlayCircleOutlined, PauseCircleOutlined, DeleteOutlined } from '@ant-design/icons';
import { useStrategyStore } from '../../stores/useStrategyStore';
import { StrategyDto } from '../../services/strategyService';
import type { ColumnsType } from 'antd/es/table';

const StrategyList: React.FC = () => {
    const { strategies, isLoading, error, fetchStrategies, startStrategy, stopStrategy, deleteStrategy } = useStrategyStore();
    
    useEffect(() => {
        fetchStrategies();
    }, []);
    
    useEffect(() => {
        if (error) {
            message.error(error);
        }
    }, [error]);
    
    const handleStart = async (id: number) => {
        try {
            await startStrategy(id);
            message.success('Strategy started successfully');
        } catch (error) {
            message.error('Failed to start strategy');
        }
    };
    
    const handleStop = async (id: number) => {
        try {
            await stopStrategy(id);
            message.success('Strategy stopped successfully');
        } catch (error) {
            message.error('Failed to stop strategy');
        }
    };
    
    const handleDelete = async (id: number) => {
        try {
            await deleteStrategy(id);
            message.success('Strategy deleted successfully');
        } catch (error) {
            message.error('Failed to delete strategy');
        }
    };
    
    const columns: ColumnsType<StrategyDto> = [
        {
            title: 'Name',
            dataIndex: 'name',
            key: 'name',
            width: 200
        },
        {
            title: 'Type',
            dataIndex: 'type',
            key: 'type',
            width: 120
        },
        {
            title: 'Status',
            dataIndex: 'status',
            key: 'status',
            width: 100,
            render: (status: string) => {
                const colorMap: Record<string, string> = {
                    Inactive: 'default',
                    Running: 'success',
                    Stopped: 'warning',
                    Error: 'error'
                };
                return <Tag color={colorMap[status] || 'default'}>{status}</Tag>;
            }
        },
        {
            title: 'Created At',
            dataIndex: 'createdAt',
            key: 'createdAt',
            width: 180,
            render: (date: string) => new Date(date).toLocaleString()
        },
        {
            title: 'Actions',
            key: 'actions',
            width: 200,
            render: (_, record) => (
                <Space>
                    {record.status !== 'Running' ? (
                        <Button
                            type="primary"
                            size="small"
                            icon={<PlayCircleOutlined />}
                            onClick={() => handleStart(record.id)}
                        >
                            Start
                        </Button>
                    ) : (
                        <Button
                            size="small"
                            icon={<PauseCircleOutlined />}
                            onClick={() => handleStop(record.id)}
                        >
                            Stop
                        </Button>
                    )}
                    <Button
                        danger
                        size="small"
                        icon={<DeleteOutlined />}
                        onClick={() => handleDelete(record.id)}
                    >
                        Delete
                    </Button>
                </Space>
            )
        }
    ];
    
    return (
        <Table
            columns={columns}
            dataSource={strategies}
            loading={isLoading}
            rowKey="id"
            pagination={{ pageSize: 10 }}
        />
    );
};

export default StrategyList;
```

---

## 8. 数据库开发

### 8.1 定义实体

```csharp
// WebUI.Data/Entities/OrderEntity.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities;

[Table("Orders")]
public class OrderEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string BrokerId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    public OrderType Type { get; set; }
    
    [Required]
    public OrderDirection Direction { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LimitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? StopPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal FilledQuantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AverageFilledPrice { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; }
    
    [StringLength(200)]
    public string? StatusMessage { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? FilledAt { get; set; }
    
    // 外键
    [Required]
    [StringLength(50)]
    public string AccountId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(AccountId))]
    public virtual AccountEntity? Account { get; set; }
    
    public int? StrategyId { get; set; }
    
    [ForeignKey(nameof(StrategyId))]
    public virtual StrategyEntity? Strategy { get; set; }
}

public enum OrderType
{
    Market,
    Limit,
    StopMarket,
    StopLimit
}

public enum OrderDirection
{
    Buy,
    Sell
}

public enum OrderStatus
{
    Pending,
    Submitted,
    PartiallyFilled,
    Filled,
    Canceled,
    Rejected,
    Invalid
}
```

### 8.2 Fluent API 配置

```csharp
// WebUI.Data/Configurations/OrderConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebUI.Data.Entities;

namespace WebUI.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        // 表名
        builder.ToTable("Orders");
        
        // 主键
        builder.HasKey(o => o.Id);
        
        // 索引
        builder.HasIndex(o => o.OrderId).IsUnique();
        builder.HasIndex(o => o.BrokerId);
        builder.HasIndex(o => o.AccountId);
        builder.HasIndex(o => o.Symbol);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        
        // 字段配置
        builder.Property(o => o.OrderId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(o => o.Symbol)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(o => o.Quantity)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(o => o.LimitPrice)
            .HasPrecision(18, 2);
        
        builder.Property(o => o.StopPrice)
            .HasPrecision(18, 2);
        
        // 关系配置
        builder.HasOne(o => o.Account)
            .WithMany(a => a.Orders)
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(o => o.Strategy)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StrategyId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // 默认值
        builder.Property(o => o.Status)
            .HasDefaultValue(OrderStatus.Pending);
        
        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
```

### 8.3 DbContext 配置

```csharp
// WebUI.Data/Context/LeanDbContext.cs
using Microsoft.EntityFrameworkCore;
using WebUI.Data.Entities;
using WebUI.Data.Configurations;

namespace WebUI.Data.Context;

public class LeanDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<PositionEntity> Positions => Set<PositionEntity>();
    public DbSet<StrategyEntity> Strategies => Set<StrategyEntity>();
    
    public LeanDbContext(DbContextOptions<LeanDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 应用所有配置
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new StrategyConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 自动设置 UpdatedAt
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.Entity is ITimestampedEntity timestamped)
            {
                timestamped.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}

public interface ITimestampedEntity
{
    DateTime? UpdatedAt { get; set; }
}
```

### 8.4 数据库迁移

```bash
# 初始迁移 (首次创建数据库结构)
cd WebUI.API
dotnet ef migrations add InitialCreate --project ../WebUI.Data --startup-project . --output-dir Migrations

# 应用迁移到数据库
dotnet ef database update --project ../WebUI.Data --startup-project .

# 添加新的迁移 (在修改实体后)
dotnet ef migrations add AddStrategyTable --project ../WebUI.Data --startup-project .

# 回滚迁移
dotnet ef migrations remove --project ../WebUI.Data --startup-project .

# 查看迁移历史
dotnet ef migrations list --project ../WebUI.Data --startup-project .

# 生成SQL脚本 (不直接执行)
dotnet ef migrations script --project ../WebUI.Data --startup-project . --output migrate.sql
```

---

## 9. 测试指南

### 9.1 单元测试 (xUnit)

```csharp
// WebUI.Tests/Unit/Services/StrategyServiceTests.cs
using Xunit;
using Moq;
using WebUI.Core.Services;
using WebUI.Data.Entities;
using WebUI.Data.UnitOfWork;
using WebUI.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace WebUI.Tests.Unit.Services;

public class StrategyServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly Mock<ILogger<StrategyService>> _loggerMock;
    private readonly StrategyService _service;
    
    public StrategyServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _loggerMock = new Mock<ILogger<StrategyService>>();
        
        _unitOfWorkMock.Setup(u => u.Strategies).Returns(_strategyRepoMock.Object);
        
        _service = new StrategyService(_unitOfWorkMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateStrategyAsync_ValidRequest_ReturnsStrategyDto()
    {
        // Arrange
        var request = new CreateStrategyRequest
        {
            Name = "Test Strategy",
            Description = "Test Description",
            AlgorithmCode = "class Algorithm...",
            Type = StrategyType.Intraday
        };
        
        var userId = "user123";
        
        _strategyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StrategyEntity>()))
            .Returns(Task.CompletedTask);
        
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        // Act
        var result = await _service.CreateStrategyAsync(request, userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        
        _strategyRepoMock.Verify(r => r.AddAsync(It.IsAny<StrategyEntity>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
    
    [Fact]
    public async Task StartStrategyAsync_StrategyNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var strategyId = 999;
        
        _strategyRepoMock
            .Setup(r => r.GetByIdAsync(strategyId))
            .ReturnsAsync((StrategyEntity?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.StartStrategyAsync(strategyId));
    }
    
    [Fact]
    public async Task StartStrategyAsync_AlreadyRunning_ThrowsBusinessException()
    {
        // Arrange
        var strategy = new StrategyEntity
        {
            Id = 1,
            Name = "Test",
            Status = StrategyStatus.Running
        };
        
        _strategyRepoMock
            .Setup(r => r.GetByIdAsync(strategy.Id))
            .ReturnsAsync(strategy);
        
        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => 
            _service.StartStrategyAsync(strategy.Id));
    }
}
```

### 9.2 集成测试

```csharp
// WebUI.Tests/Integration/Api/StrategyControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebUI.Tests.Integration.Api;

public class StrategyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public StrategyControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetStrategies_Unauthorized_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/strategy");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateStrategy_ValidRequest_Returns201()
    {
        // Arrange
        var loginRequest = new { Username = "testuser", Password = "Test123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        
        var createRequest = new
        {
            Name = "Test Strategy",
            Description = "Integration test",
            AlgorithmCode = "class TestAlgo...",
            Type = "Intraday"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/strategy", createRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<StrategyDto>();
        Assert.NotNull(result);
        Assert.Equal(createRequest.Name, result.Name);
    }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

### 9.3 前端测试 (Vitest + React Testing Library)

```typescript
// WebUI.Frontend/src/components/__tests__/StrategyList.test.tsx
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import StrategyList from '../Strategy/StrategyList';
import { useStrategyStore } from '../../stores/useStrategyStore';

// Mock store
vi.mock('../../stores/useStrategyStore');

describe('StrategyList', () => {
    const mockFetchStrategies = vi.fn();
    const mockStartStrategy = vi.fn();
    const mockStopStrategy = vi.fn();
    
    beforeEach(() => {
        vi.clearAllMocks();
        
        (useStrategyStore as any).mockReturnValue({
            strategies: [
                {
                    id: 1,
                    name: 'Test Strategy 1',
                    type: 'Intraday',
                    status: 'Inactive',
                    createdAt: '2024-01-01T00:00:00Z'
                },
                {
                    id: 2,
                    name: 'Test Strategy 2',
                    type: 'Swing',
                    status: 'Running',
                    createdAt: '2024-01-02T00:00:00Z'
                }
            ],
            isLoading: false,
            error: null,
            fetchStrategies: mockFetchStrategies,
            startStrategy: mockStartStrategy,
            stopStrategy: mockStopStrategy
        });
    });
    
    it('renders strategies table', async () => {
        render(<StrategyList />);
        
        await waitFor(() => {
            expect(screen.getByText('Test Strategy 1')).toBeInTheDocument();
            expect(screen.getByText('Test Strategy 2')).toBeInTheDocument();
        });
    });
    
    it('calls startStrategy when Start button clicked', async () => {
        const user = userEvent.setup();
        render(<StrategyList />);
        
        const startButton = await screen.findByRole('button', { name: /start/i });
        await user.click(startButton);
        
        expect(mockStartStrategy).toHaveBeenCalledWith(1);
    });
    
    it('displays error message when fetch fails', async () => {
        (useStrategyStore as any).mockReturnValue({
            strategies: [],
            isLoading: false,
            error: 'Failed to fetch strategies',
            fetchStrategies: mockFetchStrategies
        });
        
        render(<StrategyList />);
        
        // 验证 Ant Design message.error 被调用
        // 实际实现可能需要 mock Ant Design 的 message 组件
    });
});
```

---

## 10. 代码规范

### 10.1 C# 代码规范

#### 命名约定

- **类名**: PascalCase, 例如 `TradingService`, `OrderController`
- **接口名**: 以 `I` 开头, 例如 `ITradingService`, `IOrderRepository`
- **方法名**: PascalCase, 例如 `PlaceOrderAsync`, `GetAccountBalance`
- **私有字段**: camelCase 以 `_` 开头, 例如 `_unitOfWork`, `_logger`
- **参数和局部变量**: camelCase, 例如 `orderId`, `accountBalance`
- **常量**: UPPER_CASE, 例如 `MAX_ORDER_SIZE`, `DEFAULT_TIMEOUT`
- **异步方法**: 以 `Async` 结尾, 例如 `PlaceOrderAsync`

#### 代码风格

```csharp
// ✅ 好的实践
public class TradingService : ITradingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TradingService> _logger;
    
    public TradingService(
        IUnitOfWork unitOfWork,
        ILogger<TradingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        _logger.LogInformation("Placing order for symbol {Symbol}", request.Symbol);
        
        // 验证输入
        if (request.Quantity <= 0)
        {
            throw new ValidationException("Quantity must be positive");
        }
        
        // 业务逻辑
        var order = new OrderEntity
        {
            Symbol = request.Symbol,
            Quantity = request.Quantity,
            Type = request.Type,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Order {OrderId} placed successfully", order.Id);
        
        return MapToDto(order);
    }
}

// ❌ 不好的实践
public class TradingService
{
    private IUnitOfWork unitOfWork;  // 应该用 readonly 和 _
    
    public async Task<OrderDto> PlaceOrder(CreateOrderRequest request)  // 缺少 Async 后缀
    {
        // 没有参数验证
        var order = new OrderEntity();
        order.Symbol = request.Symbol;  // 应该用初始化器
        order.Quantity = request.Quantity;
        
        unitOfWork.Orders.Add(order);  // 缺少 await
        unitOfWork.SaveChanges();  // 缺少 await
        
        return MapToDto(order);  // 缺少异常处理和日志
    }
}
```

### 10.2 TypeScript 代码规范

#### 命名约定

- **类名和接口名**: PascalCase, 例如 `TradingService`, `StrategyDto`
- **函数和变量**: camelCase, 例如 `fetchStrategies`, `isLoading`
- **组件名**: PascalCase, 例如 `StrategyList`, `OrderForm`
- **常量**: UPPER_CASE, 例如 `API_TIMEOUT`, `MAX_RETRIES`
- **私有成员**: 以 `_` 开头 (可选), 例如 `_connection`

#### 代码风格

```typescript
// ✅ 好的实践
export interface CreateOrderRequest {
    symbol: string;
    quantity: number;
    type: OrderType;
    limitPrice?: number;
}

export class TradingService {
    private readonly apiClient: AxiosInstance;
    
    constructor(apiClient: AxiosInstance) {
        this.apiClient = apiClient;
    }
    
    async placeOrder(request: CreateOrderRequest): Promise<OrderDto> {
        try {
            const response = await this.apiClient.post<OrderDto>('/trading/orders', request);
            return response.data;
        } catch (error) {
            if (axios.isAxiosError(error)) {
                throw new Error(`Failed to place order: ${error.message}`);
            }
            throw error;
        }
    }
}

// React 组件
export const OrderForm: React.FC = () => {
    const [form] = Form.useForm();
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    const handleSubmit = async (values: CreateOrderRequest) => {
        setIsSubmitting(true);
        
        try {
            await tradingService.placeOrder(values);
            message.success('Order placed successfully');
            form.resetFields();
        } catch (error) {
            message.error(error instanceof Error ? error.message : 'Unknown error');
        } finally {
            setIsSubmitting(false);
        }
    };
    
    return (
        <Form form={form} onFinish={handleSubmit}>
            {/* Form fields */}
        </Form>
    );
};

// ❌ 不好的实践
function placeorder(symbol, quantity) {  // 应该用 camelCase 和类型注解
    apiClient.post('/orders', { symbol, quantity })  // 缺少 await 和错误处理
        .then(res => res.data);  // 没有返回 Promise
}

const orderform = () => {  // 组件名应该 PascalCase
    const [submitting, setsubmitting] = useState(false);  // 状态名应该描述清楚
    
    const submit = (values: any) => {  // 避免使用 any
        setsubmitting(true);
        placeorder(values.symbol, values.quantity);  // 缺少 await 和错误处理
        setsubmitting(false);  // 没有在 finally 中
    };
    
    return <div>...</div>;
};
```

---

## 11. 调试技巧

### 11.1 后端调试 (Visual Studio / VS Code)

#### Visual Studio 2022

1. **设置断点**: 在代码行号左侧点击或按 `F9`
2. **启动调试**: 按 `F5` 或点击"开始调试"
3. **单步调试**:
   - `F10`: 单步跨越 (Step Over)
   - `F11`: 单步进入 (Step Into)
   - `Shift+F11`: 单步跳出 (Step Out)
4. **查看变量**: 鼠标悬停在变量上,或在"局部变量"、"监视"窗口查看
5. **即时窗口**: 按 `Ctrl+Alt+I` 打开,可执行代码表达式

#### VS Code

创建 `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebUI.API/bin/Debug/net8.0/WebUI.API.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebUI.API",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Views": "${workspaceFolder}/Views"
            }
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }
    ]
}
```

### 11.2 前端调试

#### Chrome DevTools

1. **Sources 面板**: 按 `Ctrl+Shift+I` → Sources 标签
2. **设置断点**: 在源码行号点击
3. **调试控制**:
   - `F8`: 继续执行
   - `F10`: 单步跨越
   - `F11`: 单步进入
   - `Shift+F11`: 单步跳出
4. **Console 面板**: 查看日志和执行表达式
5. **Network 面板**: 监控 API 请求和响应

#### VS Code 调试 React

创建 `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "type": "chrome",
            "request": "launch",
            "name": "Launch Chrome against localhost",
            "url": "http://localhost:5173",
            "webRoot": "${workspaceFolder}/WebUI.Frontend/src",
            "sourceMaps": true,
            "sourceMapPathOverrides": {
                "webpack:///src/*": "${webRoot}/*"
            }
        }
    ]
}
```

### 11.3 SignalR 调试

#### 后端日志

```csharp
// Program.cs - 启用详细日志
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
```

#### 前端日志

```typescript
// signalRService.ts - 启用详细日志
this.connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .configureLogging(signalR.LogLevel.Debug)  // 开发环境使用 Debug
    .build();

// 监听所有事件
this.connection.on('*', (methodName, ...args) => {
    console.log(`SignalR event: ${methodName}`, args);
});
```

### 11.4 数据库查询调试

#### Entity Framework Core 日志

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.EntityFrameworkCore.Query": "Information"
    }
  }
}
```

#### 查看生成的 SQL

```csharp
var query = _context.Orders
    .Where(o => o.Status == OrderStatus.Pending)
    .Include(o => o.Account);

// 获取 SQL (仅用于调试)
var sql = query.ToQueryString();
_logger.LogDebug("Generated SQL: {Sql}", sql);
```

---

## 12. 部署流程

### 12.1 开发环境 → 测试环境

```bash
# 1. 拉取最新代码
git pull origin develop

# 2. 构建后端
cd WebUI.API
dotnet publish -c Release -o ./publish

# 3. 构建前端
cd ../WebUI.Frontend
npm run build

# 4. 更新数据库
cd ../WebUI.API
dotnet ef database update --project ../WebUI.Data

# 5. 重启服务
sudo systemctl restart leanwebui-api
sudo systemctl restart nginx
```

### 12.2 测试环境 → 生产环境

```bash
# 1. 合并到 main 分支
git checkout main
git merge develop
git push origin main

# 2. 创建版本标签
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# 3. 使用 Docker 部署
docker-compose -f docker-compose.prod.yml build
docker-compose -f docker-compose.prod.yml up -d

# 4. 验证部署
curl https://yourdomain.com/health

# 5. 备份数据库
pg_dump -U leanuser leanui > backup_$(date +%Y%m%d_%H%M%S).sql
```

### 12.3 Git 工作流

#### 分支策略

- **main**: 生产环境代码,仅接受 release 分支合并
- **develop**: 开发主分支
- **feature/xxx**: 新功能开发分支 (从 develop 创建)
- **bugfix/xxx**: Bug 修复分支
- **hotfix/xxx**: 紧急修复分支 (从 main 创建)

#### Commit 规范

```bash
# 格式: <type>(<scope>): <subject>

# 类型 (type):
# - feat: 新功能
# - fix: Bug 修复
# - docs: 文档更新
# - style: 代码格式调整
# - refactor: 重构
# - test: 测试相关
# - chore: 构建/工具链更新

# 示例:
git commit -m "feat(trading): add stop-loss order support"
git commit -m "fix(auth): resolve JWT refresh token expiration issue"
git commit -m "docs(readme): update installation instructions"
```

---

## 13. 贡献指南

### 13.1 如何贡献

1. **Fork 仓库**: 在 GitHub 上 fork Lean 仓库到你的账号
2. **创建分支**: `git checkout -b feature/my-new-feature`
3. **编写代码**: 遵循本文档的代码规范
4. **编写测试**: 确保新功能有对应的单元测试
5. **提交更改**: `git commit -m "feat: add my new feature"`
6. **推送分支**: `git push origin feature/my-new-feature`
7. **创建 Pull Request**: 在 GitHub 上提交 PR

### 13.2 Pull Request 检查清单

- [ ] 代码遵循项目代码规范
- [ ] 添加了单元测试且测试通过
- [ ] 更新了相关文档
- [ ] Commit 消息符合规范
- [ ] 没有合并冲突
- [ ] CI/CD 构建通过

### 13.3 报告问题

在GitHub Issues 中report问题时,请提供:

1. **问题描述**: 清晰描述遇到的问题
2. **复现步骤**: 详细说明如何重现问题
3. **预期行为**: 你期望发生什么
4. **实际行为**: 实际发生了什么
5. **环境信息**: 操作系统、.NET版本、浏览器版本等
6. **日志输出**: 相关的错误日志或堆栈跟踪
7. **截图**: 如果适用,附上截图

### 13.4 功能请求

提交功能请求时,请说明:

1. **功能描述**: 你希望添加什么功能
2. **使用场景**: 这个功能解决什么问题
3. **建议实现**: (可选) 你认为如何实现
4. **其他方案**: (可选) 是否考虑过其他方案

---

## 附录

### A. 常见问题

**Q: 如何切换数据库提供程序 (PostgreSQL ↔ SQLite)?**

A: 编辑 `appsettings.json` 中的 `Database` 配置:

```json
{
  "Database": {
    "Provider": "SQLite",  // 或 "PostgreSQL"
    "ConnectionString": "..."
  }
}
```

**Q: SignalR 连接失败怎么办?**

A: 检查:
1. 后端是否正确配置了 CORS 和 SignalR 端点
2. 前端 SignalR URL 是否正确
3. JWT Token 是否有效
4. 查看浏览器 Console 和后端日志

**Q: 如何添加新的交易所支持?**

A: 需要实现 `IBrokerageAdapter` 接口,参考 `IbkrAdapter` 的实现。



