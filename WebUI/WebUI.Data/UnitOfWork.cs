using WebUI.Data.Entities;
using WebUI.Data.Repositories;

namespace WebUI.Data
{
    /// <summary>
    /// Unit of Work pattern implementation
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<BrokerAccount> BrokerAccounts { get; }
        IStrategyRepository Strategies { get; }
        IRepository<StrategyExecution> StrategyExecutions { get; }
        IOrderRepository Orders { get; }
        IPositionRepository Positions { get; }
        IRepository<AuditLog> AuditLogs { get; }

        /// <summary>
        /// Get a generic repository for any entity type
        /// </summary>
        IRepository<T> GetRepository<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly WebUIDbContext _context;
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

        private IRepository<User>? _users;
        private IRepository<BrokerAccount>? _brokerAccounts;
        private IStrategyRepository? _strategies;
        private IRepository<StrategyExecution>? _strategyExecutions;
        private IOrderRepository? _orders;
        private IPositionRepository? _positions;
        private IRepository<AuditLog>? _auditLogs;

        public UnitOfWork(WebUIDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users =>
            _users ??= new Repository<User>(_context);

        public IRepository<BrokerAccount> BrokerAccounts =>
            _brokerAccounts ??= new Repository<BrokerAccount>(_context);

        public IStrategyRepository Strategies =>
            _strategies ??= new StrategyRepository(_context);

        public IRepository<StrategyExecution> StrategyExecutions =>
            _strategyExecutions ??= new Repository<StrategyExecution>(_context);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);

        public IPositionRepository Positions =>
            _positions ??= new PositionRepository(_context);

        public IRepository<AuditLog> AuditLogs =>
            _auditLogs ??= new Repository<AuditLog>(_context);

        public IRepository<T> GetRepository<T>() where T : class
        {
            // Return specialized repositories if available
            if (typeof(T) == typeof(Position))
                return (IRepository<T>)(object)Positions;
            if (typeof(T) == typeof(Order))
                return (IRepository<T>)(object)Orders;
            if (typeof(T) == typeof(Strategy))
                return (IRepository<T>)(object)Strategies;
            if (typeof(T) == typeof(User))
                return Users as IRepository<T> ?? throw new InvalidOperationException();
            if (typeof(T) == typeof(BrokerAccount))
                return BrokerAccounts as IRepository<T> ?? throw new InvalidOperationException();
            if (typeof(T) == typeof(StrategyExecution))
                return StrategyExecutions as IRepository<T> ?? throw new InvalidOperationException();
            if (typeof(T) == typeof(AuditLog))
                return AuditLogs as IRepository<T> ?? throw new InvalidOperationException();

            // Create a new repository for other types
            return new Repository<T>(_context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
