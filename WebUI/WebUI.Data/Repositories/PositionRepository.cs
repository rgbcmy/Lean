using Microsoft.EntityFrameworkCore;
using WebUI.Data.Entities;

namespace WebUI.Data.Repositories
{
    /// <summary>
    /// Position repository implementation
    /// </summary>
    public class PositionRepository : Repository<Position>, IPositionRepository
    {
        public PositionRepository(WebUIDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Position>> GetByBrokerAccountIdAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.BrokerAccountId == brokerAccountId && p.Quantity > 0)
                .OrderBy(p => p.Symbol)
                .ToListAsync(cancellationToken);
        }

        public async Task<Position?> GetBySymbolAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.BrokerAccountId == brokerAccountId && p.Symbol == symbol, cancellationToken);
        }

        public async Task<IEnumerable<Position>> GetFilteredPositionsAsync(
            int brokerAccountId,
            string? symbolFilter = null,
            bool? onlyProfitable = null,
            bool? onlyLosing = null,
            string sortBy = "Symbol",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.BrokerAccountId == brokerAccountId && p.Quantity > 0);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(symbolFilter))
            {
                query = query.Where(p => p.Symbol.Contains(symbolFilter.ToUpper()));
            }

            if (onlyProfitable == true)
            {
                query = query.Where(p => p.UnrealizedPnL > 0);
            }

            if (onlyLosing == true)
            {
                query = query.Where(p => p.UnrealizedPnL < 0);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "symbol" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Symbol)
                    : query.OrderBy(p => p.Symbol),
                "quantity" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Quantity)
                    : query.OrderBy(p => p.Quantity),
                "marketvalue" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Quantity * p.CurrentPrice)
                    : query.OrderBy(p => p.Quantity * p.CurrentPrice),
                "unrealizedpnl" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.UnrealizedPnL)
                    : query.OrderBy(p => p.UnrealizedPnL),
                "unrealizedpnlpercent" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.AverageCost > 0 ? (p.UnrealizedPnL / (p.Quantity * p.AverageCost)) : 0)
                    : query.OrderBy(p => p.AverageCost > 0 ? (p.UnrealizedPnL / (p.Quantity * p.AverageCost)) : 0),
                _ => query.OrderBy(p => p.Symbol)
            };

            return await query.ToListAsync(cancellationToken);
        }

        public async Task UpdatePositionPriceAsync(int positionId, decimal currentPrice, CancellationToken cancellationToken = default)
        {
            var position = await _dbSet.FindAsync(new object[] { positionId }, cancellationToken);
            if (position != null)
            {
                position.CurrentPrice = currentPrice;
                position.UnrealizedPnL = (currentPrice - position.AverageCost) * position.Quantity;
                position.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task UpdatePositionQuantityAsync(int positionId, int quantityChange, decimal tradePrice, CancellationToken cancellationToken = default)
        {
            var position = await _dbSet.FindAsync(new object[] { positionId }, cancellationToken);
            if (position != null)
            {
                if (quantityChange > 0) // Buy
                {
                    // Update average cost with weighted average
                    decimal totalCost = (position.AverageCost * position.Quantity) + (tradePrice * quantityChange);
                    position.Quantity += quantityChange;
                    position.AverageCost = position.Quantity > 0 ? totalCost / position.Quantity : 0;
                }
                else // Sell
                {
                    int sellQuantity = Math.Abs(quantityChange);
                    // Calculate realized P&L
                    decimal realizedPnL = (tradePrice - position.AverageCost) * sellQuantity;
                    position.RealizedPnL += realizedPnL;
                    position.Quantity -= sellQuantity;

                    // If position is closed, average cost remains for historical purposes
                }

                // Update unrealized P&L
                position.UnrealizedPnL = (position.CurrentPrice - position.AverageCost) * position.Quantity;
                position.LastUpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
