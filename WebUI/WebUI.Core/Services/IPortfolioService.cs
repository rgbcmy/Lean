using WebUI.Core.Models;

namespace WebUI.Core.Services
{
    /// <summary>
    /// Portfolio management service interface
    /// </summary>
    public interface IPortfolioService
    {
        /// <summary>
        /// Get all positions for the current user
        /// </summary>
        Task<PortfolioPositionsResponse> GetPositionsAsync(int brokerAccountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get filtered and sorted positions
        /// </summary>
        Task<PortfolioPositionsResponse> GetFilteredPositionsAsync(int brokerAccountId, PositionQueryOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get position detail by symbol
        /// </summary>
        Task<PositionDetailResponse?> GetPositionDetailAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Close a position (sell all or partial)
        /// </summary>
        Task<OrderResponse> ClosePositionAsync(int brokerAccountId, string symbol, ClosePositionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get portfolio allocation
        /// </summary>
        Task<PortfolioAllocationResponse> GetPortfolioAllocationAsync(int brokerAccountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get equity curve data
        /// </summary>
        Task<EquityCurveResponse> GetEquityCurveAsync(int brokerAccountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update position prices from market data
        /// </summary>
        Task UpdatePositionPricesAsync(int brokerAccountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Export positions to CSV
        /// </summary>
        Task<byte[]> ExportPositionsToCsvAsync(int brokerAccountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Export positions to Excel
        /// </summary>
        Task<byte[]> ExportPositionsToExcelAsync(int brokerAccountId, CancellationToken cancellationToken = default);
    }
}
