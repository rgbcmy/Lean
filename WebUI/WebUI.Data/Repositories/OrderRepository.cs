using Microsoft.EntityFrameworkCore;
using WebUI.Data.Entities;

namespace WebUI.Data.Repositories
{
    /// <summary>
    /// Order repository with specialized queries
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByBrokerAccountIdAsync(int brokerAccountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetOrdersBySymbolAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }

    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(WebUIDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetByBrokerAccountIdAsync(
            int brokerAccountId,
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(o => o.BrokerAccountId == brokerAccountId, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetBySymbolAsync(
            string symbol,
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(o => o.Symbol == symbol, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetOrdersBySymbolAsync(
            int brokerAccountId,
            string symbol,
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(o => o.BrokerAccountId == brokerAccountId && o.Symbol == symbol, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate,
                cancellationToken);
        }
    }
}
