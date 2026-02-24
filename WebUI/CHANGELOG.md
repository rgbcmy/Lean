# Changelog

All notable changes to the Lean WebUI project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-19

### Added

#### Infrastructure & Core
- **Project Setup**: Initialized ASP.NET Core Web API (.NET 10) and React + TypeScript frontend (Vite)
- **Database Layer**: Complete EF Core implementation with PostgreSQL and SQLite support
  - Entity models: User, BrokerAccount, Strategy, StrategyExecution, Order, Position, AuditLog
  - Repository pattern with Unit of Work
  - Automatic database migrations and seeding
  - Database health checks and connection validation
- **Docker Support**: Multi-stage Dockerfile and docker-compose for production deployment
- **CI/CD**: GitHub Actions configuration for automated builds and tests

#### Authentication & Security
- **JWT Authentication**: Full JWT token lifecycle with refresh token mechanism
- **Password Security**: PBKDF2 hashing with 10,000 iterations
- **Session Management**: Login/logout APIs with automatic token renewal
- **Audit Logging**: Complete operation audit trail
- **Rate Limiting**: 60 requests per minute per user
- **Login Protection**: Account lockout after 5 failed attempts (30 minutes)
- **Credential Encryption**: DPAPI (Windows) and AES-GCM (Linux/macOS) for IBKR credentials

#### IBKR Integration
- **Connection Management**: Automatic connection, disconnection, and health monitoring
- **Account Queries**: Real-time account balance and buying power
- **Auto-Reconnect**: Exponential backoff retry logic (1s → 60s)
- **Paper/Live Trading**: Support for both IBKR paper and live accounts
- **Connection Diagnostics**: Comprehensive connection status API

#### Market Data
- **Real-Time Quotes**: Level 1 market data (last price, bid/ask, volume)
- **Subscription Management**: Topic-based subscription system
- **Market Status**: Trading hours detection (pre-market, market, after-hours, closed)
- **Data Caching**: Redis caching with 5-minute expiration
- **Delayed Data**: 15-minute delayed quote identification
- **Rate Throttling**: 1 update per second maximum

#### Trading APIs
- **Order Management**: Market and limit orders for US stocks and ETFs
- **Order Lifecycle**: Submit, cancel, modify orders via REST API
- **Order Tracking**: Real-time order status updates
- **Order History**: Complete order execution history
- **Pre-Trade Validation**: Buying power, position limits, trading hours checks
- **Batch Orders**: Support for multiple simultaneous orders

#### Portfolio Management
- **Position Tracking**: Real-time position updates with P&L calculation
- **Portfolio Query**: Current holdings, cost basis, unrealized/realized P&L
- **Position Analysis**: Concentration, allocation, and performance metrics
- **Risk Metrics**: VaR, Sharpe Ratio, Maximum Drawdown, Beta
- **Export Functionality**: Portfolio export to CSV/Excel

#### Strategy Management
- **Strategy CRUD**: Create, read, update, delete strategies via API
- **Configuration Generation**: Automatic Lean config file (JSON/XML) generation
- **Code Upload**: Strategy code file upload and storage
- **Strategy Cloning**: Duplicate strategies with new parameters
- **Version Control**: Strategy version management API
- **Tag Management**: Organize strategies with custom tags
- **Import/Export**: Strategy backup and restore

#### Strategy Execution
- **Process Management**: Lean engine process start/stop with graceful shutdown
- **Health Monitoring**: IPC-based heartbeat checks
- **Parameter Passing**: Strategy parameters via config files
- **Log Streaming**: Real-time strategy log streaming via SignalR
- **Crash Detection**: Automatic crash detection and restart
- **Scheduler**: Cron-based scheduled strategy execution
- **Execution History**: Complete strategy run history

#### Backtesting System
- **Backtest Configuration**: Date range, initial capital, benchmark selection
- **Backtest Execution**: Integration with Lean backtesting engine
- **Result Parsing**: Comprehensive result parsing and storage
- **Performance Metrics**: Return, Sharpe ratio, max drawdown, win rate
- **Comparison Tool**: Side-by-side backtest comparison
- **Parameter Optimization**: Grid search optimization
- **Report Generation**: PDF and JSON report export

