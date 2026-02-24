# Lean WebUI - API Reference Documentation

Complete API reference for Lean WebUI backend services.

> **Related Documentation**:
> - Developer Guide (CN): [WebUI开发者完整指南.md](./WebUI开发者完整指南.md)  
> - Developer Guide (EN): [WebUI-Developer-Guide.md](./WebUI-Developer-Guide.md)
> - User Guide: [WebUI-User-Guide.md](./WebUI-User-Guide.md)

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication](#2-authentication)
3. [Trading APIs](#3-trading-apis)
4. [Strategy Management APIs](#4-strategy-management-apis)
5. [Account & Portfolio APIs](#5-account--portfolio-apis)
6. [Market Data APIs](#6-market-data-apis)
7. [Backtest APIs](#7-backtest-apis)
8. [Risk Control APIs](#8-risk-control-apis)
9. [System APIs](#9-system-apis)
10. [SignalR Hubs](#10-signalr-hubs)
11. [Error Codes](#11-error-codes)

---

## 1. Overview

### Base URL

**Development**: `http://localhost:5000/api/v1`  
**Production**: `https://yourdomain.com/api/v1`

### Authentication

All APIs (except public endpoints like `/auth/login`) require JWT Bearer token authentication:

```http
Authorization: Bearer <access_token>
```

### Request Format

- **Content-Type**: `application/json`
- **Character Encoding**: UTF-8
- **Date Format**: ISO 8601 (e.g., `2024-01-20T15:30:00Z`)

### Response Format

**Success Response**:
```json
{
  "data": { /* response data */ },
  "success": true,
  "message": null
}
```

**Error Response**:
```json
{
  "data": null,
  "success": false,
  "error": {
    "code": "INVALID_PARAMETER",
    "message": "Order quantity must be positive",
    "details": {
      "field": "quantity",
      "value": -10
    }
  }
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Request succeeded |
| 201 | Created | Resource created successfully |
| 204 | No Content | Request succeeded with no response body |
| 400 | Bad Request | Invalid parameters |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource conflict (e.g., duplicate) |
| 422 | Unprocessable Entity | Validation failed |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service temporarily unavailable |

### Rate Limiting

- **Standard Users**: 60 requests/minute per user
- **Rate Limit Headers**:
  - `X-RateLimit-Limit`: Maximum requests per minute
  - `X-RateLimit-Remaining`: Remaining requests in current window
  - `X-RateLimit-Reset`: Unix timestamp when limit resets

---

## 2. Authentication

### 2.1 Login

Authenticate user and obtain JWT tokens.

**Endpoint**: `POST /auth/login`

**Request Headers**:
- `Content-Type: application/json`

**Request Body**:
```json
{
  "username": "admin",
  "password": "YourPassword123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 900,
    "user": {
      "id": "user-123",
      "username": "admin",
      "email": "admin@example.com",
      "role": "admin"
    }
  }
}
```

**Error Codes**:
- `INVALID_CREDENTIALS`: Invalid username or password
- `ACCOUNT_LOCKED`: Account temporarily locked due to failed attempts
- `ACCOUNT_DISABLED`: Account has been disabled

**Example** (cURL):
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"YourPassword123!"}'
```

**Example** (C#):
```csharp
var client = new HttpClient();
var content = new StringContent(
    JsonSerializer.Serialize(new { username = "admin", password = "YourPassword123!" }),
    Encoding.UTF8,
    "application/json"
);
var response = await client.PostAsync("http://localhost:5000/api/v1/auth/login", content);
var result = await response.Content.ReadAsStringAsync();
```

**Example** (TypeScript):
```typescript
const response = await axios.post('/auth/login', {
  username: 'admin',
  password: 'YourPassword123!'
});
const { accessToken, refreshToken, user } = response.data.data;
```

---

### 2.2 Refresh Token

Refresh expired access token using refresh token.

**Endpoint**: `POST /auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900
  }
}
```

---

### 2.3 Logout

Revoke refresh tokens and logout user.

**Endpoint**: `POST /auth/logout`

**Request Headers**:
- `Authorization: Bearer <access_token>`

**Response** (204 No Content)

---

### 2.4 Change Password

Change user password (requires old password).

**Endpoint**: `POST /auth/change-password`

**Authentication**: Required

**Request Body**:
```json
{
  "oldPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

**Error Codes**:
- `INVALID_OLD_PASSWORD`: Old password is incorrect
- `PASSWORD_MISMATCH`: New password and confirm password don't match
- `WEAK_PASSWORD`: New password doesn't meet security requirements

---

## 3. Trading APIs

### 3.1 Place Order

Submit a new trading order.

**Endpoint**: `POST /trading/orders`

**Authentication**: Required

**Request Body**:
```json
{
  "symbol": "AAPL",
  "quantity": 100,
  "orderType": "limit",
  "direction": "buy",
  "limitPrice": 150.50,
  "stopPrice": null,
  "timeInForce": "day",
  "accountId": "DU12345"
}
```

**Field Descriptions**:
- `symbol` (string, required): Stock ticker symbol
- `quantity` (number, required): Order quantity (must be positive)
- `orderType` (string, required): Order type - `market`, `limit`, `stopMarket`, `stopLimit`
- `direction` (string, required): Order direction - `buy` or `sell`
- `limitPrice` (number, optional): Limit price (required for limit/stopLimit orders)
- `stopPrice` (number, optional): Stop price (required for stopMarket/stopLimit orders)
- `timeInForce` (string, optional): Time-in-force - `day` (default), `gtc` (Good-Till-Cancel), `ioc` (Immediate-Or-Cancel)
- `accountId` (string, required): IBKR account ID

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "orderId": "order-12345",
    "brokerId": "123456789",
    "symbol": "AAPL",
    "quantity": 100,
    "orderType": "limit",
    "direction": "buy",
    "limitPrice": 150.50,
    "status": "pending",
    "createdAt": "2024-01-20T10:30:00Z",
    "message": "Order submitted successfully"
  }
}
```

**Error Codes**:
- `INVALID_SYMBOL`: Invalid or unsupported ticker symbol
- `INSUFFICIENT_FUNDS`: Insufficient buying power
- `INVALID_QUANTITY`: Quantity must be positive and within limits
- `MARKET_CLOSED`: Market is currently closed
- `IBKR_CONNECTION_ERROR`: Failed to connect to IBKR

**Example** (cURL):
```bash
curl -X POST http://localhost:5000/api/v1/trading/orders \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "quantity": 100,
    "orderType": "limit",
    "direction": "buy",
    "limitPrice": 150.50,
    "accountId": "DU12345"
  }'
```

---

### 3.2 Get Orders

Retrieve user's orders (with pagination and filtering).

**Endpoint**: `GET /trading/orders`

**Authentication**: Required

**Query Parameters**:
- `page` (number, optional): Page number (default: 1)
- `pageSize` (number, optional): Items per page (default: 50, max: 200)
- `status` (string, optional): Filter by status - `pending`, `submitted`, `partiallyFilled`, `filled`, `canceled`, `rejected`
- `symbol` (string, optional): Filter by ticker symbol
- `startDate` (string, optional): Filter orders from this date (ISO 8601)
- `endDate` (string, optional): Filter orders until this date (ISO 8601)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "orders": [
      {
        "orderId": "order-12345",
        "brokerId": "123456789",
        "symbol": "AAPL",
        "quantity": 100,
        "filledQuantity": 50,
        "orderType": "limit",
        "direction": "buy",
        "limitPrice": 150.50,
        "averageFilledPrice": 150.25,
        "status": "partiallyFilled",
        "createdAt": "2024-01-20T10:30:00Z",
        "updatedAt": "2024-01-20T10:35:00Z",
        "statusMessage": "Partially filled: 50/100"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 50,
      "totalCount": 150,
      "totalPages": 3
    }
  }
}
```

**Example** (cURL):
```bash
curl -X GET "http://localhost:5000/api/v1/trading/orders?status=filled&symbol=AAPL&page=1&pageSize=20" \
  -H "Authorization: Bearer <token>"
```

---

### 3.3 Get Order Details

Retrieve detailed information about a specific order.

**Endpoint**: `GET /trading/orders/{orderId}`

**Authentication**: Required

**Path Parameters**:
- `orderId` (string, required): Order ID

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "orderId": "order-12345",
    "brokerId": "123456789",
    "symbol": "AAPL",
    "quantity": 100,
    "filledQuantity": 100,
    "orderType": "limit",
    "direction": "buy",
    "limitPrice": 150.50,
    "averageFilledPrice": 150.25,
    "status": "filled",
    "createdAt": "2024-01-20T10:30:00Z",
    "updatedAt": "2024-01-20T11:00:00Z",
    "filledAt": "2024-01-20T11:00:00Z",
    "statusMessage": "Order filled",
    "fills": [
      {
        "fillId": "fill-001",
        "quantity": 50,
        "price": 150.20,
        "timestamp": "2024-01-20T10:35:00Z",
        "commission": 0.50
      },
      {
        "fillId": "fill-002",
        "quantity": 50,
        "price": 150.30,
        "timestamp": "2024-01-20T11:00:00Z",
        "commission": 0.50
      }
    ]
  }
}
```

---

### 3.4 Cancel Order

Cancel a pending or partially filled order.

**Endpoint**: `DELETE /trading/orders/{orderId}`

**Authentication**: Required

**Path Parameters**:
- `orderId` (string, required): Order ID

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Order canceled successfully",
  "data": {
    "orderId": "order-12345",
    "status": "canceled",
    "canceledAt": "2024-01-20T11:15:00Z"
  }
}
```

**Error Codes**:
- `ORDER_NOT_FOUND`: Order does not exist
- `CANNOT_CANCEL`: Order is already filled or canceled
- `IBKR_ERROR`: Failed to cancel order at broker

---

### 3.5 Modify Order

Modify a pending order's price or quantity.

**Endpoint**: `PUT /trading/orders/{orderId}`

**Authentication**: Required

**Path Parameters**:
- `orderId` (string, required): Order ID

**Request Body**:
```json
{
  "quantity": 150,
  "limitPrice": 151.00
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Order modified successfully",
  "data": {
    "orderId": "order-12345",
    "quantity": 150,
    "limitPrice": 151.00,
    "status": "pending",
    "updatedAt": "2024-01-20T11:20:00Z"
  }
}
```

---

## 4. Strategy Management APIs

### 4.1 Create Strategy

Create a new trading strategy.

**Endpoint**: `POST /strategies`

**Authentication**: Required

**Request Body**:
```json
{
  "name": "Mean Reversion Strategy",
  "description": "Buy oversold stocks and sell overbought stocks",
  "algorithmCode": "class MeanReversionAlgorithm : QCAlgorithm\n{\n    // Strategy code here\n}",
  "type": "intraday",
  "parameters": {
    "rsiPeriod": 14,
    "oversoldThreshold": 30,
    "overboughtThreshold": 70
  },
  "symbols": ["AAPL", "MSFT", "GOOGL"],
  "initialBalance": 100000
}
```

**Field Descriptions**:
- `name` (string, required): Strategy name (max 100 chars)
- `description` (string, optional): Strategy description (max 500 chars)
- `algorithmCode` (string, required): C# algorithm code (Lean QCAlgorithm)
- `type` (string, required): Strategy type - `intraday`, `swing`, `scalpTrading`, `position`
- `parameters` (object, optional): Strategy parameters as key-value pairs
- `symbols` (array, required): List of ticker symbols to trade
- `initialBalance` (number, required): Initial capital for backtesting

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "strategyId": 1,
    "name": "Mean Reversion Strategy",
    "type": "intraday",
    "status": "inactive",
    "createdAt": "2024-01-20T12:00:00Z",
    "userId": "user-123"
  }
}
```

**Error Codes**:
- `INVALID_ALGORITHM_CODE`: Algorithm code contains syntax errors
- `DUPLICATE_STRATEGY_NAME`: Strategy name already exists for this user
- `INVALID_SYMBOLS`: One or more ticker symbols are invalid

---

### 4.2 Get Strategies

Retrieve user's strategies.

**Endpoint**: `GET /strategies`

**Authentication**: Required

**Query Parameters**:
- `page` (number, optional): Page number
- `pageSize` (number, optional): Items per page
- `status` (string, optional): Filter by status - `inactive`, `running`, `stopped`, `error`
- `type` (string, optional): Filter by type

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "strategies": [
      {
        "strategyId": 1,
        "name": "Mean Reversion Strategy",
        "type": "intraday",
        "status": "running",
        "createdAt": "2024-01-20T12:00:00Z",
        "startedAt": "2024-01-20T12:30:00Z",
        "statistics": {
          "trades": 45,
          "winRate": 0.65,
          "profitLoss": 2500.50
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 15,
      "totalPages": 1
    }
  }
}
```

---

### 4.3 Start Strategy

Start executing a strategy in live trading.

**Endpoint**: `POST /strategies/{strategyId}/start`

**Authentication**: Required

**Path Parameters**:
- `strategyId` (number, required): Strategy ID

**Request Body**:
```json
{
  "accountId": "DU12345",
  "mode": "live"
}
```

**Field Descriptions**:
- `accountId` (string, required): IBKR account ID
- `mode` (string, required): Execution mode - `live` or `paper`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Strategy started successfully",
  "data": {
    "strategyId": 1,
    "status": "running",
    "startedAt": "2024-01-20T13:00:00Z"
  }
}
```

**Error Codes**:
- `STRATEGY_ALREADY_RUNNING`: Strategy is already running
- `IBKR_NOT_CONNECTED`: IBKR connection not established
- `INSUFFICIENT_FUNDS`: Account has insufficient balance

---

### 4.4 Stop Strategy

Stop a running strategy.

**Endpoint**: `POST /strategies/{strategyId}/stop`

**Authentication**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Strategy stopped successfully",
  "data": {
    "strategyId": 1,
    "status": "stopped",
    "stoppedAt": "2024-01-20T16:00:00Z",
    "finalStatistics": {
      "trades": 67,
      "winRate": 0.68,
      "profitLoss": 3250.75,
      "sharpeRatio": 1.85
    }
  }
}
```

---

### 4.5 Delete Strategy

Delete a strategy (only if not running).

**Endpoint**: `DELETE /strategies/{strategyId}`

**Authentication**: Required

**Response** (204 No Content)

---

## 5. Account & Portfolio APIs

### 5.1 Get Account Info

Retrieve IBKR account information.

**Endpoint**: `GET /accounts/{accountId}`

**Authentication**: Required

**Path Parameters**:
- `accountId` (string, required): IBKR account ID

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accountId": "DU12345",
    "accountType": "INDIVIDUAL",
    "currency": "USD",
    "netLiquidation": 105250.75,
    "totalCashValue": 45000.00,
    "grossPositionValue": 60250.75,
    "availableFunds": 35000.00,
    "buyingPower": 140000.00,
    "unrealizedPnL": 2250.75,
    "realizedPnL": 3000.00,
    "maintenanceMargin": 10250.00,
    "lastUpdated": "2024-01-20T16:30:00Z"
  }
}
```

---

### 5.2 Get Positions

Retrieve current open positions.

**Endpoint**: `GET /accounts/{accountId}/positions`

**Authentication**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "positions": [
      {
        "symbol": "AAPL",
        "quantity": 200,
        "averagePrice": 148.50,
        "currentPrice": 151.25,
        "marketValue": 30250.00,
        "unrealizedPnL": 550.00,
        "unrealizedPnLPercent": 0.0185,
        "costBasis": 29700.00,
        "lastUpdated": "2024-01-20T16:30:00Z"
      },
      {
        "symbol": "MSFT",
        "quantity": 100,
        "averagePrice": 380.00,
        "currentPrice": 385.50,
        "marketValue": 38550.00,
        "unrealizedPnL": 550.00,
        "unrealizedPnLPercent": 0.0145,
        "costBasis": 38000.00,
        "lastUpdated": "2024-01-20T16:30:00Z"
      }
    ],
    "summary": {
      "totalValue": 68800.00,
      "totalUnrealizedPnL": 1100.00,
      "positionCount": 2
    }
  }
}
```

---

### 5.3 Get Trade History

Retrieve historical trades.

**Endpoint**: `GET /accounts/{accountId}/trades`

**Authentication**: Required

**Query Parameters**:
- `startDate` (string, optional): Start date (ISO 8601)
- `endDate` (string, optional): End date
- `symbol` (string, optional): Filter by symbol
- `page` (number, optional): Page number
- `pageSize` (number, optional): Items per page

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "trades": [
      {
        "tradeId": "trade-001",
        "orderId": "order-12345",
        "symbol": "AAPL",
        "quantity": 100,
        "direction": "buy",
        "price": 150.25,
        "amount": 15025.00,
        "commission": 1.00,
        "executedAt": "2024-01-20T10:35:00Z"
      }
    ],
    "summary": {
      "totalTrades": 150,
      "totalCommissions": 150.00,
      "totalVolume": 52500.00
    },
    "pagination": {
      "page": 1,
      "pageSize": 50,
      "totalCount": 150,
      "totalPages": 3
    }
  }
}
```

---

## 6. Market Data APIs

### 6.1 Get Quote

Get real-time quote for a symbol.

**Endpoint**: `GET /marketdata/quote/{symbol}`

**Authentication**: Required

**Path Parameters**:
- `symbol` (string, required): Ticker symbol

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "symbol": "AAPL",
    "lastPrice": 151.25,
    "bidPrice": 151.20,
    "askPrice": 151.30,
    "bidSize": 100,
    "askSize": 200,
    "volume": 52350000,
    "open": 150.00,
    "high": 152.00,
    "low": 149.50,
    "previousClose": 150.50,
    "change": 0.75,
    "changePercent": 0.50,
    "timestamp": "2024-01-20T16:00:00Z"
  }
}
```

---

### 6.2 Get Historical Bars

Get historical OHLCV data.

**Endpoint**: `GET /marketdata/history/{symbol}`

**Authentication**: Required

**Path Parameters**:
- `symbol` (string, required): Ticker symbol

**Query Parameters**:
- `resolution` (string, required): Bar resolution - `1min`, `5min`, `15min`, `1hour`, `1day`
- `startDate` (string, required): Start date (ISO 8601)
- `endDate` (string, optional): End date (default: now)
- `limit` (number, optional): Max bars to return (default: 500, max: 5000)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "symbol": "AAPL",
    "resolution": "1day",
    "bars": [
      {
        "timestamp": "2024-01-15T00:00:00Z",
        "open": 148.50,
        "high": 150.00,
        "low": 147.00,
        "close": 149.50,
        "volume": 45000000
      },
      {
        "timestamp": "2024-01-16T00:00:00Z",
        "open": 149.75,
        "high": 151.50,
        "low": 149.00,
        "close": 150.50,
        "volume": 48000000
      }
    ],
    "count": 2
  }
}
```

---

## 7. Backtest APIs

### 7.1 Run Backtest

Execute a backtest for a strategy.

**Endpoint**: `POST /backtest`

**Authentication**: Required

**Request Body**:
```json
{
  "strategyId": 1,
  "startDate": "2023-01-01T00:00:00Z",
  "endDate": "2023-12-31T23:59:59Z",
  "initialCapital": 100000,
  "resolution": "minute"
}
```

**Response** (202 Accepted):
```json
{
  "success": true,
  "data": {
    "backtestId": "backtest-abc123",
    "status": "running",
    "startedAt": "2024-01-20T14:00:00Z",
    "estimatedDuration": 120
  }
}
```

---

### 7.2 Get Backtest Status

Check backtest execution status.

**Endpoint**: `GET /backtest/{backtestId}`

**Authentication**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "backtestId": "backtest-abc123",
    "strategyId": 1,
    "status": "completed",
    "startedAt": "2024-01-20T14:00:00Z",
    "completedAt": "2024-01-20T14:02:15Z",
    "duration": 135,
    "results": {
      "totalTrades": 234,
      "winningTrades": 156,
      "losingTrades": 78,
      "winRate": 0.6667,
      "totalReturn": 15.25,
      "annualizedReturn": 15.25,
      "sharpeRatio": 1.85,
      "maxDrawdown": -8.5,
      "profitFactor": 2.15,
      "totalCommissions": 234.00,
      "netProfit": 15250.00
    },
    "equity Curve": [
      {
        "date": "2023-01-01",
        "equity": 100000
      },
      {
        "date": "2023-01-02",
        "equity": 100250
      }
    ]
  }
}
```

---

## 8. Risk Control APIs

### 8.1 Get Risk Settings

Retrieve current risk control settings.

**Endpoint**: `GET /risk/settings`

**Authentication**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "maxDailyLoss": 5000,
    "maxDailyLossPercent": 5.0,
    "maxPositionSize": 10000,
    "maxPositionSizePercent": 10.0,
    "maxOrdersPerDay": 100,
    "enableStopLoss": true,
    "defaultStopLossPercent": 2.0,
    "enableTakeProfit": true,
    "defaultTakeProfitPercent": 5.0
  }
}
```

---

### 8.2 Update Risk Settings

Update risk control settings.

**Endpoint**: `PUT /risk/settings`

**Authentication**: Required

**Request Body**:
```json
{
  "maxDailyLoss": 3000,
  "maxDailyLossPercent": 3.0,
  "enableStopLoss": true,
  "defaultStopLossPercent": 1.5
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Risk settings updated successfully"
}
```

---

## 9. System APIs

### 9.1 Health Check

Check system health.

**Endpoint**: `GET /health`

**Authentication**: Not Required

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-20T16:30:00Z"
}
```

---

### 9.2 Detailed Health Check

Get detailed health information for all components.

**Endpoint**: `GET /health/detailed`

**Authentication**: Required (Admin only)

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "components": {
    "database": {
      "status": "Healthy",
      "responseTime": 12
    },
    "ibkrConnection": {
      "status": "Healthy",
      "connected": true,
      "accountId": "DU12345"
    },
    "redis": {
      "status": "Healthy",
      "responseTime": 5
    }
  },
  "timestamp": "2024-01-20T16:30:00Z"
}
```

