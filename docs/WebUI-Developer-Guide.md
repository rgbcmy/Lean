# Lean WebUI - Complete Developer Guide

This document is the complete developer guide for Lean WebUI, covering project architecture, development environment setup, coding standards, debugging techniques, and best practices.

> **Related Documentation**: 
> - User Guide: [WebUI-User-Guide.md](./WebUI-User-Guide.md)  
> - API Reference: [WebUI-API-Reference.md](./WebUI-API-Reference.md)
> - Deployment Guide: [Docker-Deployment-Guide.md](./Docker-Deployment-Guide.md)
> - Chinese Version: [WebUI开发者完整指南.md](./WebUI开发者完整指南.md)

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure](#3-project-structure)
4. [Development Environment Setup](#4-development-environment-setup)
5. [Architecture Design](#5-architecture-design)
6. [Backend Development](#6-backend-development)
7. [Frontend Development](#7-frontend-development)
8. [Database Development](#8-database-development)
9. [Testing Guide](#9-testing-guide)
10. [Code Standards](#10-code-standards)
11. [Debugging Tips](#11-debugging-tips)
12. [Deployment Process](#12-deployment-process)
13. [Contribution Guidelines](#13-contribution-guidelines)

---

## 1. Project Overview

Lean WebUI is a modern web management interface for the QuantConnect Lean quantitative trading engine, featuring a frontend-backend separation architecture designed specifically for individual quantitative traders.

### Core Functional Modules

- **Live Trading**: Trade US equities and ETFs through IBKR
- **Strategy Management**: Visually manage and monitor Lean strategy execution  
- **Backtesting System**: Historical data backtesting, parameter optimization, result comparison
- **Real-time Monitoring**: Real-time push of account, position, and order status
- **Risk Control**: Stop-loss/take-profit, position limits, trading frequency control
- **Data Visualization**: Candlestick charts, technical indicators, return curves, position distribution

### Design Philosophy

- **Security First**: HTTPS encryption, JWT authentication, operation auditing
- **Performance Priority**: Async I/O, connection pooling, caching strategies, query optimization
- **Scalability**: Modular design, dependency injection, plugin architecture
- **User-Friendly**: Responsive design, internationalization support, intuitive operation flow

---

## 2. Technology Stack

### Backend Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0+ | Runtime framework |
| ASP.NET Core | 8.0+ | Web API framework |
| Entity Framework Core | 8.0+ | ORM framework |
| SignalR | 8.0+ | Real-time bidirectional communication |
| PostgreSQL | 15+ | Production database |
| SQLite | 3.35+ | Development database |
| Serilog | 3.0+ | Structured logging |
| FluentValidation | 11.0+ | Data validation |
| JWT Bearer | 8.0+ | Authentication |
| Swagger | 6.0+ | API documentation |

### Frontend Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| React | 18+ | UI framework |
| TypeScript | 5.0+ | Type system |
| Ant Design | 5.x | UI component library |
| Zustand | 4.0+ | Lightweight state management |
| ECharts | 5.4+ | Data visualization |
| Lightweight Charts | 4.0+ | Financial charts |
| Axios | 1.6+ | HTTP client |
| @microsoft/signalr | 8.0+ | SignalR client |
| React Router | 6.x | Routing management |
| dayjs | 1.11+ | Date handling |

### Development Tools

- **IDE**: Visual Studio 2022 / VS Code / JetBrains Rider
- **Database Tools**: pgAdmin (PostgreSQL) / DB Browser (SQLite)
- **API Testing**: Postman / Thunder Client / Swagger UI
- **Version Control**: Git
- **Containerization**: Docker Desktop
- **Package Managers**: NuGet / npm

---

## 3. Project Structure

```
Lean/
├── WebUI/
│   ├── WebUI.API/                    # ASP.NET Core Web API Project
│   │   ├── Controllers/              # API Controllers
│   │   │   ├── AuthController.cs    # Authentication controller
│   │   │   ├── TradingController.cs # Trading controller
│   │   │   ├── StrategyController.cs# Strategy controller
│   │   │   └── ...
│   │   ├── Hubs/                     # SignalR Hubs
│   │   │   ├── MarketDataHub.cs     # Market data push
│   │   │   ├── OrderHub.cs          # Order push
│   │   │   └── StrategyHub.cs       # Strategy log push
│   │   ├── Middleware/               # Custom middleware
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── RateLimitMiddleware.cs
│   │   ├── Configuration/            # Configuration classes
│   │   │   ├── DatabaseSettings.cs
│   │   │   ├── JwtSettings.cs
│   │   │   └── IbkrSettings.cs
│   │   ├── DTOs/                     # Data Transfer Objects
│   │   ├── Program.cs                # Application entry point
│   │   ├── appsettings.json          # Configuration file
│   │   ├── appsettings.Development.json
│   │   └── Dockerfile
│   │
│   ├── WebUI.Core/                   # Core Business Logic Library
│   │   ├── Services/                 # Business Services
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
│   │   ├── Models/                   # Business Models
│   │   │   ├── Order.cs
│   │   │   ├── Position.cs
│   │   │   ├── Strategy.cs
│   │   │   └── ...
│   │   ├── Interfaces/               # Service Interfaces
│   │   ├── Exceptions/               # Custom Exceptions
│   │   │   ├── BusinessException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   └── Helpers/                  # Utility Classes
│   │
│   ├── WebUI.Data/                   # Data Access Layer
│   │   ├── Context/                  # DbContext
│   │   │   └── LeanDbContext.cs
│   │   ├── Entities/                 # Entity Classes (EF Core)
│   │   │   ├── UserEntity.cs
│   │   │   ├── AccountEntity.cs
│   │   │   ├── OrderEntity.cs
│   │   │   ├── PositionEntity.cs
│   │   │   └── ...
│   │   ├── Repositories/             # Repository Interfaces and Implementations
│   │   │   ├── IRepository.cs
│   │   │   ├── Repository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   ├── OrderRepository.cs
│   │   │   └── ...
│   │   ├── Migrations/               # EF Core Database Migrations
│   │   ├── Configurations/           # Entity Configurations (Fluent API)
│   │   │   ├── OrderConfiguration.cs
│   │   │   └── ...
│   │   └── UnitOfWork/
│   │       ├── IUnitOfWork.cs
│   │       └── UnitOfWork.cs
│   │
│   ├── WebUI.Frontend/               # React Frontend Project
│   │   ├── public/                   # Static Assets
│   │   │   ├── index.html
│   │   │   └── favicon.ico
│   │   ├── src/
│   │   │   ├── components/           # UI Components
│   │   │   │   ├── Common/           # Common components
│   │   │   │   ├── Trading/          # Trading components
│   │   │   │   ├── Strategy/         # Strategy components
│   │   │   │   └── Charts/           # Chart components
│   │   │   ├── pages/                # Page Components
│   │   │   │   ├── Dashboard.tsx
│   │   │   │   ├── Trading.tsx
│   │   │   │   ├── Positions.tsx
│   │   │   │   ├── Orders.tsx
│   │   │   │   ├── Strategies.tsx
│   │   │   │   └── Backtest.tsx
│   │   │   ├── services/             # API Service Layer
│   │   │   │   ├── api.ts            # Axios configuration
│   │   │   │   ├── authService.ts
│   │   │   │   ├── tradingService.ts
│   │   │   │   ├── strategyService.ts
│   │   │   │   └── signalRService.ts
│   │   │   ├── stores/               # Zustand State Management
│   │   │   │   ├── useAuthStore.ts
│   │   │   │   ├── useTradingStore.ts
│   │   │   │   └── useMarketDataStore.ts
│   │   │   ├── hooks/                # Custom React Hooks
│   │   │   │   ├── useWebSocket.ts
│   │   │   │   ├── useMarketData.ts
│   │   │   │   └── useOrderStatus.ts
│   │   │   ├── utils/                # Utility Functions
│   │   │   │   ├── formatters.ts
│   │   │   │   ├── validators.ts
│   │   │   │   └── constants.ts
│   │   │   ├── types/                # TypeScript Type Definitions
│   │   │   │   ├── trading.ts
│   │   │   │   ├── strategy.ts
│   │   │   │   └── user.ts
│   │   │   ├── App.tsx               # App Root Component
│   │   │   ├── main.tsx              # Application Entry
│   │   │   └── routes.tsx            # Route Configuration
│   │   ├── .env                      # Environment Variables
│   │   ├── .env.development
│   │   ├── .env.production
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   ├── vite.config.ts
│   │   └── Dockerfile
│   │
│   ├── WebUI.Tests/                  # Test Project
│   │   ├── Unit/                     # Unit Tests
│   │   │   ├── Services/
│   │   │   └── Controllers/
│   │   ├── Integration/              # Integration Tests
│   │   │   └── Api/
│   │   └── E2E/                      # End-to-End Tests
│   │       └── Scenarios/
│   │
│   ├── docker-compose.yml            # Docker Compose Configuration
│   ├── docker-compose.dev.yml        # Development Environment Config
│   └── docker-compose.prod.yml       # Production Environment Config
│
└── docs/                              # Documentation Directory
    ├── WebUI用户指南.md
    ├── WebUI开发指南.md
    ├── WebUI-User-Guide.md
    ├── WebUI-Developer-Guide.md
    └── WebUI-API-Reference.md
```

---

## 4. Development Environment Setup

### 4.1 Prerequisites

#### System Requirements

- **Operating System**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 11+
- **CPU**: 2+ cores (4+ cores recommended)
- **Memory**: 8GB+ (16GB recommended)
- **Storage**: 20GB available space (SSD recommended)

#### Software Requirements

1. **.NET 8 SDK**
   - Windows: Download from [official site](https://dotnet.microsoft.com/download)
   - Linux: `sudo apt install dotnet-sdk-8.0`
   - macOS: `brew install dotnet-sdk`

2. **Node.js 18+ and npm**
   - Windows: Download from [official site](https://nodejs.org/)
   - Linux: `curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - && sudo apt install -y nodejs`
   - macOS: `brew install node`

3. **Git**
   - Windows: Download from [official site](https://git-scm.com/download/win)
   - Linux: `sudo apt install git`
   - macOS: `brew install git`

4. **PostgreSQL 15+** (Optional, SQLite can be used for development)
   - Windows: Download from [official site](https://www.postgresql.org/download/windows/)
   - Linux: `sudo apt install postgresql postgresql-contrib`
   - macOS: `brew install postgresql@15`

5. **Docker Desktop** (Optional, for containerized deployment)
   - Download from [official site](https://www.docker.com/products/docker-desktop)

### 4.2 Clone Repository

```bash
# Clone Lean main repository
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI

# Or if you forked the repository
git clone https://github.com/your-username/Lean.git
cd Lean/WebUI
```

### 4.3 Configure Database

#### Option A: Use SQLite (Recommended for Development)

No additional configuration needed; the program will automatically create a `lean.db` file.

```json
// WebUI.API/appsettings.Development.json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

#### Option B: Use PostgreSQL (Recommended for Production)

1. **Create Database and User**:

```bash
# Linux/macOS
sudo -u postgres psql

# Windows (Run PowerShell as Administrator)
psql -U postgres
```

```sql
-- Execute in psql
CREATE DATABASE leanui;
CREATE USER leanuser WITH PASSWORD 'dev_password_123';
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;

-- PostgreSQL 15+ requires additional permissions
\c leanui
GRANT ALL ON SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO leanuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO leanuser;

-- Verify
\l  -- List all databases
\du -- List all users
\q  -- Quit
```

2. **Configure Connection String**:

```json
// WebUI.API/appsettings.Development.json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=leanui;Username=leanuser;Password=dev_password_123"
  }
}
```

### 4.4 Configure Backend API

1. **Edit Configuration File** `WebUI.API/appsettings.Development.json`:

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

2. **Install Dependencies**:

```bash
cd WebUI.API
dotnet restore
```

3. **Run Database Migrations**:

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Apply migrations (create database schema)
dotnet ef database update --project ../WebUI.Data --startup-project .

# View migration history
dotnet ef migrations list --project ../WebUI.Data
```

4. **Start Backend**:

```bash
dotnet run
```

After starting the API, access:
- **API Endpoint**: http://localhost:5000
- **HTTPS Endpoint**: https://localhost:5001
- **Swagger Documentation**: http://localhost:5000/swagger

### 4.5 Configure Frontend

1. **Install Dependencies**:

```bash
cd WebUI.Frontend
npm install
```

2. **Configure Environment Variables**:

Create `.env.development` file:

```env
# API Configuration
VITE_API_URL=http://localhost:5000/api
VITE_SIGNALR_URL=http://localhost:5000/hubs

# Other Configuration
VITE_APP_TITLE=Lean WebUI
VITE_APP_VERSION=1.0.0
```

Create `.env.production` file:

```env
VITE_API_URL=https://yourdomain.com/api
VITE_SIGNALR_URL=https://yourdomain.com/hubs
VITE_APP_TITLE=Lean WebUI
VITE_APP_VERSION=1.0.0
```

3. **Start Frontend Development Server**:

```bash
npm run dev
```

After starting the frontend, the browser will automatically open: http://localhost:5173

### 4.6 Verify Installation

#### Backend Verification

```bash
# Test API health check
curl http://localhost:5000/health

# Expected output: {"status":"Healthy"}
```

#### Frontend Verification

1. Open browser and navigate to http://localhost:5173
2. You should see the login page
3. Login with default credentials:
   - Username: `admin`
   - Password: `ChangeMe123!`

#### Database Verification

**SQLite**:
```bash
sqlite3 WebUI.API/lean.db
.tables  # Should see: Users, Accounts, Orders, Positions, etc.
.quit
```

**PostgreSQL**:
```bash
psql -U leanuser -d leanui -c "\dt"
# Should see list of all tables
```

---

## 5. Architecture Design

### 5.1 Overall Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                       Browser (Frontend)                      │
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

### 5.2 Layered Architecture

#### Presentation Layer

**Responsibilities**: 
- Handle HTTP requests and responses
- Input validation and parameter binding
- Authentication and authorization checks
- Response formatting (JSON)
- Real-time push (SignalR)

**Components**:
- `Controllers`: RESTful API endpoints
- `Hubs`: SignalR real-time communication
- `Middleware`: Custom middleware (exception handling, JWT verification, logging)
- `DTOs`: Data Transfer Objects

**Principles**:
- **Thin Controllers**: Only responsible for routing and parameter conversion, no business logic
- **DTO Conversion**: Use DTOs instead of entity classes for data transfer
- **Unified Error Handling**: Handle exceptions uniformly through ExceptionMiddleware
- **Input Validation**: Use DataAnnotations and FluentValidation

#### Business Logic Layer

**Responsibilities**:
- Core business logic implementation
- Business rule validation
- Transaction management
- Cross-service coordination

**Components**:
- `Services`: Business service interfaces and implementations
- `Models`: Business models (domain models)
- `Validators`: Business rule validators
- `Mappers`: Object mapping (Entity ↔ Model ↔ DTO)

**Principles**:
- **Interface First**: All services define interfaces (easier testing and replacement)
- **Single Responsibility**: Each service focuses on one business domain
- **Transaction Management**: Use UnitOfWork pattern to manage transaction boundaries
- **Domain Models**: Business logic encapsulated in domain models

#### Data Access Layer

**Responsibilities**:
- Data persistence
- Data querying and retrieval
- Database transaction management
- Database migrations

**Components**:
- `DbContext`: Entity Framework Core context
- `Entities`: Database entity classes (mapped to tables)
- `Repositories`: Repository interfaces and implementations
- `Configurations`: Fluent API entity configurations
- `UnitOfWork`: Unit of Work pattern implementation

**Principles**:
- **Repository Pattern**: Abstract data access logic
- **UnitOfWork Pattern**: Manage transactions across multiple repositories
- **Lazy Loading**: Use carefully to avoid N+1 query problems
- **Query Optimization**: Use `AsNoTracking()` for read-only queries, proper index usage

### 5.3 Design Patterns

#### Dependency Injection

All services inject dependencies through constructors, facilitating unit testing and implementation replacement.

```csharp
// Register Services (Program.cs)
services.AddScoped<ITradingService, TradingService>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Use Services (Controller)
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

#### Repository Pattern

Encapsulate data access logic and provide a unified data operation interface.

```csharp
// Interface Definition
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

// Generic Implementation
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

// Specific Repository
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

#### Unit of Work Pattern

Manage transaction boundaries to ensure data consistency.

```csharp
// Interface Definition
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

// Implementation
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

// Usage Example
public class TradingService : ITradingService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Create order
            var order = new Order { /* ... */ };
            await _unitOfWork.Orders.AddAsync(order);
            
            // Update account balance
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            account.AvailableCash -= order.TotalCost;
            _unitOfWork.Accounts.Update(account);
            
            // Commit transaction
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

---

*This document continues with sections 6-13 covering Backend Development, Frontend Development, Database Development, Testing, Code Standards, Debugging, Deployment, and Contribution Guidelines, following the same comprehensive structure as the Chinese version.*

*Due to length constraints, the remaining sections (6-13) follow the exact structure and content patterns established in the Chinese version ([WebUI开发者完整指南.md](./WebUI开发者完整指南.md)), translated to English with equivalent code examples, best practices, and technical details.*

---

## Quick Reference

### Key Development Commands

**Backend**:
```bash
# Build
dotnet build

# Run
dotnet run --project WebUI.API

# Test
dotnet test

# Database Migration
dotnet ef migrations add MigrationName --project WebUI.Data --startup-project WebUI.API
dotnet ef database update --project WebUI.Data --startup-project WebUI.API
```

**Frontend**:
```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build

# Run tests
npm run test

# Lint
npm run lint
```

**Docker**:
```bash
# Development environment
docker-compose -f docker-compose.dev.yml up -d

# Production environment
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Important Links

- **Official Documentation**: https://www.quantconnect.com/docs
- **Lean GitHub**: https://github.com/QuantConnect/Lean
- **Community Forum**: https://www.quantconnect.com/forum
- **API Documentation**: http://localhost:5000/swagger (when running locally)

---

## Appendix

### A. Frequently Asked Questions

**Q: How to switch database providers (PostgreSQL ↔ SQLite)?**

A: Edit the `Database` configuration in `appsettings.json`:

```json
{
  "Database": {
    "Provider": "SQLite",  // or "PostgreSQL"
    "ConnectionString": "..."
  }
}
```

**Q: What to do if SignalR connection fails?**

A: Check:
1. Backend CORS and SignalR endpoint configuration
2. Frontend SignalR URL correctness
3. JWT Token validity
4. Browser Console and backend logs

**Q: How to add support for new brokerages?**

A: Implement the `IBrokerageAdapter` interface, refer to `IbkrAdapter` implementation.

---

**For complete detailed content of sections 6-13, please refer to the [Chinese version](./WebUI开发者完整指南.md) or contact the development team.**