#### Risk Control
- **Stop Loss/Take Profit**: Configurable stop loss and take profit rules
- **Position Limits**: Per-stock, total position, and cash ratio limits
- **Trade Frequency**: Daily trade frequency restrictions
- **Margin Monitoring**: Real-time margin usage alerts
- **Concentration Checks**: Portfolio concentration limits
- **Circuit Breakers**: Price volatility and order volume anomaly detection
- **PDT Rule**: Pattern Day Trader rule compliance
- **Risk Dashboard**: Real-time risk metrics display

#### Web Frontend
- **Modern UI Framework**: React 19 + TypeScript + Ant Design
- **Routing**: React Router with protected routes
- **State Management**: Zustand for global state
- **Real-Time Updates**: SignalR integration for live data
- **HTTP Client**: Axios with request/response interceptors
- **Theme Support**: Dark/light theme switching
- **Internationalization**: Chinese language UI with English documentation
- **Responsive Design**: Desktop, tablet, and mobile browser support

#### User Interface Pages
- **Authentication**: Login page, password change, logout
- **Dashboard**: Account overview, quick actions, notifications
- **Trading**: Stock search, order form, order list, ETF trading, recurring investments
- **Portfolio**: Position list, position details, allocation charts, equity curve
- **Strategies**: Strategy list, create/edit strategy, strategy details, execution logs
- **Backtesting**: Backtest configuration, results, comparison, optimization
- **Charts**: Candlestick charts, technical indicators (MA, MACD, RSI), live price charts
- **Risk Management**: Risk configuration, position limits, risk dashboard
- **Settings**: IBKR connection, database provider, theme, notifications

#### Data Visualization
- **K-Line Charts**: ECharts-based candlestick charts with zoom and pan
- **Technical Indicators**: Moving averages, MACD, RSI
- **Live Price Charts**: Lightweight Charts for real-time price updates
- **Allocation Pie Charts**: Portfolio allocation visualization
- **P&L Bar Charts**: Profit/loss breakdown
- **Equity Curves**: Account performance over time
- **Chart Export**: PNG/SVG chart export

#### SignalR Real-Time Communication
- **Market Data Hub**: Real-time quote streaming
- **Order Hub**: Live order status updates
- **Position Hub**: Real-time position updates
- **Strategy Hub**: Live strategy log streaming
- **JWT Authentication**: Secure WebSocket connections
- **Auto-Reconnect**: Client-side reconnection handling
- **Update Throttling**: Intelligent update batching

#### API Features
- **RESTful Design**: Consistent REST API design patterns
- **API Versioning**: /api/v1/ versioning scheme
- **OpenAPI Documentation**: Swagger/Scalar UI with Chinese and English annotations
- **Validation**: FluentValidation for request validation
- **Error Handling**: Unified error response format
- **Compression**: Gzip response compression
- **Health Checks**: /health, /health/detailed, /health/ready endpoints
- **Structured Logging**: Serilog with JSON formatting

#### Testing & Quality
- **Unit Tests**: Backend unit tests (>70% coverage)
- **Integration Tests**: API endpoint integration tests
- **Frontend Tests**: Jest + React Testing Library component tests
- **E2E Tests**: Playwright end-to-end tests
- **Performance Tests**: API response time and concurrency tests
- **Security Audit**: OWASP Top 10 compliance
- **Code Quality**: SonarQube/CodeQL scanning
- **Cross-Browser**: Chrome, Firefox, Edge, Safari compatibility
- **Cross-Platform**: Windows, Linux, macOS compatibility

#### Documentation
- **User Guide**: Chinese and English installation guides
- **IBKR Setup**: Interactive Brokers connection tutorial
- **Database Setup**: PostgreSQL and SQLite configuration guides
- **Docker Deployment**: Container deployment tutorial
- **Developer Guide**: Chinese and English developer documentation
- **API Reference**: Complete API endpoint documentation
- **Architecture Diagrams**: System architecture and flow diagrams
- **Video Tutorial**: Quick start video script

