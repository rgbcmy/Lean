# Lean WebUI - Architecture Diagrams and Flowcharts

This document contains architecture diagrams, data flow diagrams, and sequence diagrams for the Lean WebUI system.

> **Related Documentation**:
> - Developer Guide: [WebUI-Developer-Guide.md](./WebUI-Developer-Guide.md)
> - API Reference: [WebUI-API-Reference.md](./WebUI-API-Reference.md)
> - User Guide: [WebUI-User-Guide.md](./WebUI-User-Guide.md)

---

## Table of Contents

1. [System Architecture](#1-system-architecture)
2. [Component Diagrams](#2-component-diagrams)
3. [Data Flow Diagrams](#3-data-flow-diagrams)
4. [Sequence Diagrams](#4-sequence-diagrams)
5. [Deployment Diagrams](#5-deployment-diagrams)
6. [Database Schema](#6-database-schema)

---

## 1. System Architecture

### 1.1 High-Level Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        Mobile[Mobile Browser]
    end
    
    subgraph "Frontend Layer"
        React[React Application<br/>TypeScript + Ant Design]
        Router[React Router]
        State[Zustand State Management]
        Charts[ECharts + Lightweight Charts]
    end
    
    subgraph "API Gateway Layer"
        NGINX[NGINX Reverse Proxy<br/>SSL/TLS Termination]
    end
    
    subgraph "Backend Layer"
        API[ASP.NET Core Web API<br/>.NET 8]
        SignalR[SignalR Hubs<br/>Real-time Communication]
        Auth[JWT Authentication]
        Middleware[Custom Middleware<br/>Exception, Logging, Rate Limiting]
    end
    
    subgraph "Business Logic Layer"
        TradingService[Trading Service]
        StrategyService[Strategy Service]
        RiskService[Risk Control Service]
        MarketDataService[Market Data Service]
        BacktestService[Backtest Service]
    end
    
    subgraph "Data Access Layer"
        EFCore[Entity Framework Core]
        Repositories[Repositories]
        UnitOfWork[Unit of Work]
    end
    
    subgraph "External Services"
        IBKR[IBKR TWS/Gateway<br/>Interactive Brokers API]
        LeanEngine[Lean Engine<br/>Backtesting & Execution]
    end
    
    subgraph "Data Layer"
        PostgreSQL[(PostgreSQL<br/>Production)]
        SQLite[(SQLite<br/>Development)]
        Redis[(Redis<br/>Caching)]
    end
    
    Browser --> React
    Mobile --> React
    React --> Router
    React --> State
    React --> Charts
    React <--> NGINX
    
    NGINX <--> API
    NGINX <--> SignalR
    
    API --> Auth
    API --> Middleware
    API --> TradingService
    API --> StrategyService
    API --> RiskService
    API --> MarketDataService
    API --> BacktestService
    
    TradingService --> EFCore
    StrategyService --> EFCore
    RiskService --> EFCore
    MarketDataService --> EFCore
    BacktestService --> EFCore
    
    EFCore --> Repositories
    Repositories --> UnitOfWork
    
    UnitOfWork --> PostgreSQL
    UnitOfWork --> SQLite
    
    TradingService --> IBKR
    StrategyService --> LeanEngine
    BacktestService --> LeanEngine
    MarketDataService --> IBKR
    
    API --> Redis
    SignalR --> Redis
    
    style Browser fill:#e1f5ff
    style Mobile fill:#e1f5ff
    style React fill:#61dafb
    style API fill:#512bd4
    style PostgreSQL fill:#336791
    style Redis fill:#dc382d
    style IBKR fill:#ff6600
    style LeanEngine fill:#00cc88
```

### 1.2 Layered Architecture

```mermaid
graph TD
    subgraph "Presentation Layer"
        Controllers[Controllers<br/>RESTful API Endpoints]
        Hubs[SignalR Hubs<br/>Real-time Push]
        DTOs[Data Transfer Objects]
        Middleware2[Middleware<br/>Exception, Auth, Logging]
    end
    
    subgraph "Business Logic Layer"
        Services[Business Services<br/>Trading, Strategy, Risk]
        Validators[Business Validators]
        Mappers[Object Mappers<br/>Entity ↔ DTO]
        Models[Domain Models]
    end
    
    subgraph "Data Access Layer"
        Repos[Repositories<br/>Data Access Abstraction]
        UoW[Unit of Work<br/>Transaction Management]
        DbContext[EF Core DbContext]
        Entities[Database Entities]
    end
    
    subgraph "Infrastructure Layer"
        IBKR2[IBKR Adapter]
        LeanAdapter[Lean Engine Adapter]
        Cache[Redis Cache]
        Logger[Structured Logging<br/>Serilog]
    end
    
    subgraph "Persistence Layer"
        DB[(PostgreSQL / SQLite)]
    end
    
    Controllers --> Services
    Hubs --> Services
    Controllers --> DTOs
    Controllers --> Middleware2
    
    Services --> Validators
    Services --> Mappers
    Services --> Models
    Services --> Repos
    
    Repos --> UoW
    UoW --> DbContext
    DbContext --> Entities
    DbContext --> DB
    
    Services --> IBKR2
    Services --> LeanAdapter
    Services --> Cache
    Services --> Logger
    
    style Controllers fill:#4caf50
    style Services fill:#2196f3
    style Repos fill:#ff9800
    style DB fill:#9c27b0
    style IBKR2 fill:#f44336
```

---

## 2. Component Diagrams

### 2.1 Frontend Structure

```mermaid
graph TB
    subgraph "Pages"
        Dashboard[Dashboard Page]
        Trading[Trading Page]
        Strategies[Strategies Page]
        Positions[Positions Page]
        Orders[Orders Page]
        Backtest[Backtest Page]
    end
    
    subgraph "Components"
        Common[Common Components<br/>Button, Table, Modal]
        TradingComp[Trading Components<br/>OrderForm, OrderBook]
        StrategyComp[Strategy Components<br/>StrategyCard, StrategyForm]
        ChartComp[Chart Components<br/>CandlestickChart, LineChart]
    end
    
    subgraph "Services"
        AuthService[Auth Service]
        TradingServiceAPI[Trading API Service]
        StrategyServiceAPI[Strategy API Service]
        MarketDataServiceAPI[Market Data Service]
        SignalRService[SignalR Service]
    end
    
    subgraph "State Management"
        AuthStore[Auth Store]
        TradingStore[Trading Store]
        StrategyStore[Strategy Store]
        MarketDataStore[Market Data Store]
    end
    
    subgraph "Utilities"
        Formatters[Formatters<br/>Date, Currency, Number]
        Validators[Input Validators]
        Constants[Constants & Enums]
    end
    
    Dashboard --> Common
    Trading --> TradingComp
    Trading --> ChartComp
    Strategies --> StrategyComp
    
    TradingComp --> TradingServiceAPI
    StrategyComp --> StrategyServiceAPI
    ChartComp --> MarketDataServiceAPI
    
    TradingServiceAPI --> TradingStore
    StrategyServiceAPI --> StrategyStore
    SignalRService --> MarketDataStore
    
    TradingComp --> Formatters
    TradingComp --> Validators
    
    style Dashboard fill:#e3f2fd
    style TradingServiceAPI fill:#bbdefb
    style TradingStore fill:#90caf9
```

### 2.2 Backend Services

```mermaid
graph LR
    subgraph "Trading Module"
        TradingController[Trading Controller]
        TradingServiceImpl[Trading Service]
        OrderRepo[Order Repository]
        PositionRepo[Position Repository]
    end
    
    subgraph "Strategy Module"
        StrategyController[Strategy Controller]
        StrategyServiceImpl[Strategy Service]
        StrategyRepo[Strategy Repository]
        ExecutionService[Execution Service]
    end
    
    subgraph "Risk Module"
        RiskController[Risk Controller]
        RiskServiceImpl[Risk Service]
        RiskRuleEngine[Risk Rule Engine]
        PositionLimitService[Position Limit Service]
    end
    
    subgraph "Market Data Module"
        MarketDataHub[Market Data Hub]
        MarketDataServiceImpl[Market Data Service]
        QuoteCache[Quote Cache<br/>Redis]
    end
    
    subgraph "Authentication Module"
        AuthController[Auth Controller]
        AuthServiceImpl[Auth Service]
        JWTService[JWT Token Service]
        UserRepo[User Repository]
    end
    
    TradingController --> TradingServiceImpl
    TradingServiceImpl --> OrderRepo
    TradingServiceImpl --> PositionRepo
    TradingServiceImpl --> RiskServiceImpl
    
    StrategyController --> StrategyServiceImpl
    StrategyServiceImpl --> StrategyRepo
    StrategyServiceImpl --> ExecutionService
    
    RiskController --> RiskServiceImpl
    RiskServiceImpl --> RiskRuleEngine
    RiskServiceImpl --> PositionLimitService
    
    MarketDataHub --> MarketDataServiceImpl
    MarketDataServiceImpl --> QuoteCache
    
    AuthController --> AuthServiceImpl
    AuthServiceImpl --> JWTService
    AuthServiceImpl --> UserRepo
    
    style TradingController fill:#4caf50
    style StrategyController fill:#2196f3
    style RiskController fill:#ff9800
    style MarketDataHub fill:#9c27b0
    style AuthController fill:#f44336
```

---

## 3. Data Flow Diagrams

### 3.1 Order Placement Flow

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant TradingService
    participant RiskService
    participant OrderRepo
    participant IBKR
    participant Database
    participant SignalR
    
    User->>Frontend: Click "Place Order"
    Frontend->>Frontend: Validate Input
    Frontend->>API: POST /api/v1/trading/orders<br/>{symbol, quantity, price}
    
    API->>API: Validate JWT Token
    API->>API: Validate Request Body
    
    API->>TradingService: PlaceOrderAsync(request)
    
    TradingService->>RiskService: ValidateOrder(order)
    RiskService-->>TradingService: Risk Check Passed
    
    TradingService->>OrderRepo: CreateOrder(order)
    OrderRepo->>Database: INSERT INTO Orders
    Database-->>OrderRepo: Order Saved
    OrderRepo-->>TradingService: Order Created
    
    TradingService->>IBKR: Submit Order to Broker
    IBKR-->>TradingService: Order Acknowledged
    
    TradingService->>SignalR: Push Order Update
    SignalR-->>Frontend: Real-time Order Status
    
    TradingService-->>API: OrderDto
    API-->>Frontend: 201 Created<br/>{orderId, status}
    Frontend-->>User: Show Success Message
    
    Note over IBKR,SignalR: Async Order Fill Updates
    IBKR->>SignalR: Order Filled Event
    SignalR-->>Frontend: Push Fill Notification
    Frontend-->>User: Show Fill Notification
```

### 3.2 Strategy Execution Flow

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant StrategyService
    participant LeanEngine
    participant ExecutionService
    participant TradingService
    participant Database
    participant SignalR
    
    User->>Frontend: Click "Start Strategy"
    Frontend->>API: POST /api/v1/strategies/{id}/start
    
    API->>StrategyService: StartStrategyAsync(strategyId)
    StrategyService->>Database: Load Strategy Config
    Database-->>StrategyService: Strategy Data
    
    StrategyService->>LeanEngine: Initialize Algorithm
    LeanEngine-->>StrategyService: Algorithm Ready
    
    StrategyService->>ExecutionService: Start Execution Loop
    
    loop Every Bar/Tick
        ExecutionService->>LeanEngine: OnData(data)
        LeanEngine->>LeanEngine: Run Strategy Logic
        
        alt Signal Generated
            LeanEngine->>ExecutionService: Trade Signal
            ExecutionService->>TradingService: PlaceOrder(signal)
            TradingService-->>ExecutionService: Order Placed
            
            ExecutionService->>SignalR: Push Trade Event
            SignalR-->>Frontend: Real-time Update
        end
        
        ExecutionService->>SignalR: Push Strategy Stats
        SignalR-->>Frontend: Update Dashboard
    end
    
    StrategyService-->>API: Strategy Started
    API-->>Frontend: 200 OK
    Frontend-->>User: Show "Strategy Running"
```

### 3.3 Real-time Market Data Flow

```mermaid
flowchart TD
    Start([Frontend Loads Trading Page]) --> Subscribe[Subscribe to Symbol via SignalR]
    
    Subscribe --> CheckCache{Quote in<br/>Redis Cache?}
    
    CheckCache -->|Yes| SendCached[Send Cached Quote]
    CheckCache -->|No| RequestIBKR[Request from IBKR]
    
    RequestIBKR --> IBKRStream[IBKR Starts Streaming]
    
    SendCached --> Display[Display Quote on Frontend]
    
    IBKRStream --> ReceiveQuote[Receive Quote Update]
    ReceiveQuote --> UpdateCache[Update Redis Cache]
    UpdateCache --> BroadcastSignalR[Broadcast via SignalR]
    BroadcastSignalR --> Display
    
    Display --> Wait[Wait for Next Update]
    Wait --> ReceiveQuote
    
    User[User Navigates Away] --> Unsubscribe[Unsubscribe from Symbol]
    Unsubscribe --> CheckSubscribers{Other Subscribers<br/>for Symbol?}
    
    CheckSubscribers -->|Yes| KeepStream[Keep IBKR Stream]
    CheckSubscribers -->|No| StopStream[Stop IBKR Stream]
    
    StopStream --> End([End])
    KeepStream --> End
    
    style Start fill:#4caf50
    style Display fill:#2196f3
    style End fill:#f44336
```

### 3.4 Authentication Flow

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant AuthService
    participant Database
    participant Redis
    
    User->>Frontend: Enter Username & Password
    Frontend->>API: POST /api/v1/auth/login
    
    API->>AuthService: LoginAsync(credentials)
    AuthService->>Database: Query User by Username
    Database-->>AuthService: User Record
    
    AuthService->>AuthService: Verify Password Hash
    
    alt Password Correct
        AuthService->>AuthService: Generate JWT Tokens
        AuthService->>Redis: Store Refresh Token
        AuthService-->>API: {accessToken, refreshToken, user}
        API-->>Frontend: 200 OK + Tokens
        Frontend->>Frontend: Store Tokens in LocalStorage
        Frontend-->>User: Redirect to Dashboard
    else Password Incorrect
        AuthService->>Database: Increment Failed Login Count
        AuthService-->>API: 401 Unauthorized
        API-->>Frontend: Error Response
        Frontend-->>User: Show Error Message
    end
    
    Note over Frontend,API: Subsequent Authenticated Requests
    
    Frontend->>API: GET /api/v1/trading/orders<br/>Header: Authorization: Bearer {accessToken}
    API->>API: Validate JWT Signature
    
    alt Token Valid
        API->>API: Process Request
        API-->>Frontend: 200 OK + Data
    else Token Expired
        API-->>Frontend: 401 Unauthorized
        Frontend->>API: POST /api/v1/auth/refresh<br/>{refreshToken}
        API->>Redis: Validate Refresh Token
        Redis-->>API: Token Valid
        API->>AuthService: Generate New Access Token
        AuthService-->>API: New Access Token
        API-->>Frontend: 200 OK + {accessToken}
        Frontend->>Frontend: Update Stored Token
        Frontend->>API: Retry Original Request
    end
```

---

## 4. Sequence Diagrams

### 4.1 Backtest Execution

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant BacktestService
    participant StrategyRepo
    participant LeanEngine
    participant Database
    participant SignalR
    
    User->>Frontend: Configure & Start Backtest
    Frontend->>API: POST /api/v1/backtest<br/>{strategyId, startDate, endDate}
    
    API->>BacktestService: RunBacktestAsync(request)
    BacktestService->>StrategyRepo: GetStrategy(strategyId)
    StrategyRepo->>Database: SELECT * FROM Strategies
    Database-->>StrategyRepo: Strategy Data
    StrategyRepo-->>BacktestService: Strategy Object
    
    BacktestService->>BacktestService: Create Backtest Job
    BacktestService->>Database: INSERT INTO Backtests
    BacktestService-->>API: {backtestId, status: 'running'}
    API-->>Frontend: 202 Accepted
    
    Frontend->>SignalR: Subscribe to Backtest Progress
    
    par Async Backtest Execution
        BacktestService->>LeanEngine: Initialize Backtest
        LeanEngine->>LeanEngine: Load Historical Data
        
        loop For Each Time Period
            LeanEngine->>LeanEngine: Run Algorithm.OnData()
            LeanEngine->>LeanEngine: Execute Trades
            LeanEngine->>BacktestService: Progress Update
            BacktestService->>SignalR: Push Progress (10%, 20%, ...)
            SignalR-->>Frontend: Update Progress Bar
        end
        
        LeanEngine->>LeanEngine: Calculate Statistics
        LeanEngine-->>BacktestService: Backtest Results
        
        BacktestService->>Database: UPDATE Backtests SET results=...
        BacktestService->>SignalR: Push Completion
        SignalR-->>Frontend: Show Results
    end
    
    Frontend->>API: GET /api/v1/backtest/{backtestId}
    API->>BacktestService: GetBacktestResults(backtestId)
    BacktestService->>Database: SELECT * FROM Backtests
    Database-->>BacktestService: Backtest Results
    BacktestService-->>API: BacktestDto
    API-->>Frontend: 200 OK + Results
    Frontend-->>User: Display Performance Metrics & Charts
```

### 4.2 Risk Control Violation

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant TradingService
    participant RiskService
    participant Database
    participant SignalR
    
    User->>Frontend: Place Large Order
    Frontend->>API: POST /api/v1/trading/orders<br/>{symbol: 'AAPL', quantity: 10000}
    
    API->>TradingService: PlaceOrderAsync(request)
    TradingService->>RiskService: ValidateOrder(order)
    
    RiskService->>Database: Get User Risk Settings
    Database-->>RiskService: {maxPositionSize: 5000}
    
    RiskService->>RiskService: Check Position Limits
    
    alt Order Exceeds Limit
        RiskService-->>TradingService: ValidationException<br/>"Exceeds max position size"
        TradingService-->>API: 400 Bad Request
        API-->>Frontend: Error Response
        Frontend-->>User: Show Error Alert<br/>"Order exceeds position limit (max: 5000)"
    else  Order Within Limits
        RiskService-->>TradingService: Validation Passed
        TradingService->>TradingService: Proceed with Order
    end
    
    Note over RiskService,SignalR: Real-time Risk Monitoring
    
    loop Every Minute
        RiskService->>Database: Check Daily P&L
        Database-->>RiskService: Current P&L: -4800
        
        alt Exceeds Max Daily Loss
            RiskService->>TradingService: Stop All Strategies
            TradingService->>SignalR: Push Alert
            SignalR-->>Frontend: Show Critical Alert<br/>"Max daily loss reached"
            Frontend-->>User: Display Warning Banner
        end
    end
```

### 4.3 WebSocket Connection Lifecycle

```mermaid
sequenceDiagram
    participant Frontend
    participant SignalRHub
    participant Redis
    participant IBKR
    
    Frontend->>SignalRHub: Connect (with JWT Token)
    SignalRHub->>SignalRHub: Validate Token
    SignalRHub-->>Frontend: Connected (ConnectionId: abc123)
    
    Frontend->>SignalRHub: SubscribeToSymbol('AAPL')
    SignalRHub->>Redis: Add to Group 'Market_AAPL'
    SignalRHub->>IBKR: Start Data Stream (if not exists)
    IBKR-->>SignalRHub: Stream Started
    SignalRHub-->>Frontend: Subscribed Successfully
    
    loop Real-time Updates
        IBKR->>SignalRHub: Quote Update {symbol: 'AAPL', price: 151.25}
        SignalRHub->>Redis: Cache Quote
        SignalRHub->>SignalRHub: Get 'Market_AAPL' Group
        SignalRHub-->>Frontend: ReceiveQuote(quote)
        Frontend->>Frontend: Update UI
    end
    
    alt Network Interruption
        Frontend-xFrontend: Connection Lost
        Frontend->>Frontend: Auto-reconnect (with exponential backoff)
        Frontend->>SignalRHub: Reconnect
        SignalRHub-->>Frontend: Connected (New ConnectionId)
        Frontend->>SignalRHub: Re-subscribe to 'AAPL'
        SignalRHub-->>Frontend: Subscribed
    end
    
    Frontend->>SignalRHub: UnsubscribeFromSymbol('AAPL')
    SignalRHub->>Redis: Remove from Group 'Market_AAPL'
    
    SignalRHub->>SignalRHub: Check Group Subscribers
    
    alt No More Subscribers
        SignalRHub->>IBKR: Stop Data Stream
        IBKR-->>SignalRHub: Stream Stopped
    end
    
    Frontend->>SignalRHub: Disconnect
    SignalRHub-->>Frontend: Disconnected
```

---

## 5. Deployment Diagrams

### 5.1 Docker Compose Deployment

```mermaid
graph TB
    subgraph "Docker Host"
        subgraph "Containers"
            NGINX[nginx<br/>Port: 80, 443]
            API[webui-api<br/>Port: 5000]
            Frontend[webui-frontend<br/>Port: 3000]
            PostgreSQL[postgres<br/>Port: 5432]
            Redis[redis<br/>Port: 6379]
        end
        
        subgraph "Volumes"
            DBData[postgres-data<br/>Persistent Storage]
            Logs[logs<br/>Application Logs]
            SSL[ssl-certs<br/>SSL Certificates]
        end
        
        subgraph "Networks"
            WebNet[webui-network<br/>Internal Network]
        end
    end
    
    Internet[Internet] --> NGINX
    
    NGINX --> Frontend
    NGINX --> API
    
    API --> PostgreSQL
    API --> Redis
    
    PostgreSQL --> DBData
    API --> Logs
    NGINX --> SSL
    
    NGINX -.- WebNet
    API -.- WebNet
    Frontend -.- WebNet
    PostgreSQL -.- WebNet
    Redis -.- WebNet
    
    style NGINX fill:#009639
    style API fill:#512bd4
    style Frontend fill:#61dafb
    style PostgreSQL fill:#336791
    style Redis fill:#dc382d
```

### 5.2 Production Deployment Architecture

```mermaid
graph TB
    subgraph "User Devices"
        Desktop[Desktop Browser]
        Mobile[Mobile Device]
    end
    
    subgraph "CDN & Load Balancer"
        CloudFlare[CloudFlare CDN<br/>Static Assets]
        LB[Load Balancer<br/>HTTPS Termination]
    end
    
    subgraph "Web Server Cluster"
        NGINX1[NGINX 1<br/>Reverse Proxy]
        NGINX2[NGINX 2<br/>Reverse Proxy]
    end
    
    subgraph "Application Server Cluster"
        API1[API Server 1<br/>.NET 8 + SignalR]
        API2[API Server 2<br/>.NET 8 + SignalR]
        API3[API Server 3<br/>.NET 8 + SignalR]
    end
    
    subgraph "Cache Layer"
        RedisCluster[Redis Cluster<br/>Primary + Replicas]
    end
    
    subgraph "Database Layer"
        PGPrimary[(PostgreSQL Primary<br/>Write Operations)]
        PGReplica1[(PostgreSQL Replica 1<br/>Read Operations)]
        PGReplica2[(PostgreSQL Replica 2<br/>Read Operations)]
    end
    
    subgraph "External Services"
        IBKR2[IBKR Gateway<br/>Live Trading]
        Monitoring[Prometheus + Grafana<br/>Monitoring & Alerts]
    end
    
    subgraph "Backup & Storage"
        S3[AWS S3 / Azure Blob<br/>Backup Storage]
    end
    
    Desktop --> CloudFlare
    Mobile --> CloudFlare
    CloudFlare --> LB
    
    LB --> NGINX1
    LB --> NGINX2
    
    NGINX1 --> API1
    NGINX1 --> API2
    NGINX2 --> API2
    NGINX2 --> API3
    
    API1 --> RedisCluster
    API2 --> RedisCluster
    API3 --> RedisCluster
    
    API1 --> PGPrimary
    API2 --> PGReplica1
    API3 --> PGReplica2
    
    PGPrimary -.Replication.-> PGReplica1
    PGPrimary -.Replication.-> PGReplica2
    
    API1 --> IBKR2
    API2 --> IBKR2
    API3 --> IBKR2
    
    API1 --> Monitoring
    API2 --> Monitoring
    API3 --> Monitoring
    
    PGPrimary -.Backup.-> S3
    
    style Desktop fill:#e1f5ff
    style CloudFlare fill:#f48120
    style LB fill:#4caf50
    style API1 fill:#512bd4
    style RedisCluster fill:#dc382d
    style PGPrimary fill:#336791
    style IBKR2 fill:#ff6600
```

---

## 6. Database Schema

### 6.1 Entity Relationship Diagram

```mermaid
erDiagram
    Users ||--o{ Accounts : has
    Users ||--o{ Strategies : owns
    Users ||--o{ RefreshTokens : has
    
    Accounts ||--o{ Orders : places
    Accounts ||--o{ Positions : holds
    Accounts ||--o{ Trades : executes
    
    Strategies ||--o{ Orders : generates
    Strategies ||--o{ StrategyExecutions : logs
    Strategies ||--o{ Backtests : runs
    
    Orders ||--o{ OrderFills : has
    Orders }o--|| OrderTypes : "is type of"
    
    Positions }o--|| Accounts : "belongs to"
    
    Backtests ||--o{ BacktestTrades : contains
    
    Users {
        int Id PK
        string Username UK
        string PasswordHash
        string Email
        string Role
        datetime CreatedAt
        datetime LastLoginAt
        int FailedLoginAttempts
        datetime LockoutEnd
    }
    
    Accounts {
        string AccountId PK
        int UserId FK
        string BrokerName
        string AccountType
        decimal NetLiquidation
        decimal AvailableFunds
        decimal BuyingPower
        datetime LastUpdated
    }
    
    Orders {
        int Id PK
        string OrderId UK
        string BrokerId UK
        string Symbol
        int Quantity
        int FilledQuantity
        string OrderType
        string Direction
        decimal LimitPrice
        decimal StopPrice
        decimal AverageFilledPrice
        string Status
        datetime CreatedAt
        datetime UpdatedAt
        datetime FilledAt
        string AccountId FK
        int StrategyId FK
    }
    
    Positions {
        int Id PK
        string AccountId FK
        string Symbol
        int Quantity
        decimal AveragePrice
        decimal CurrentPrice
        decimal MarketValue
        decimal UnrealizedPnL
        decimal CostBasis
        datetime LastUpdated
    }
    
    Strategies {
        int Id PK
        int UserId FK
        string Name
        string Description
        string AlgorithmCode
        string Type
        string Status
        json Parameters
        datetime CreatedAt
        datetime StartedAt
        datetime StoppedAt
    }
    
    StrategyExecutions {
        int Id PK
        int StrategyId FK
        string LogLevel
        string Message
        json Data
        datetime Timestamp
    }
    
    Backtests {
        int Id PK
        int StrategyId FK
        datetime StartDate
        datetime EndDate
        decimal InitialCapital
        string Status
        json Results
        datetime CreatedAt
        datetime CompletedAt
    }
    
    Trades {
        int Id PK
        string TradeId UK
        int OrderId FK
        string AccountId FK
        string Symbol
        int Quantity
        string Direction
        decimal Price
        decimal Amount
        decimal Commission
        datetime ExecutedAt
    }
    
    OrderFills {
        int Id PK
        int OrderId FK
        string FillId UK
        int Quantity
        decimal Price
        decimal Commission
        datetime Timestamp
    }
    
    RefreshTokens {
        int Id PK
        int UserId FK
        string Token UK
        datetime ExpiresAt
        datetime CreatedAt
        bool IsRevoked
    }
    
    OrderTypes {
        int Id PK
        string Name UK
        string Description
    }
```

### 6.2 Database Index Strategy

```mermaid
graph TD
    subgraph "High-Frequency Query Tables"
        Orders[Orders Table<br/>~100K rows/day]
        Positions[Positions Table<br/>~1K rows]
        Trades[Trades Table<br/>~200K rows/day]
    end
    
    subgraph "Index Strategy"
        PK[Primary Key Indexes<br/>Auto-created]
        
        OrderIndexes[Orders Indexes:<br/>- AccountId<br/>- Symbol<br/>- Status<br/>- CreatedAt<br/>- (AccountId, CreatedAt) Composite]
        
        PositionIndexes[Positions Indexes:<br/>- AccountId<br/>- Symbol<br/>- (AccountId, Symbol) UK]
        
        TradeIndexes[Trades Indexes:<br/>- AccountId<br/>- Symbol<br/>- ExecutedAt<br/>- (AccountId, ExecutedAt) Composite]
        
        StrategyIndexes[Strategies Indexes:<br/>- UserId<br/>- Status<br/>- (UserId, Status) Composite]
    end
    
    subgraph "Query Optimization"
        CoveringIndex[Covering Indexes<br/>Include columns in index]
        Partitioning[Table Partitioning<br/>By date for large tables]
        Stats[Statistics Updates<br/>Regular ANALYZE/UPDATE STATISTICS]
    end
    
    Orders --> OrderIndexes
    Positions --> PositionIndexes
    Trades --> TradeIndexes
    
    OrderIndexes --> CoveringIndex
    TradeIndexes --> Partitioning
    
    PK --> Stats
    OrderIndexes --> Stats
    
    style Orders fill:#ff9800
    style OrderIndexes fill:#4caf50
    style CoveringIndex fill:#2196f3
```

---

## 7. Security Architecture

### 7.1 Security Layers

```mermaid
graph TB
    subgraph "Network Security"
        Firewall[Firewall<br/>Allow: 80, 443<br/>Block: All Others]
        WAF[Web Application Firewall<br/>OWASP Top 10 Protection]
        DDoS[DDoS Protection<br/>CloudFlare]
    end
    
    subgraph "Transport Security"
        TLS[TLS 1.3<br/>Strong Cipher Suites]
        HSTS[HSTS Header<br/>Force HTTPS]
        Cert[SSL Certificate<br/>Let's Encrypt / DigiCert]
    end
    
    subgraph "Application Security"
        Auth[JWT Authentication<br/>RS256 Signing]
        RBAC[Role-Based Access Control<br/>User/Admin Roles]
        RateLimit[Rate Limiting<br/>60 req/min per user]
        CORS[CORS Policy<br/>Whitelist Origins]
    end
    
    subgraph "Data Security"
        Encryption[Data at Rest Encryption<br/>AES-256]
        Hashing[Password Hashing<br/>PBKDF2 (10K iterations)]
        InputVal[Input Validation<br/>FluentValidation]
        SQLPrev[SQL Injection Prevention<br/>Parameterized Queries]
    end
    
    subgraph "Monitoring & Audit"
        Logging[Structured Logging<br/>Serilog]
        AuditLog[Audit Trail<br/>All Critical Operations]
        Alerts[Security Alerts<br/>Failed Login, Unusual Activity]
    end
    
    Internet[Internet] --> DDoS
    DDoS --> WAF
    WAF --> Firewall
    
    Firewall --> TLS
    TLS --> HSTS
    HSTS --> Cert
    
    Cert --> Auth
    Auth --> RBAC
    RBAC --> RateLimit
    RateLimit --> CORS
    
    CORS --> Encryption
    Encryption --> Hashing
    Hashing --> InputVal
    InputVal --> SQLPrev
    
    SQLPrev --> Logging
    Logging --> AuditLog
    AuditLog --> Alerts
    
    style DDoS fill:#f44336
    style TLS fill:#4caf50
    style Auth fill:#2196f3
    style Encryption fill:#9c27b0
    style Logging fill:#ff9800
```

---

## Appendix: Diagram Rendering

These diagrams are written in **Mermaid** syntax and can be rendered in:

1. **GitHub**: Natively supports Mermaid in Markdown files
2. **VS Code**: Install "Markdown Preview Mermaid Support" extension
3. **Online**: https://mermaid.live/
4. **Documentation Sites**: MkDocs, Docusaurus, GitBook (with Mermaid plugins)

### Example: Rendering in VS Code

1. Install extension: `bierner.markdown-mermaid`
2. Open this file in VS Code
3. Press `Ctrl+Shift+V` (Windows/Linux) or `Cmd+Shift+V` (macOS) to preview
4. Diagrams will render automatically

### Export Options

- **PNG/SVG**: Use Mermaid Live Editor → Export
- **PDF**: VS Code Preview → Print to PDF
- **Embed in Documentation**: Copy diagram code blocks

---

**For more details, refer to:**
- [Mermaid Documentation](https://mermaid.js.org/)
- [Developer Guide](./WebUI-Developer-Guide.md)
- [API Reference](./WebUI-API-Reference.md)

