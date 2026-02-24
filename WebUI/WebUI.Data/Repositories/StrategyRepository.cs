using Microsoft.EntityFrameworkCore;
using WebUI.Data.Entities;

namespace WebUI.Data.Repositories
{
    /// <summary>
    /// Strategy repository with specialized queries
    /// </summary>
    public interface IStrategyRepository : IRepository<Strategy>
    {
        Task<IEnumerable<Strategy>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Strategy>> GetActiveStrategiesAsync(CancellationToken cancellationToken = default);
        Task<Strategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }

    public class StrategyRepository : Repository<Strategy>, IStrategyRepository
    {
        public StrategyRepository(WebUIDbContext context) : base(context) { }

        public async Task<IEnumerable<Strategy>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Strategy>> GetActiveStrategiesAsync(
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(s => s.IsActive, cancellationToken);
        }

        public async Task<Strategy?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
        }
    }
}