---

### 9.3 Get API Version

Get current API version information.

**Endpoint**: `GET /version`

**Authentication**: Not Required

**Response** (200 OK):
```json
{
  "version": "1.0.0",
  "apiVersion": "v1",
  "buildDate": "2024-01-15",
  "environment": "production"
}
```

---

## 10. SignalR Hubs

SignalR provides real-time bidirectional communication between client and server.

**Connection URL**: `ws://localhost:5000/hubs/{hubName}`

**Authentication**: Include JWT token in connection:
```typescript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/marketdata", {
        accessTokenFactory: () => accessToken
    })
    .build();
```

### 10.1 Market Data Hub

**Hub URL**: `/hubs/marketdata`

#### Subscribe to Symbol

**Method**: `SubscribeToSymbol`

**Parameters**:
- `symbol` (string): Ticker symbol

**Client Method**: `ReceiveQuote`

**Data**:
```json
{
  "symbol": "AAPL",
  "lastPrice": 151.25,
  "bidPrice": 151.20,
  "askPrice": 151.30,
  "volume": 52350000,
  "timestamp": "2024-01-20T16:00:15.123Z"
}
```

**Example**:
```typescript
// Subscribe
await connection.invoke("SubscribeToSymbol", "AAPL");

// Receive updates
connection.on("ReceiveQuote", (data) => {
    console.log("Quote update:", data);
});

// Unsubscribe
await connection.invoke("UnsubscribeFromSymbol", "AAPL");
```

