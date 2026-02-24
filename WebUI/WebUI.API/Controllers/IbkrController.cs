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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers
{
    /// <summary>
    /// Controller for IBKR (Interactive Brokers) integration
    /// </summary>
    [ApiController]
    [Route("api/v1/ibkr")]
    [Authorize]
    public class IbkrController : ControllerBase
    {
        private readonly ILogger<IbkrController> _logger;
        private readonly IIbkrConnectionService _ibkrConnectionService;

        public IbkrController(
            ILogger<IbkrController> logger,
            IIbkrConnectionService ibkrConnectionService)
        {
            _logger = logger;
            _ibkrConnectionService = ibkrConnectionService;
        }

        /// <summary>
        /// Get current connection status
        /// </summary>
        /// <returns>Current IBKR connection state</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(IbkrConnectionState), StatusCodes.Status200OK)]
        public ActionResult<IbkrConnectionState> GetStatus()
        {
            var state = _ibkrConnectionService.ConnectionState;
            return Ok(state);
        }

        /// <summary>
        /// Connect to IBKR TWS/Gateway
        /// </summary>
        /// <param name="config">Connection configuration</param>
        /// <returns>Connection result</returns>
        [HttpPost("connect")]
        [ProducesResponseType(typeof(IbkrConnectionState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IbkrConnectionState>> Connect([FromBody] IbkrConnectionConfig config)
        {
            if (config == null)
            {
                return BadRequest(new
                {
                    message = "Connection configuration is required"
                });
            }

            // Validate configuration
            if (string.IsNullOrWhiteSpace(config.AccountId))
            {
                return BadRequest(new
                {
                    message = "Account ID is required"
                });
            }

            if (config.Port <= 0 || config.Port > 65535)
            {
                return BadRequest(new
                {
                    message = "Port must be between 1 and 65535"
                });
            }

            // Warn if connecting to live account
            if (config.AccountType == IbkrAccountType.Live && config.Port == 7496)
            {
                _logger.LogWarning("Connecting to LIVE IBKR account: {AccountId}", config.AccountId);
            }

            try
            {
                var success = await _ibkrConnectionService.ConnectAsync(config);

                if (success)
                {
                    return Ok(_ibkrConnectionService.ConnectionState);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        message = _ibkrConnectionService.ConnectionState.LastError ?? "Failed to connect to IBKR"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to IBKR");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Disconnect from IBKR
        /// </summary>
        /// <returns>Disconnection result</returns>
        [HttpPost("disconnect")]
        [ProducesResponseType(typeof(IbkrConnectionState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IbkrConnectionState>> Disconnect()
        {
            try
            {
                await _ibkrConnectionService.DisconnectAsync();
                return Ok(_ibkrConnectionService.ConnectionState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from IBKR");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get IBKR account summary (balance, buying power, positions value)
        /// </summary>
        /// <returns>Account summary information</returns>
        [HttpGet("account")]
        [ProducesResponseType(typeof(IbkrAccountSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IbkrAccountSummary>> GetAccount()
        {
            if (_ibkrConnectionService.ConnectionState.Status != IbkrConnectionStatus.Connected)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Not connected to IBKR. Please connect first."
                });
            }

            try
            {
                var summary = await _ibkrConnectionService.GetAccountSummaryAsync();

                if (summary == null)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        message = "Could not retrieve account summary from IBKR"
                    });
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account summary");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get account balance and buying power
        /// </summary>
        /// <returns>Balance and buying power information</returns>
        [HttpGet("account/balance")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetBalance()
        {
            if (_ibkrConnectionService.ConnectionState.Status != IbkrConnectionStatus.Connected)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Not connected to IBKR. Please connect first."
                });
            }

            try
            {
                var summary = await _ibkrConnectionService.GetAccountSummaryAsync();

                if (summary == null)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        message = "Could not retrieve account balance from IBKR"
                    });
                }

                return Ok(new
                {
                    accountId = summary.AccountId,
                    baseCurrency = summary.BaseCurrency,
                    cashBalance = summary.CashBalance,
                    netLiquidation = summary.NetLiquidation,
                    availableFunds = summary.AvailableFunds,
                    buyingPower = summary.BuyingPower,
                    grossPositionValue = summary.GrossPositionValue,
                    marginRequirement = summary.MarginRequirement,
                    excessLiquidity = summary.ExcessLiquidity,
                    updatedAt = summary.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account balance");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get connection diagnostics (version info, error history, rate limits)
        /// </summary>
        /// <returns>Diagnostics information</returns>
        [HttpGet("diagnostics")]
        [ProducesResponseType(typeof(IbkrDiagnostics), StatusCodes.Status200OK)]
        public ActionResult<IbkrDiagnostics> GetDiagnostics()
        {
            var diagnostics = _ibkrConnectionService.GetDiagnostics();
            return Ok(diagnostics);
        }

        /// <summary>
        /// Health check endpoint for IBKR connection
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
        public ActionResult GetHealth()
        {
            var state = _ibkrConnectionService.ConnectionState;

            if (state.IsHealthy)
            {
                return Ok(new
                {
                    status = "healthy",
                    connected = true,
                    accountType = state.AccountType.ToString(),
                    connectionDurationSeconds = state.ConnectionDurationSeconds,
                    lastHeartbeat = state.LastHeartbeatAt
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "unhealthy",
                    connected = false,
                    connectionStatus = state.Status.ToString(),
                    lastError = state.LastError
                });
            }
        }
    }
}
