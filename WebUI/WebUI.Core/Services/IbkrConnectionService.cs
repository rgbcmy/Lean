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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Services
{
    /// <summary>
    /// Interface for IBKR connection management service
    /// </summary>
    public interface IIbkrConnectionService
    {
        /// <summary>
        /// Current connection state
        /// </summary>
        IbkrConnectionState ConnectionState { get; }

        /// <summary>
        /// Connect to IBKR TWS/Gateway
        /// </summary>
        Task<bool> ConnectAsync(IbkrConnectionConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnect from IBKR
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Get account summary
        /// </summary>
        Task<IbkrAccountSummary?> GetAccountSummaryAsync();

        /// <summary>
        /// Get connection diagnostics
        /// </summary>
        IbkrDiagnostics GetDiagnostics();

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        event EventHandler<IbkrConnectionState>? ConnectionStateChanged;
    }

    /// <summary>
    /// IBKR connection management service with health monitoring and auto-reconnect
    /// </summary>
    public class IbkrConnectionService : BackgroundService, IIbkrConnectionService
    {
        private readonly ILogger<IbkrConnectionService> _logger;
        private IbkrConnectionConfig? _config;
        private IbkrConnectionState _connectionState;
        private readonly ConcurrentQueue<IbkrErrorRecord> _errorHistory;
        private Timer? _heartbeatTimer;
        private Timer? _reconnectTimer;
        private readonly SemaphoreSlim _connectionLock;
        private CancellationTokenSource? _healthCheckCts;
        private int _reconnectAttempts;

        // Rate limiting
        private readonly ConcurrentQueue<DateTime> _requestTimestamps;
        private const int MaxRequestsPerSecond = 50;

        /// <summary>
        /// Current connection state
        /// </summary>
        public IbkrConnectionState ConnectionState => _connectionState;

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        public event EventHandler<IbkrConnectionState>? ConnectionStateChanged;

        public IbkrConnectionService(ILogger<IbkrConnectionService> logger)
        {
            _logger = logger;
            _connectionState = new IbkrConnectionState
            {
                Status = IbkrConnectionStatus.Disconnected
            };
            _errorHistory = new ConcurrentQueue<IbkrErrorRecord>();
            _connectionLock = new SemaphoreSlim(1, 1);
            _requestTimestamps = new ConcurrentQueue<DateTime>();
        }

        /// <summary>
        /// Connect to IBKR TWS/Gateway
        /// </summary>
        public async Task<bool> ConnectAsync(IbkrConnectionConfig config, CancellationToken cancellationToken = default)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Attempting to connect to IBKR at {Host}:{Port} (Account: {AccountId}, Type: {AccountType})",
                    config.Host, config.Port, config.AccountId, config.AccountType);

                _config = config;
                UpdateConnectionState(IbkrConnectionStatus.Connecting);

                // TODO: Implement actual connection to Lean engine via IPC (Named Pipe or TCP)
                // This will be implemented in task 5.1 when extending Lean's IBrokerageHandler
                // For now, simulate connection logic
                
                // Simulate connection attempt
                await Task.Delay(1000, cancellationToken);

                // Check if TWS/Gateway is accessible
                // In production, this would connect via IPC to Lean engine
                var connected = await AttemptConnectionAsync(config, cancellationToken);

                if (connected)
                {
                    _connectionState.AccountId = config.AccountId;
                    _connectionState.AccountType = config.AccountType;
                    _connectionState.ConnectedAt = DateTime.UtcNow;
                    _connectionState.LastHeartbeatAt = DateTime.UtcNow;
                    _connectionState.TwsVersion = "Unknown"; // Will be populated from actual connection
                    _connectionState.ApiVersion = "Unknown";
                    _reconnectAttempts = 0;

                    UpdateConnectionState(IbkrConnectionStatus.Connected);
                    StartHeartbeatMonitoring();

                    _logger.LogInformation("Successfully connected to IBKR (Account: {AccountId})", config.AccountId);
                    return true;
                }
                else
                {
                    var error = $"Failed to connect to IBKR at {config.Host}:{config.Port}";
                    RecordError(500, error);
                    UpdateConnectionState(IbkrConnectionStatus.Error, error);
                    _logger.LogWarning(error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var error = $"Connection error: {ex.Message}";
                RecordError(999, error, ex.ToString());
                UpdateConnectionState(IbkrConnectionStatus.Error, error);
                _logger.LogError(ex, "Failed to connect to IBKR");
                return false;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Disconnect from IBKR
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                _logger.LogInformation("Disconnecting from IBKR");

                StopHeartbeatMonitoring();
                StopReconnectTimer();

                // TODO: Implement actual disconnection from Lean engine
                await Task.Delay(100);

                UpdateConnectionState(IbkrConnectionStatus.Disconnected);
                _connectionState.ConnectedAt = null;
                _connectionState.LastHeartbeatAt = null;

                _logger.LogInformation("Disconnected from IBKR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Get account summary
        /// </summary>
        public async Task<IbkrAccountSummary?> GetAccountSummaryAsync()
        {
            if (_connectionState.Status != IbkrConnectionStatus.Connected)
            {
                _logger.LogWarning("Cannot get account summary: not connected");
                return null;
            }

            try
            {
                await RateLimitAsync();

                // TODO: Implement actual account query via IPC to Lean engine
                // This will use Lean's IBrokerage.GetCashBalance() and GetAccountHoldings()
                
                // Placeholder data
                var summary = new IbkrAccountSummary
                {
                    AccountId = _connectionState.AccountId,
                    AccountType = _connectionState.AccountType,
                    BaseCurrency = "USD",
                    CashBalance = 100000m,
                    NetLiquidation = 100000m,
                    AvailableFunds = 100000m,
                    BuyingPower = 400000m, // 4x margin for stocks
                    GrossPositionValue = 0m,
                    MarginRequirement = 0m,
                    ExcessLiquidity = 100000m,
                    UpdatedAt = DateTime.UtcNow
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get account summary");
                RecordError(998, "Failed to get account summary", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get connection diagnostics
        /// </summary>
        public IbkrDiagnostics GetDiagnostics()
        {
            // Mask sensitive data in configuration
            var maskedConfig = _config != null ? new IbkrConnectionConfig
            {
                Host = _config.Host,
                Port = _config.Port,
                AccountId = MaskAccountId(_config.AccountId),
                AccountType = _config.AccountType,
                TimeoutSeconds = _config.TimeoutSeconds,
                EnableAutoReconnect = _config.EnableAutoReconnect,
                MaxReconnectAttempts = _config.MaxReconnectAttempts
            } : null;

            return new IbkrDiagnostics
            {
                ConnectionState = _connectionState,
                ErrorHistory = _errorHistory.ToList(),
                Configuration = maskedConfig,
                RateLimitStatus = new IbkrRateLimitStatus
                {
                    CurrentRequestsPerSecond = GetCurrentRequestRate(),
                    MaxRequestsPerSecond = MaxRequestsPerSecond,
                    QueuedRequests = 0 // Will be implemented with actual request queue
                }
            };
        }

        /// <summary>
        /// Background service execution
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IBKR Connection Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Clean up old request timestamps for rate limiting
                CleanupOldRequestTimestamps();

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            _logger.LogInformation("IBKR Connection Service stopped");
        }

        /// <summary>
        /// Attempt connection to IBKR
        /// </summary>
        private async Task<bool> AttemptConnectionAsync(IbkrConnectionConfig config, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: Implement actual connection via IPC to Lean engine
                // This is a placeholder that simulates a successful connection
                await Task.Delay(500, cancellationToken);
                return true; // Simulated success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection attempt failed");
                return false;
            }
        }

        /// <summary>
        /// Start heartbeat monitoring (every 10 seconds)
        /// </summary>
        private void StartHeartbeatMonitoring()
        {
            _healthCheckCts = new CancellationTokenSource();
            _heartbeatTimer = new Timer(async _ => await PerformHeartbeatAsync(), null, 
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Stop heartbeat monitoring
        /// </summary>
        private void StopHeartbeatMonitoring()
        {
            _healthCheckCts?.Cancel();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }

        /// <summary>
        /// Perform heartbeat check
        /// </summary>
        private async Task PerformHeartbeatAsync()
        {
            try
            {
                if (_connectionState.Status != IbkrConnectionStatus.Connected)
                {
                    return;
                }

                // TODO: Implement actual heartbeat via IPC to Lean engine
                // Check if Lean engine is still responsive
                await Task.Delay(100);

                // Update heartbeat timestamp
                _connectionState.LastHeartbeatAt = DateTime.UtcNow;

                // Check if heartbeat is stale (more than 30 seconds)
                if (!_connectionState.IsHealthy && _config?.EnableAutoReconnect == true)
                {
                    _logger.LogWarning("Connection lost, initiating reconnect");
                    _ = Task.Run(async () => await TriggerReconnectAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Heartbeat check failed");
                if (_config?.EnableAutoReconnect == true)
                {
                    _ = Task.Run(async () => await TriggerReconnectAsync());
                }
            }
        }

        /// <summary>
        /// Trigger auto-reconnect with exponential backoff
        /// </summary>
        private async Task TriggerReconnectAsync()
        {
            if (_connectionState.Status == IbkrConnectionStatus.Reconnecting)
            {
                return; // Already reconnecting
            }

            UpdateConnectionState(IbkrConnectionStatus.Reconnecting);

            while (_reconnectAttempts < (_config?.MaxReconnectAttempts ?? 10))
            {
                _reconnectAttempts++;

                // Calculate exponential backoff: 1s, 2s, 4s, 8s, ... max 60s
                var delaySeconds = Math.Min(Math.Pow(2, _reconnectAttempts - 1), 60);
                _connectionState.NextReconnectDelaySeconds = (int)delaySeconds;
                OnConnectionStateChanged();

                _logger.LogInformation("Reconnect attempt {Attempt}/{MaxAttempts} in {Delay} seconds",
                    _reconnectAttempts, _config?.MaxReconnectAttempts, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                if (_config != null)
                {
                    var success = await ConnectAsync(_config);
                    if (success)
                    {
                        _logger.LogInformation("Reconnection successful");
                        _reconnectAttempts = 0;
                        return;
                    }
                }
            }

            _logger.LogError("Max reconnection attempts reached. Giving up.");
            UpdateConnectionState(IbkrConnectionStatus.Error, "Max reconnection attempts reached");
        }

        /// <summary>
        /// Stop reconnect timer
        /// </summary>
        private void StopReconnectTimer()
        {
            _reconnectTimer?.Dispose();
            _reconnectTimer = null;
        }

        /// <summary>
        /// Update connection state and raise event
        /// </summary>
        private void UpdateConnectionState(IbkrConnectionStatus status, string? error = null)
        {
            _connectionState.Status = status;
            _connectionState.LastError = error;

            if (status != IbkrConnectionStatus.Reconnecting)
            {
                _connectionState.ReconnectAttempts = _reconnectAttempts;
                _connectionState.NextReconnectDelaySeconds = null;
            }

            OnConnectionStateChanged();
        }

        /// <summary>
        /// Raise connection state changed event
        /// </summary>
        private void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, _connectionState);
        }

        /// <summary>
        /// Record an error in history
        /// </summary>
        private void RecordError(int errorCode, string message, string? details = null)
        {
            var error = new IbkrErrorRecord
            {
                Timestamp = DateTime.UtcNow,
                ErrorCode = errorCode,
                Message = message,
                Details = details
            };

            _errorHistory.Enqueue(error);

            // Keep only last 50 errors
            while (_errorHistory.Count > 50)
            {
                _errorHistory.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Rate limiting: ensure we don't exceed IBKR's 50 requests/second limit
        /// </summary>
        private async Task RateLimitAsync()
        {
            _requestTimestamps.Enqueue(DateTime.UtcNow);

            var currentRate = GetCurrentRequestRate();
            if (currentRate >= MaxRequestsPerSecond)
            {
                // Wait 1 second before proceeding
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Get current request rate (requests per second)
        /// </summary>
        private int GetCurrentRequestRate()
        {
            var oneSecondAgo = DateTime.UtcNow.AddSeconds(-1);
            return _requestTimestamps.Count(ts => ts > oneSecondAgo);
        }

        /// <summary>
        /// Cleanup old request timestamps (keep only last 2 seconds)
        /// </summary>
        private void CleanupOldRequestTimestamps()
        {
            var twoSecondsAgo = DateTime.UtcNow.AddSeconds(-2);
            while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < twoSecondsAgo)
            {
                _requestTimestamps.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Mask account ID for security (show only last 4 characters)
        /// </summary>
        private string MaskAccountId(string accountId)
        {
            if (string.IsNullOrEmpty(accountId) || accountId.Length <= 4)
            {
                return "****";
            }

            return "****" + accountId.Substring(accountId.Length - 4);
        }

        public override void Dispose()
        {
            StopHeartbeatMonitoring();
            StopReconnectTimer();
            _connectionLock.Dispose();
            _healthCheckCts?.Dispose();
            base.Dispose();
        }
    }
}