---

### 10.2 Order Hub

**Hub URL**: `/hubs/orders`

#### Subscribe to Order Updates

**Method**: `SubscribeToOrders`

**Client Method**: `ReceiveOrderUpdate`

**Data**:
```json
{
  "orderId": "order-12345",
  "status": "filled",
  "filledQuantity": 100,
  "averageFilledPrice": 150.25,
  "timestamp": "2024-01-20T10:35:00Z",
  "message": "Order filled"
}
```

---

### 10.3 Strategy Hub

**Hub URL**: `/hubs/strategy`

#### Subscribe to Strategy Logs

**Method**: `SubscribeToStrategy`

**Parameters**:
- `strategyId` (number): Strategy ID

**Client Method**: `ReceiveStrategyLog`

**Data**:
```json
{
  "strategyId": 1,
  "level": "info",
  "message": "Order placed: Buy 100 AAPL @ 150.50",
  "timestamp": "2024-01-20T10:30:15.456Z"
}
```

---

## 11. Error Codes

### Authentication Errors

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `INVALID_CREDENTIALS` | 401 | Invalid username or password |
| `TOKEN_EXPIRED` | 401 | Access token has expired |
| `INVALID_TOKEN` | 401 | Token is malformed or invalid |
| `REFRESH_TOKEN_EXPIRED` | 401 | Refresh token has expired |
| `ACCOUNT_LOCKED` | 403 | Account temporarily locked |
| `ACCOUNT_DISABLED` | 403 | Account has been disabled |
| `INSUFFICIENT_PERMISSIONS` | 403 | User lacks required permissions |

