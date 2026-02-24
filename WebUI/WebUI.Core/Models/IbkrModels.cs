/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace WebUI.Core.Models
{
    /// <summary>
    /// Connection status for IBKR TWS/Gateway
    /// </summary>
    public enum IbkrConnectionStatus
    {
        /// <summary>
        /// Not connected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Currently connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Successfully connected
        /// </summary>
        Connected,

        /// <summary>
        /// Connection lost, attempting to reconnect
        /// </summary>
        Reconnecting,

        /// <summary>
        /// Connection error occurred
        /// </summary>
        Error
    }

    /// <summary>
    /// Account type for IBKR
    /// </summary>
    public enum IbkrAccountType
    {
        /// <summary>
        /// Paper trading account
        /// </summary>
        Paper,

        /// <summary>
        /// Live trading account
        /// </summary>
        Live
    }

    /// <summary>
    /// IBKR connection configuration
    /// </summary>
    public class IbkrConnectionConfig
    {
        /// <summary>
        /// TWS/Gateway host (default: localhost)
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// TWS/Gateway port (7496 for live, 7497 for paper)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// IBKR account ID
        /// </summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Account type (Paper or Live)
        /// </summary>
        public IbkrAccountType AccountType { get; set; }

        /// <summary>
        /// Connection timeout in seconds (default: 10)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Enable auto-reconnect on disconnect
        /// </summary>
        public bool EnableAutoReconnect { get; set; } = true;

        /// <summary>
        /// Maximum reconnection attempts (default: 10)
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 10;
    }

    /// <summary>
    /// IBKR connection state information
    /// </summary>
    public class IbkrConnectionState
    {
        /// <summary>
        /// Current connection status
        /// </summary>
        public IbkrConnectionStatus Status { get; set; }

        /// <summary>
        /// Account ID
        /// </summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Account type
        /// </summary>
        public IbkrAccountType AccountType { get; set; }

        /// <summary>
        /// TWS/Gateway version
        /// </summary>
        public string TwsVersion { get; set; } = string.Empty;

        /// <summary>
        /// API version
        /// </summary>
        public string ApiVersion { get; set; } = string.Empty;

        /// <summary>
        /// Connection time (UTC)
        /// </summary>
        public DateTime? ConnectedAt { get; set; }

        /// <summary>
        /// Last heartbeat time (UTC)
        /// </summary>
        public DateTime? LastHeartbeatAt { get; set; }

        /// <summary>
        /// Time elapsed since connection (seconds)
        /// </summary>
        public int? ConnectionDurationSeconds => ConnectedAt.HasValue 
            ? (int)(DateTime.UtcNow - ConnectedAt.Value).TotalSeconds 
            : null;

        /// <summary>
        /// Current reconnection attempt count
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// Next reconnect delay in seconds (exponential backoff)
        /// </summary>
        public int? NextReconnectDelaySeconds { get; set; }

        /// <summary>
        /// Last error message
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// Is currently healthy (connected and recent heartbeat)
        /// </summary>
        public bool IsHealthy => Status == IbkrConnectionStatus.Connected 
            && LastHeartbeatAt.HasValue 
            && (DateTime.UtcNow - LastHeartbeatAt.Value).TotalSeconds < 30;
    }

    /// <summary>
    /// IBKR account summary information
    /// </summary>
    public class IbkrAccountSummary
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Account type
        /// </summary>
        public IbkrAccountType AccountType { get; set; }

        /// <summary>
        /// Base currency (e.g., USD)
        /// </summary>
        public string BaseCurrency { get; set; } = "USD";

        /// <summary>
        /// Total cash balance
        /// </summary>
        public decimal CashBalance { get; set; }

        /// <summary>
        /// Net liquidation value (total account value)
        /// </summary>
        public decimal NetLiquidation { get; set; }

        /// <summary>
        /// Available funds for trading
        /// </summary>
        public decimal AvailableFunds { get; set; }

        /// <summary>
        /// Buying power
        /// </summary>
        public decimal BuyingPower { get; set; }

        /// <summary>
        /// Gross position value (long positions)
        /// </summary>
        public decimal GrossPositionValue { get; set; }

        /// <summary>
        /// Margin requirement
        /// </summary>
        public decimal MarginRequirement { get; set; }

        /// <summary>
        /// Excess liquidity
        /// </summary>
        public decimal ExcessLiquidity { get; set; }

        /// <summary>
        /// Last update time (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// IBKR connection diagnostics information
    /// </summary>
    public class IbkrDiagnostics
    {
        /// <summary>
        /// Connection state
        /// </summary>
        public IbkrConnectionState ConnectionState { get; set; } = new IbkrConnectionState();

        /// <summary>
        /// Recent error history (last 50 errors)
        /// </summary>
        public List<IbkrErrorRecord> ErrorHistory { get; set; } = new List<IbkrErrorRecord>();

        /// <summary>
        /// Connection configuration (sensitive data masked)
        /// </summary>
        public IbkrConnectionConfig? Configuration { get; set; }

        /// <summary>
        /// API rate limit status
        /// </summary>
        public IbkrRateLimitStatus RateLimitStatus { get; set; } = new IbkrRateLimitStatus();
    }

    /// <summary>
    /// IBKR error record
    /// </summary>
    public class IbkrErrorRecord
    {
        /// <summary>
        /// Error timestamp (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// IBKR error code
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Additional details
        /// </summary>
        public string? Details { get; set; }
    }

    /// <summary>
    /// API rate limit status
    /// </summary>
    public class IbkrRateLimitStatus
    {
        /// <summary>
        /// Current requests per second
        /// </summary>
        public int CurrentRequestsPerSecond { get; set; }

        /// <summary>
        /// Maximum allowed requests per second (IBKR limit: 50)
        /// </summary>
        public int MaxRequestsPerSecond { get; set; } = 50;

        /// <summary>
        /// Number of queued requests
        /// </summary>
        public int QueuedRequests { get; set; }

        /// <summary>
        /// Is currently throttled
        /// </summary>
        public bool IsThrottled => CurrentRequestsPerSecond >= MaxRequestsPerSecond;
    }
}
