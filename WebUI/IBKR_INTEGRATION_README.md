# IBKR Integration Module

## Overview

The IBKR (Interactive Brokers) Integration module provides secure connectivity between the Lean WebUI and Interactive Brokers TWS (Trader Workstation) or Gateway. This module handles connection management, account queries, health monitoring, and secure credential storage.

## Features

### ✅ Implemented

1. **Connection Management** (`IbkrConnectionService`)
   - Connect to TWS/Gateway (ports 7496 for live, 7497 for paper trading)
   - Graceful disconnection
   - Connection state tracking
   - Connection timeout handling (default: 10 seconds)

2. **Health Monitoring**
   - Heartbeat checks every 10 seconds
   - Connection health status (`IsHealthy`)
   - Last heartbeat timestamp tracking

3. **Auto-Reconnect Logic**
   - Exponential backoff: 1s → 2s → 4s → 8s → ... → max 60s
   - Configurable max reconnection attempts (default: 10)
   - Automatic retry on connection lost

4. **Credential Encryption** (`CredentialEncryptionService`)
   - **Windows**: DPAPI (Data Protection API) encryption
   - **Linux/macOS**: AES-256-GCM encryption with machine-specific key derivation
   - Secure storage of IBKR credentials

5. **Account Queries**
   - Account summary (balance, net liquidation, buying power)
   - Available funds and margin requirements
   - Paper trading vs. live account distinction

6. **REST API Endpoints** (`IbkrController`)
   - `GET /api/v1/ibkr/status` - Connection status
   - `POST /api/v1/ibkr/connect` - Connect to IBKR
   - `POST /api/v1/ibkr/disconnect` - Disconnect from IBKR
   - `GET /api/v1/ibkr/account` - Account summary
   - `GET /api/v1/ibkr/account/balance` - Balance and buying power
   - `GET /api/v1/ibkr/diagnostics` - Connection diagnostics
   - `GET /api/v1/ibkr/health` - Health check endpoint

7. **Rate Limiting**
   - Respects IBKR's 50 requests/second limit
   - Request queuing when limit exceeded
   - Real-time rate monitoring

8. **Error Handling**
   - Error history tracking (last 50 errors)
   - Detailed error records with timestamps and IBKR error codes
   - Comprehensive diagnostics information

9. **Validation** (`IbkrConnectionConfigValidator`)
   - FluentValidation for connection configuration
   - Port validation for account types
   - Timeout and reconnect attempt validation

10. **Unit Tests**
    - Comprehensive test coverage for `IbkrConnectionService`
    - Comprehensive test coverage for `CredentialEncryptionService`
    - Mock-based testing using Moq and xUnit

## Architecture

```
┌─────────────────┐
│  IBKR Controller│  (API Endpoints)
└────────┬────────┘
         │
         ▼
┌─────────────────────┐
│IbkrConnectionService│  (Connection Management)
└────────┬────────────┘
         │
         ├──► Heartbeat Monitoring (10s interval)
         ├──► Auto-Reconnect (Exponential Backoff)
         ├──► Rate Limiting (50 req/s max)
         └──► Error History (Last 50 errors)
         
┌──────────────────────┐
│CredentialEncryption  │  (Secure Storage)
│       Service        │
└──────────────────────┘
         │
         ├──► Windows: DPAPI
         └──► Linux/macOS: AES-256-GCM
```

## Usage

### 1. Registering Services

In `Program.cs` or startup configuration:

```csharp
// Register IBKR services
builder.Services.AddSingleton<ICredentialEncryptionService, CredentialEncryptionService>();
builder.Services.AddSingleton<IIbkrConnectionService, IbkrConnectionService>();

// Register validators
builder.Services.AddScoped<IValidator<IbkrConnectionConfig>, IbkrConnectionConfigValidator>();
```

### 2. Connecting to IBKR

**Paper Trading (Port 7497):**
```bash
POST /api/v1/ibkr/connect
Content-Type: application/json

{
  "host": "localhost",
  "port": 7497,
  "accountId": "DU1234567",
  "accountType": "Paper",
  "timeoutSeconds": 10,
  "enableAutoReconnect": true,
  "maxReconnectAttempts": 10
}
```

**Live Trading (Port 7496):**
```bash
POST /api/v1/ibkr/connect
Content-Type: application/json

{
  "host": "localhost",
  "port": 7496,
  "accountId": "U1234567",
  "accountType": "Live",  ⚠️ WARNING: Live account!
  "timeoutSeconds": 10,
  "enableAutoReconnect": true,
  "maxReconnectAttempts": 10
}
```

### 3. Querying Account Information

**Get Account Summary:**
```bash
GET /api/v1/ibkr/account
Authorization: Bearer <jwt-token>
```

