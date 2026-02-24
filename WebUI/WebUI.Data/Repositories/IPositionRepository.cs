using WebUI.Data.Entities;

namespace WebUI.Data.Repositories
{
    /// <summary>
    /// Position repository interface
    /// </summary>
    public interface IPositionRepository : IRepository<Position>
    {
        /// <summary>
        /// Get all positions for a broker account
        /// </summary>
        Task<IEnumerable<Position>> GetByBrokerAccountIdAsync(int brokerAccountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific position by broker account and symbol
        /// </summary>
        Task<Position?> GetBySymbolAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get positions with filtering and sorting
        /// </summary>
        Task<IEnumerable<Position>> GetFilteredPositionsAsync(
            int brokerAccountId,
            string? symbolFilter = null,
            bool? onlyProfitable = null,
            bool? onlyLosing = null,
            string sortBy = "Symbol",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update position price and P&L
        /// </summary>
        Task UpdatePositionPriceAsync(int positionId, decimal currentPrice, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update position quantity (after trade execution)
        /// </summary>
        Task UpdatePositionQuantityAsync(int positionId, int quantityChange, decimal tradePrice, CancellationToken cancellationToken = default);
    }
}
