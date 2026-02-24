# Lean WebUI User Guide

## Table of Contents

- [Introduction](#introduction)
- [System Requirements](#system-requirements)
- [Quick Start](#quick-start)
- [Installation & Deployment](#installation--deployment)
  - [Docker Compose Deployment (Recommended)](#docker-compose-deployment-recommended)
  - [Local Deployment](#local-deployment)
- [Configuration Guide](#configuration-guide)
  - [Database Configuration](#database-configuration)
  - [IBKR Connection Configuration](#ibkr-connection-configuration)
  - [Security Configuration](#security-configuration)
- [Features](#features)
  - [Login](#login)
  - [Dashboard](#dashboard)
  - [Account Management](#account-management)
  - [Live Trading](#live-trading)
  - [Position Management](#position-management)
  - [Order Management](#order-management)
  - [Strategy Management](#strategy-management)
  - [Backtesting System](#backtesting-system)
  - [Data Visualization](#data-visualization)
  - [Risk Control](#risk-control)
- [FAQ](#faq)
- [Troubleshooting](#troubleshooting)

---

## Introduction

Lean WebUI is a modern web management interface for the QuantConnect Lean quantitative trading engine, designed specifically for individual quantitative traders. With WebUI, you can:

- 🖥️ **Visual Operations**: Perform all trading operations through a browser without writing code
- 📊 **Real-time Monitoring**: View account, position, order status, and market data in real-time
- 🤖 **Strategy Management**: Visually manage and monitor quantitative strategy execution
- 📈 **Data Visualization**: Professional charts including candlestick charts, technical indicators, and equity curves
- 🛡️ **Risk Control**: Built-in stop-loss, take-profit, and position limit functions
- 🌐 **Multi-language Support**: Complete English and Chinese interface and documentation

### Key Features

- ✅ Support for Interactive Brokers (IBKR) live trading
- ✅ Focus on US equity markets (stocks, ETFs)
- ✅ Real-time market data push (sub-second latency)
- ✅ Decoupled frontend and backend architecture, responsive design
- ✅ Support for PostgreSQL and SQLite databases
- ✅ Local deployment, secure and controllable data
- ✅ HTTPS encryption, JWT authentication
- ✅ Cross-platform support (Windows/Linux/macOS)

---

## System Requirements

### Hardware Requirements

- **CPU**: 2 cores or more (4 cores recommended)
- **Memory**: 4GB or more (8GB recommended)
- **Storage**: 10GB available space (SSD recommended)
- **Network**: Stable internet connection (low latency for live trading)

### Software Requirements

#### Docker Deployment (Recommended)

- Docker 20.10+
- Docker Compose 2.0+
- OS: Windows 10/11, Linux (Ubuntu 20.04+), macOS 11+

#### Local Deployment

- .NET 10 Runtime/SDK
- Node.js 18+ and npm 9+
- Database:
  - PostgreSQL 14+ (recommended for production)
  - SQLite 3.35+ (for development/testing)
- OS: Windows 10/11, Linux, macOS

#### IBKR Requirements

- Interactive Brokers account
- TWS (Trader Workstation) or IB Gateway installed and configured
- API access enabled

---

## Quick Start

If you want to quickly experience Lean WebUI, follow these steps:

### 1. Clone the Repository

```bash
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI
```

### 2. Start with Docker Compose (Easiest)

```bash
docker-compose up -d
```

### 3. Access WebUI

Open your browser and visit: `https://localhost:5001`

Default login credentials:
- Username: `admin`
- Password: `ChangeMe123!`

⚠️ **Security Alert**: Please change the default password immediately after first login!

---

## Installation & Deployment

### Docker Compose Deployment (Recommended)

Docker Compose is the simplest deployment method, suitable for production environments.

#### 1. Prepare Configuration File

Create or edit `docker-compose.yml` in the `WebUI` directory:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: lean-postgres
    environment:
      POSTGRES_DB: leanui
      POSTGRES_USER: leanuser
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    restart: unless-stopped

  api:
    build:
      context: ./WebUI.API
      dockerfile: Dockerfile
    container_name: lean-api
    depends_on:
      - postgres
    environment:
      Database__Provider: PostgreSQL
      Database__ConnectionString: Host=postgres;Database=leanui;Username=leanuser;Password=${DB_PASSWORD}
      JWT__SecretKey: ${JWT_SECRET}
      IBKR__TWS__Host: ${IBKR_HOST:-host.docker.internal}
      IBKR__TWS__Port: ${IBKR_PORT:-7497}
    ports:
      - "5000:5000"
      - "5001:5001"
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    restart: unless-stopped

  frontend:
    build:
      context: ./WebUI.Frontend
      dockerfile: Dockerfile
    container_name: lean-frontend
    depends_on:
      - api
    ports:
      - "3000:80"
    restart: unless-stopped

volumes:
  postgres_data:
```

#### 2. Create Environment Variables File

Create a `.env` file:

```env
# Database password
DB_PASSWORD=your_secure_password_here

# JWT secret key (at least 32 characters)
JWT_SECRET=your_jwt_secret_key_at_least_32_characters_long

# IBKR TWS/Gateway configuration
IBKR_HOST=host.docker.internal
IBKR_PORT=7497
```

#### 3. Start Services

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Check service status
docker-compose ps
```

#### 4. Initialize Database

On first startup, the database will automatically migrate and create table structures. You can also manually execute:

```bash
docker-compose exec api dotnet ef database update
```

#### 5. Access Application

- **WebUI Frontend**: http://localhost:3000
- **API Endpoints**: http://localhost:5000
- **API Documentation (Swagger)**: http://localhost:5000/swagger

---

### Local Deployment

If you prefer not to use Docker, you can run directly on your local machine.

#### 1. Install Dependencies

Ensure you have installed:
- .NET 10 SDK
- Node.js 18+
- PostgreSQL or SQLite

#### 2. Configure Database

##### Using PostgreSQL (Recommended)

```bash
# Install PostgreSQL
# Windows: Download from https://www.postgresql.org/download/windows/
# Linux: sudo apt install postgresql
# macOS: brew install postgresql

# Create database
psql -U postgres
CREATE DATABASE leanui;
CREATE USER leanuser WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;
\q
```

##### Using SQLite (Development)

No additional configuration needed, the program will automatically create a `lean.db` file.

#### 3. Configure Backend

Edit `WebUI.API/appsettings.json`:

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password"
  },
  "JWT": {
    "SecretKey": "your_jwt_secret_key_at_least_32_characters_long",
    "Issuer": "LeanWebUI",
    "Audience": "LeanWebUIClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

#### 4. Start Backend

```bash
cd WebUI.API
dotnet restore
dotnet ef database update
dotnet run
```

#### 5. Configure Frontend

Edit `WebUI.Frontend/.env`:

```env
REACT_APP_API_URL=https://localhost:5001/api
REACT_APP_SIGNALR_URL=https://localhost:5001/hubs
```

#### 6. Start Frontend

```bash
cd WebUI.Frontend
npm install
npm start
```

The browser will automatically open `http://localhost:3000`

---

## Configuration Guide

### Database Configuration

#### PostgreSQL Configuration

Suitable for production environments, providing better performance and concurrency support.

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password;Port=5432"
  }
}
```

**Connection String Parameters**:
- `Host`: Database server address
- `Database`: Database name
- `Username`: Username
- `Password`: Password
- `Port`: Port (default 5432)

#### SQLite Configuration

Suitable for development/testing environments, no additional database service required.

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

#### Switching Databases

To switch databases, simply modify `Provider` and `ConnectionString`, then re-run migrations:

```bash
dotnet ef database update
```

---

### IBKR Connection Configuration

For detailed IBKR configuration tutorial, see: [IBKR Configuration Guide](./IBKR-Configuration-Guide.md)

#### Basic Configuration

```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    },
    "Account": {
      "AccountId": "your_ibkr_account_id"
    }
  }
}
```

#### Port Reference

- **TWS Paper Trading**: 7497
- **TWS Live Trading**: 7496
- **IB Gateway Paper Trading**: 4002
- **IB Gateway Live Trading**: 4001

#### Prerequisites

1. **Start TWS or IB Gateway**
2. **Enable API Access**:
   - TWS: `File → Global Configuration → API → Settings`
   - Check `Enable ActiveX and Socket Clients`
   - Uncheck `Read-Only API` (if trading is required)
   - Add `127.0.0.1` to trusted IP list
3. **Configure Port**: Ensure port number matches configuration file

---

### Security Configuration

#### Generate JWT Secret Key

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell (Windows)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

Fill the generated key into `JWT.SecretKey` in `appsettings.json`.

#### HTTPS Certificate Configuration

##### Development Environment (Self-signed Certificate)

```bash
dotnet dev-certs https --trust
```

##### Production Environment (Let's Encrypt)

Refer to the Caddy configuration in the Docker deployment tutorial.

#### Change Default Password

After first login, go to `Settings → Account Security` to change password.

Or use the command-line tool:

```bash
dotnet WebUI.API.dll --reset-password
```

---

## Features

### Login

1. Visit the WebUI address (default `https://localhost:5001`)
2. Enter username and password
3. Please change the default password immediately after first login
4. After login, you will receive an Access Token valid for 15 minutes

**Multi-device Login**: Supports logging in to the same account on multiple devices (shared Refresh Token).

---

### Dashboard

The dashboard is displayed after login, showing:

- **Account Overview**: Total assets, available cash, position value, today's P&L
- **Position Overview**: Position list (top 5), P&L ranking
- **Today's Orders**: Latest order status
- **Strategy Status**: Running strategies and their performance
- **Market Heat**: Popular stock gainers/losers
- **Quick Actions**: Quick order placement, start strategies

---

### Account Management

#### View Account Information

`Account → Account Overview`

- Account balance (cash, stock value, total assets)
- Margin usage
- Buying power
- Account historical equity curve

#### Connection Status Monitoring

`Account → Connection Status`

- IBKR connection status (Connected/Disconnected)
- Last heartbeat time
- Data latency
- Reconnect button

---

### Live Trading

#### Search Stocks

`Trading → Stock Search`

1. Enter stock symbol (e.g., `AAPL`) or company name
2. View real-time quotes, basic information, technical indicators
3. Click `Buy` or `Sell`

#### Place Order

`Trading → Place Order`

1. **Select Stock**: Enter symbol or select from holdings
2. **Select Order Type**:
   - **Market Order**: Execute immediately at market price
   - **Limit Order**: Execute at specified price
   - **Stop Order**: Convert to market order after trigger price
   - **Stop Limit Order**: Convert to limit order after trigger
3. **Enter Quantity**: Integer number of shares
4. **Set Time in Force**:
   - Day (Day)
   - Good till Canceled (GTC)
5. **Confirm Order**: Review order details and click confirm

**Example**:

```
Symbol: AAPL
Order Type: Limit Order
Price: $150.00
Quantity: 10 shares
Direction: Buy
Time in Force: Day
```

#### ETF Trading

`Trading → ETF`

- Support for mainstream ETFs (SPY, QQQ, IWM, etc.)
- Provide dollar-cost averaging (DCA) feature (daily/weekly fixed amount purchases)

---

### Position Management

`Positions → Position List`

#### View Positions

Table displays:
- Stock symbol
- Position quantity
- Average cost
- Current price
- Unrealized P&L (amount, percentage)
- Market value

#### Position Actions

- **Close All**: Quickly sell all positions
- **Partial Close**: Sell specified quantity
- **Set Stop-Loss/Take-Profit**: Automatically trigger sell orders

#### Position Analysis

`Positions → Portfolio Analysis`

- **Position Distribution Pie Chart**: By market value percentage
- **Industry Distribution**: By industry classification
- **Equity Curve**: Account net value history
- **Risk Metrics**: Sharpe ratio, maximum drawdown, volatility

---

### Order Management

`Orders → Order History`

#### View Orders

- **Today's Orders**: All orders for the day
- **Historical Orders**: All order history (filterable by date range)
- **Status Filter**: Pending, Submitted, Partially Filled, Filled, Canceled

#### Order Details

Click on an order to view:
- Order basic information (stock, type, price, quantity)
- Fill details (details of multiple fills)
- Order status flow history
- Commission and fees

#### Cancel Order

For unfilled or partially filled orders, click the `Cancel Order` button.

---

### Strategy Management

`Strategies → Strategy List`

#### Create New Strategy

1. Click `New Strategy`
2. Select strategy template or start from scratch
3. Configure strategy parameters:
   - **Strategy Name**
   - **Strategy Code** (upload C# or Python file)
   - **Initial Capital**
   - **Trading Instruments**
   - **Time Range**
4. Save strategy

#### Start Strategy

1. Enter strategy detail page
2. Check configuration parameters
3. Click `Start Strategy`
4. Confirm risk warning

#### Monitor Strategy

When strategy is running, you can view:
- **Real-time Logs**: Log information output by strategy (auto-scroll)
- **Current Holdings**: Stocks held by strategy
- **Order Records**: All orders placed by strategy
- **Performance Metrics**: Return rate, Sharpe ratio, win rate

#### Stop Strategy

Click the `Stop Strategy` button, and the strategy will gracefully shutdown (complete current trades then stop).

#### Strategy Version Management

`Strategies → Version History`

- View all historical versions of strategy
- Roll back to old versions
- Compare differences between versions

---

### Backtesting System

`Backtest → New Backtest`

#### Configure Backtest

1. **Select Strategy**: Choose from existing strategies
2. **Set Backtest Parameters**:
   - **Start and End Date**: Historical data range
   - **Initial Capital**: $100,000
   - **Benchmark**: SPY (for comparison)
3. **Click Run Backtest**

#### View Backtest Results

`Backtest → Backtest Results`

- **Equity Curve Chart**: Account net value changes
- **Drawdown Chart**: Maximum drawdown and drawdown periods
- **Performance Metrics**:
  - Total return
  - Annualized return
  - Sharpe ratio
  - Maximum drawdown
  - Win rate
  - Profit factor
- **Trade Details**: All backtest trade records

#### Parameter Optimization

`Backtest → Parameter Optimization`

- Select parameters to optimize (e.g., moving average period)
- Set parameter range (e.g., 10-50, step 5)
- Run grid search
- View optimal parameter combination

#### Backtest Comparison

`Backtest → Comparative Analysis`

- Select multiple backtest results
- Compare performance metrics side by side
- Generate comparison report (PDF)

---

### Data Visualization

#### Candlestick Chart

`Charts → Candlestick Chart`

- Select stock symbol
- Switch time period (daily, weekly, monthly)
- Overlay technical indicators (MA, MACD, RSI, Bollinger Bands)
- Zoom and pan (mouse wheel, drag)
- Export chart (PNG/SVG)

#### Real-time Quote Chart

`Charts → Real-time Quotes`

- Lightweight real-time price chart
- 1-second update frequency
- Support multi-stock comparison

#### Position Distribution Chart

`Positions → Distribution Chart`

- Pie chart showing stock allocation percentage
- Bar chart showing P&L ranking

#### Equity Curve

`Account → Equity Curve`

- Account net value historical curve
- Compare with benchmark (SPY)
- Display drawdown regions

---

### Risk Control

`Settings → Risk Control`

#### Stop-Loss and Take-Profit Rules

- **Global Stop-Loss**: Close all positions when total account assets fall below threshold (e.g., -5%)
- **Individual Stock Stop-Loss**: Sell when individual stock falls below purchase price by certain percentage (e.g., -3%)
- **Take-Profit**: Sell when individual stock profit reaches target (e.g., +10%)

#### Position Limits

- **Maximum Position Per Stock**: Not more than 20% of total assets
- **Total Position Count Limit**: Hold at most 10 stocks
- **Cash Ratio**: Maintain at least 10% cash

#### Trading Frequency Limits

- **Intraday Trades**: Maximum 20 trades per day
- **Pattern Day Trader (PDT) Check**: Limit day trading when account is below $25,000

#### Circuit Breaker Mechanism

- **Abnormal Price Fluctuation**: Reject orders where price deviates more than 5% from market price
- **Abnormal Order Volume**: Alert when single order exceeds 1% of average daily volume

---

## FAQ

### Q1: Cannot Connect to IBKR TWS

**Reasons**:
- TWS not running
- API settings not enabled
- Port configuration error
- Firewall blocking

**Solutions**:
1. Confirm TWS or IB Gateway is started
2. TWS: `File → Global Configuration → API → Settings`, enable API
3. Check port number (paper trading 7497, live 7496)
4. Add `127.0.0.1` to trusted IPs
5. Temporarily disable firewall for testing

---

### Q2: Logged Out Immediately After Login

**Reason**: JWT Token configuration error or inconsistent secret key

**Solutions**:
1. Check that `JWT.SecretKey` in `appsettings.json` is at least 32 characters
2. Clear browser cache and cookies
3. Restart backend service

---

### Q3: Real-time Quotes Not Updating

**Reasons**:
- SignalR connection disconnected
- IBKR data subscription unsuccessful
- Market closed

**Solutions**:
1. Press F12 to open browser developer tools, check Console for WebSocket errors
2. Check IBKR connection status (Account → Connection Status)
3. Confirm market trading hours (US stocks: 9:30-16:00 ET)
4. Re-subscribe to market data

---

### Q4: Order Submission Failed

**Reasons**:
- Insufficient account balance
- Exceeds position limits
- Risk control rules triggered
- Market closed

**Solutions**:
1. View order error message
2. Check account buying power (Account → Account Overview)
3. Check risk control settings (Settings → Risk Control)
4. Confirm market trading hours

---

### Q5: Strategy Start Failed

**Reasons**:
- Strategy code syntax error
- Lean engine not correctly installed
- Configuration file error

**Solutions**:
1. View strategy logs (Strategy detail page → Logs)
2. Check if strategy code compiles
3. Confirm Lean engine path is correctly configured
4. View backend logs: `docker-compose logs api`

---

### Q6: Database Connection Failed

**Reasons**:
- PostgreSQL service not started
- Connection string error
- Insufficient permissions

**Solutions**:
1. Check PostgreSQL service: `sudo systemctl status postgresql`
2. Verify connection string parameters (Host, Port, username, password)
3. Test database connection: `psql -h localhost -U leanuser -d leanui`
4. Check user permissions: `GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;`

---

## Troubleshooting

### View Logs

#### Docker Deployment

```bash
# View all service logs
docker-compose logs

# View specific service logs
docker-compose logs api
docker-compose logs frontend

# Follow logs in real-time
docker-compose logs -f api
```

#### Local Deployment

- **Backend Logs**: `WebUI.API/logs/` directory
- **Frontend Logs**: Browser Console (F12)
- **Lean Engine Logs**: `data/logs/` directory

### Restart Services

#### Docker Deployment

```bash
# Restart all services
docker-compose restart

# Restart specific service
docker-compose restart api
```

#### Local Deployment

```bash
# Stop backend (Ctrl+C), then run again
cd WebUI.API
dotnet run

# Stop frontend (Ctrl+C), then run again
cd WebUI.Frontend
npm start
```

### Clean Data

```bash
# Docker deployment: Delete database volume (Warning: will delete all data!)
docker-compose down -v

# Local deployment: Delete database file
rm lean.db
```

### Health Checks

Visit the following URLs to check service status:
- API health check: `http://localhost:5000/health`
- Database connection: `http://localhost:5000/health/db`
- IBKR connection: `http://localhost:5000/health/ibkr`

---

## Contact Support

If you encounter issues that cannot be resolved, you can:

1. **View Documentation**: [WebUI Developer Guide](./WebUI-Developer-Guide.md), [API Reference](./WebUI-API-Reference.md)
2. **Submit Issue**: [GitHub Issues](https://github.com/QuantConnect/Lean/issues)
3. **Community Discussion**: [QuantConnect Forum](https://www.quantconnect.com/forum)

---

## License

Lean WebUI follows the Apache 2.0 open source license. See [LICENSE](../LICENSE) for details.

---

**Happy Trading!** 🚀
