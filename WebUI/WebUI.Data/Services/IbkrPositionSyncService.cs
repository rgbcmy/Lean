/*
 * IBKR Position Sync Service
 * 从 IBKR TWS 同步持仓数据到本地数据库
 * Syncs position and account data from IBKR TWS into the local database.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data.Entities;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Result of an IBKR position sync operation
    /// </summary>
    public class IbkrSyncResult
    {
        public bool Success { get; set; }
        public int PositionsSynced { get; set; }
        public int PositionsAdded { get; set; }
        public int PositionsUpdated { get; set; }
        public int PositionsRemoved { get; set; }
        public decimal CashBalance { get; set; }
        public decimal NetLiquidation { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Interface for the IBKR position sync service
    /// </summary>
    public interface IIbkrPositionSyncService
    {
        /// <summary>
        /// Sync all positions and account data from IBKR into the local database.
        /// Creates the BrokerAccount if it does not exist yet.
        /// </summary>
        Task<IbkrSyncResult> SyncAsync(
            IbkrConnectionConfig config, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Syncs live IBKR data into the local SQLite/PostgreSQL database.
    /// </summary>
    public class IbkrPositionSyncService : IIbkrPositionSyncService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIbkrTwsApiClient _twsClient;
        private readonly ILogger<IbkrPositionSyncService> _logger;

        public IbkrPositionSyncService(
            IServiceScopeFactory scopeFactory,
            IIbkrTwsApiClient twsClient,
            ILogger<IbkrPositionSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _twsClient = twsClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IbkrSyncResult> SyncAsync(
            IbkrConnectionConfig config, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting IBKR sync for account {AccountId}", config.AccountId);

            var result = new IbkrSyncResult();

            try
            {
                // ── 1. Fetch positions from TWS ──────────────────────────────────────
                // The TWS client maintains a persistent connection; no host/port needed here.
                var positions = await _twsClient.GetPositionsAsync(cancellationToken);

                // ── 2. Fetch account values from TWS ─────────────────────────────────
                var accountValues = await _twsClient.GetAccountUpdatesAsync(
                    config.AccountId, cancellationToken);

                decimal cashBalance = ParseAccountValue(accountValues, "TotalCashValue", config.AccountId)
                                   ?? ParseAccountValue(accountValues, "CashBalance", config.AccountId)
                                   ?? 0;
                decimal netLiquidation = ParseAccountValue(accountValues, "NetLiquidation", config.AccountId)
                                      ?? 0;
                decimal buyingPower = ParseAccountValue(accountValues, "BuyingPower", config.AccountId)
                                   ?? 0;

                result.CashBalance = cashBalance;
                result.NetLiquidation = netLiquidation;

                // ── 3. Persist to DB ──────────────────────────────────────────────────
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<WebUIDbContext>();

                // Find or create the BrokerAccount for this IBKR account
                var brokerAccount = await dbContext.BrokerAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == config.AccountId, cancellationToken);

                if (brokerAccount == null)
                {
                    // Create a default BrokerAccount (UserId=1 = seed admin user)
                    brokerAccount = new BrokerAccount
                    {
                        UserId = 1,
                        BrokerName = "IBKR",
                        AccountNumber = config.AccountId,
                        AccountType = config.AccountType == IbkrAccountType.Live ? "Live" : "Paper",
                        IsConnected = true,
                        LastConnectedAt = DateTime.UtcNow,
                        CashBalance = cashBalance,
                        BuyingPower = buyingPower,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    dbContext.BrokerAccounts.Add(brokerAccount);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created BrokerAccount for IBKR account {AccountId} with DB id={Id}", config.AccountId, brokerAccount.Id);
                }
                else
                {
                    // Update existing account
                    brokerAccount.IsConnected = true;
                    brokerAccount.LastConnectedAt = DateTime.UtcNow;
                    brokerAccount.CashBalance = cashBalance;
                    brokerAccount.BuyingPower = buyingPower;
                    brokerAccount.UpdatedAt = DateTime.UtcNow;
                    dbContext.BrokerAccounts.Update(brokerAccount);
                    _logger.LogInformation("Updated BrokerAccount for IBKR account {AccountId} (DB id={Id})", config.AccountId, brokerAccount.Id);
                }

                int brokerAccountId = brokerAccount.Id;

                // Load existing positions from DB
                var existingPositions = await dbContext.Positions
                    .Where(p => p.BrokerAccountId == brokerAccountId)
                    .ToListAsync(cancellationToken);

                // Build a set of symbols coming from IBKR
                var ibkrSymbols = new HashSet<string>(
                    positions
                        .Where(p => p.Quantity != 0 && p.SecType == "STK") // stock positions only
                        .Select(p => p.Symbol),
                    StringComparer.OrdinalIgnoreCase);

                // Remove positions that are no longer in IBKR
                var toRemove = existingPositions
                    .Where(p => !ibkrSymbols.Contains(p.Symbol))
                    .ToList();
                if (toRemove.Any())
                {
                    dbContext.Positions.RemoveRange(toRemove);
                    result.PositionsRemoved = toRemove.Count;
                    _logger.LogInformation("Removing {Count} stale positions", toRemove.Count);
                }

                // Upsert positions
                foreach (var ibkrPos in positions.Where(p => p.Quantity != 0 && p.SecType == "STK"))
                {
                    var existing = existingPositions.FirstOrDefault(p =>
                        string.Equals(p.Symbol, ibkrPos.Symbol, StringComparison.OrdinalIgnoreCase));

                    decimal currentPrice = ibkrPos.AverageCost; // will be refreshed by market data later
                    int qty = (int)Math.Abs(ibkrPos.Quantity);
                    if (ibkrPos.Quantity < 0) qty = -(int)Math.Abs(ibkrPos.Quantity);

                    if (existing == null)
                    {
                        // Add new position
                        var newPos = new Position
                        {
                            BrokerAccountId = brokerAccountId,
                            Symbol = ibkrPos.Symbol.ToUpper(),
                            Quantity = qty,
                            AverageCost = ibkrPos.AverageCost,
                            CurrentPrice = ibkrPos.AverageCost,
                            UnrealizedPnL = 0,
                            RealizedPnL = 0,
                            FirstPurchaseDate = DateTime.UtcNow,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        dbContext.Positions.Add(newPos);
                        result.PositionsAdded++;
                    }
                    else
                    {
                        // Update existing
                        bool changed = existing.Quantity != qty || existing.AverageCost != ibkrPos.AverageCost;
                        existing.Quantity = qty;
                        existing.AverageCost = ibkrPos.AverageCost;
                        existing.LastUpdatedAt = DateTime.UtcNow;
                        if (changed)
                        {
                            existing.UnrealizedPnL = (existing.CurrentPrice - existing.AverageCost) * existing.Quantity;
                        }
                        dbContext.Positions.Update(existing);
                        result.PositionsUpdated++;
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                result.Success = true;
                result.PositionsSynced = ibkrSymbols.Count;
                _logger.LogInformation(
                    "IBKR sync complete: added={A} updated={U} removed={R}",
                    result.PositionsAdded, result.PositionsUpdated, result.PositionsRemoved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during IBKR sync for account {AccountId}", config.AccountId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Try to parse a specific account value key from the fetched IbkrAccountValue list.
        /// Prefers the base-currency value; falls back to the first match.
        /// </summary>
        private static decimal? ParseAccountValue(List<IbkrAccountValue> values, string key, string account)
        {
            // Prefer the entry for the requested account (IBKR can return values for multiple accounts)
            var matches = values.Where(v =>
                string.Equals(v.Key, key, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(v.AccountName) ||
                 string.Equals(v.AccountName, account, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Prefer USD / base-currency entry; fall back to any
            var best = matches.FirstOrDefault(m => string.Equals(m.Currency, "USD", StringComparison.OrdinalIgnoreCase))
                    ?? matches.FirstOrDefault(m => string.IsNullOrEmpty(m.Currency))
                    ?? matches.FirstOrDefault();

            if (best == null) return null;

            return decimal.TryParse(best.Val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;
        }
    }
}