#### Deployment & Operations
- **Production Config**: Production-ready appsettings.json
- **HTTPS Support**: Self-signed and Let's Encrypt certificate setup
- **Static File Serving**: Nginx and ASP.NET static file configuration
- **Log Aggregation**: ELK Stack and Seq integration guides
- **Monitoring**: Prometheus + Grafana and Application Insights setup
- **Database Backup**: Automated backup scripts
- **Operations Manual**: Troubleshooting and rollback procedures
- **Disaster Recovery**: Disaster recovery drill documentation
- **Production Checklist**: Pre-release deployment checklist

#### Phase 1 (MVP)
- ✅ IBKR connection and authentication
- ✅ US stock manual trading (market/limit orders)
- ✅ Real-time order status updates
- ✅ Position and balance queries
- ✅ Basic web interface (login, navigation, responsive)
- ✅ User acceptance testing (3+ scenarios)

#### Phase 2 (Enhancements)
- ✅ Real-time market data subscription
- ✅ ETF search and trading
- ✅ Recurring investment plans
- ✅ Strategy start/stop controls
- ✅ Real-time strategy log display
- ✅ K-line charts with technical indicators
- ✅ UI/UX optimization
- ✅ Performance optimization (<500ms API response)

#### Phase 3 (Complete)
- ✅ Backtesting system
- ✅ Parameter optimization
- ✅ Backtest report generation
- ✅ Risk control rules (stop loss, position limits)
- ✅ Risk metric calculations
- ✅ All chart components
- ✅ Final performance tuning (100 orders/sec load test)
- ✅ Final security hardening

### Technical Stack

**Backend:**
- ASP.NET Core 10.0
- Entity Framework Core 10.0
- SignalR for real-time communication
- PostgreSQL 16 and SQLite
- JWT authentication
- Serilog structured logging
- FluentValidation
- Redis caching

**Frontend:**
- React 19
- TypeScript 5.9
- Vite 7
- Ant Design 6
- React Router 7
- Zustand state management
- ECharts 6
- Lightweight Charts 5
- Axios
- SignalR Client

**DevOps:**
- Docker multi-stage builds
- Docker Compose
- GitHub Actions CI/CD
- xUnit, Moq, FluentAssertions
- Playwright E2E testing
- ESLint, TypeScript compiler

**Security:**
- JWT with refresh tokens
- PBKDF2 password hashing
- Rate limiting
- CORS configuration
- HTTPS/TLS
- Audit logging
- DPAPI/AES-GCM credential encryption

### System Requirements

**Minimum:**
- .NET 10.0 Runtime
- Node.js 20+ (for frontend build)
- 2GB RAM
- 10GB disk space
- SQLite (development)

**Recommended:**
- .NET 10.0 Runtime
- PostgreSQL 16
- 4GB RAM
- 20GB disk space
- Redis (for caching)

**Supported Platforms:**
- Windows 10/11 (x64)
- Linux (Ubuntu 20.04+, Debian 11+, RHEL 8+)
- macOS 12+ (x64/ARM64)

### Known Limitations

- Single-user system (multi-tenancy not supported in v1.0)
- IBKR only (other brokerages not supported)
- US stocks and ETFs only (no futures, options, forex)
- Local deployment only (no cloud hosting service)
- English/Chinese only (no other languages)

### Breaking Changes

This is the initial 1.0.0 release, no breaking changes from previous versions.

### Migration Notes

As this is the first release (v1.0.0), no migration from previous versions is required.

### Contributors

- Development Team
- Testing Team
- Documentation Team

### Special Thanks

- QuantConnect Lean team for the core algorithmic trading engine
- Interactive Brokers for their API
- All open-source contributors of the libraries used in this project

---

## [Unreleased]

### Planned for v1.1.0
- Multi-user support with role-based access control
- Mobile native apps (iOS/Android)
- Additional broker integrations
- Options and futures trading support
- Social features (strategy sharing, community forums)
- Cloud hosting service
- Advanced charting tools
- More languages (Japanese, Korean, Spanish)

---

[1.0.0]: https://github.com/QuantConnect/Lean/releases/tag/webui-v1.0.0
[Unreleased]: https://github.com/QuantConnect/Lean/compare/webui-v1.0.0...HEAD