### Trading Errors

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `INVALID_SYMBOL` | 400 | Invalid or unsupported ticker symbol |
| `INVALID_QUANTITY` | 400 | Order quantity is invalid |
| `INVALID_ORDER_TYPE` | 400 | Order type is not supported |
| `INSUFFICIENT_FUNDS` | 400 | Insufficient buying power |
| `MARKET_CLOSED` | 400 | Market is currently closed |
| `ORDER_NOT_FOUND` | 404 | Order does not exist |
| `CANNOT_CANCEL` | 409 | Order cannot be canceled (filled/canceled) |
| `IBKR_CONNECTION_ERROR` | 503 | Failed to connect to IBKR |
| `IBKR_ERROR` | 500 | IBKR returned an error |

### Strategy Errors

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `STRATEGY_NOT_FOUND` | 404 | Strategy does not exist |
| `INVALID_ALGORITHM_CODE` | 400 | Algorithm code contains errors |
| `DUPLICATE_STRATEGY_NAME` | 409 | Strategy name already exists |
| `STRATEGY_ALREADY_RUNNING` | 409 | Strategy is already running |
| `STRATEGY_NOT_RUNNING` | 400 | Strategy is not currently running |
| `COMPILATION_ERROR` | 400 | Failed to compile algorithm code |