Response:
```json
{
  "accountId": "DU1234567",
  "accountType": "Paper",
  "baseCurrency": "USD",
  "cashBalance": 100000.00,
  "netLiquidation": 100000.00,
  "availableFunds": 100000.00,
  "buyingPower": 400000.00,
  "grossPositionValue": 0.00,
  "marginRequirement": 0.00,
  "excessLiquidity": 100000.00,
  "updatedAt": "2026-02-15T10:30:00Z"
}
```

### 4. Monitoring Connection Status

```bash
GET /api/v1/ibkr/status
Authorization: Bearer <jwt-token>
```

Response:
```json
{
  "status": "Connected",
  "accountId": "DU1234567",
  "accountType": "Paper",
  "twsVersion": "10.19",
  "apiVersion": "9.88",
  "connectedAt": "2026-02-15T10:00:00Z",
  "lastHeartbeatAt": "2026-02-15T10:30:00Z",
  "connectionDurationSeconds": 1800,
  "reconnectAttempts": 0,
  "isHealthy": true
}
```

### 5. Diagnostics

```bash
GET /api/v1/ibkr/diagnostics
Authorization: Bearer <jwt-token>
```

Response includes:
- Connection state
- Error history (last 50 errors)
- Rate limit status
- Configuration (with masked credentials)

## Security Considerations

1. **Credential Encryption**
   - All IBKR credentials are encrypted at rest
   - Windows: Uses user-scoped DPAPI
   - Linux/macOS: Uses AES-256-GCM with machine-specific key

2. **Account ID Masking**
   - Account IDs are masked in diagnostic outputs (show only last 4 digits)
   - Example: `DU1234567` → `****4567`

3. **Live Account Warning**
   - Live account connections log warnings
   - API validates account type matches port number

4. **JWT Authorization**
   - All endpoints (except health check) require JWT authentication
   - Bearer token must be provided in Authorization header

## Configuration

### Connection Config Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Host` | string | "localhost" | TWS/Gateway host |
| `Port` | int | - | TWS/Gateway port (7496=live, 7497=paper) |
| `AccountId` | string | - | IBKR account ID |
| `AccountType` | enum | - | Paper or Live |
| `TimeoutSeconds` | int | 10 | Connection timeout |
| `EnableAutoReconnect` | bool | true | Enable auto-reconnect |
| `MaxReconnectAttempts` | int | 10 | Max reconnection attempts |

### Reconnection Backoff Schedule

| Attempt | Delay |
|---------|-------|
| 1 | 1 second |
| 2 | 2 seconds |
| 3 | 4 seconds |
| 4 | 8 seconds |
| 5 | 16 seconds |
| 6 | 32 seconds |
| 7+ | 60 seconds |

## Testing

Run the IBKR integration tests:

```bash
dotnet test WebUI.Tests --filter "FullyQualifiedName~IbkrConnectionServiceTests"
dotnet test WebUI.Tests --filter "FullyQualifiedName~CredentialEncryptionServiceTests"
```

## Known Limitations

1. **IPC Interface**: Task 5.1 (extending Lean's IBrokerageHandler for IPC) is currently a placeholder. The actual IPC communication with Lean engine needs to be implemented when Lean is extended to expose a Named Pipe or TCP interface.

2. **Simulated Connection**: The current implementation simulates successful connections for development purposes. Production deployment requires actual integration with Lean's IBKR brokerage handler.

3. **Single Connection**: Only one IBKR connection is supported at a time. Multi-account support is not yet implemented.

## Next Steps

To complete the IBKR integration:

1. **Extend Lean Engine**:
   - Add IPC interface (Named Pipe or TCP) to Lean's IBrokerageHandler
   - Expose methods for querying connection status, account info, positions, and orders
   - Implement bi-directional communication for real-time updates

2. **SignalR Integration**:
   - Push connection status changes to frontend via SignalR
   - Push account balance updates in real-time
   - Implement subscription-based updates

3. **Frontend Components**:
   - IBKR connection status indicator
   - Connection configuration UI
   - Account balance display
   - Diagnostics dashboard

## References

- [Interactive Brokers API Documentation](https://interactivebrokers.github.io/tws-api/)
- [Lean IBKR Brokerage Implementation](../../Brokerages/)
- [IBKR Integration Spec](../../openspec/changes/personal-trading-webui/specs/ibkr-integration/spec.md)

## Support

For issues or questions:
1. Check the diagnostics endpoint (`GET /api/v1/ibkr/diagnostics`)
2. Review error history in diagnostics output
3. Verify TWS/Gateway is running and API access is enabled
4. Confirm correct port (7496 for live, 7497 for paper)
5. Check that account ID is valid and matches account type