### System Errors

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `INTERNAL_ERROR` | 500 | Internal server error |
| `SERVICE_UNAVAILABLE` | 503 | Service temporarily unavailable |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `VALIDATION_ERROR` | 422 | Input validation failed |
| `DATABASE_ERROR` | 500 | Database operation failed |

---

## Appendix

### A. Request/Response Examples

#### Complete Order Flow Example

**1. Login**:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Pass123!"}'
```

**2. Place Order**:
```bash
curl -X POST http://localhost:5000/api/v1/trading/orders \
  -H "Authorization: Bearer eyJhbG..." \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "quantity": 100,
    "orderType": "limit",
    "direction": "buy",
    "limitPrice": 150.50,
    "accountId": "DU12345"
  }'
```

**3. Check Order Status**:
```bash
curl -X GET http://localhost:5000/api/v1/trading/orders/order-12345 \
  -H "Authorization: Bearer eyJhbG..."
```

---

### B. SDK Examples

#### C# SDK Usage

```csharp
using LeanWebUI.SDK;

var client = new LeanWebUIClient("http://localhost:5000");

// Login
var loginResult = await client.Auth.LoginAsync("admin", "Pass123!");
client.SetToken(loginResult.AccessToken);

// Place order
var order = await client.Trading.PlaceOrderAsync(new PlaceOrderRequest
{
    Symbol = "AAPL",
    Quantity = 100,
    OrderType = OrderType.Limit,
    Direction = OrderDirection.Buy,
    LimitPrice = 150.50m,
    AccountId = "DU12345"
});

Console.WriteLine($"Order placed: {order.OrderId}, Status: {order.Status}");
```

#### TypeScript SDK Usage

```typescript
import { LeanWebUIClient } from 'lean-webui-sdk';

const client = new LeanWebUIClient('http://localhost:5000');

// Login
const loginResult = await client.auth.login('admin', 'Pass123!');
client.setToken(loginResult.accessToken);

// Place order
const order = await client.trading.placeOrder({
    symbol: 'AAPL',
    quantity: 100,
    orderType: 'limit',
    direction: 'buy',
    limitPrice: 150.50,
    accountId: 'DU12345'
});

console.log(`Order placed: ${order.orderId}, Status: ${order.status}`);
```

---

### C. Postman Collection

Import the Postman collection to quickly test all API endpoints:

**Collection URL**: `http://localhost:5000/api/v1/swagger/v1/swagger.json`

Or download from: [LeanWebUI-API-Collection.json](./LeanWebUI-API-Collection.json)

---

**For additional support, please refer to**:
- [WebUI Developer Guide](./WebUI-Developer-Guide.md)
- [User Guide](./WebUI-User-Guide.md)
- [GitHub Issues](https://github.com/QuantConnect/Lean/issues)

