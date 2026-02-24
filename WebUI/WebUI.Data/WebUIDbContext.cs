using Microsoft.EntityFrameworkCore;
using WebUI.Data.Entities;

namespace WebUI.Data
{
    /// <summary>
    /// WebUI Database Context
    /// </summary>
    public class WebUIDbContext : DbContext
    {
        public WebUIDbContext(DbContextOptions<WebUIDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<BrokerAccount> BrokerAccounts { get; set; } = null!;
        public DbSet<Strategy> Strategies { get; set; } = null!;
        public DbSet<StrategyExecution> StrategyExecutions { get; set; } = null!;
        public DbSet<StrategyVersion> StrategyVersions { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RecurringPlan> RecurringPlans { get; set; } = null!;
        public DbSet<Backtest> Backtests { get; set; } = null!;
        public DbSet<ParameterOptimization> ParameterOptimizations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.CreatedAt);
            });

            // BrokerAccount configuration
            modelBuilder.Entity<BrokerAccount>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AccountNumber);
                entity.HasIndex(e => new { e.UserId, e.AccountNumber }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.BrokerAccounts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Strategy configuration
            modelBuilder.Entity<Strategy>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Strategies)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StrategyExecution configuration
            modelBuilder.Entity<StrategyExecution>(entity =>
            {
                entity.HasIndex(e => e.StrategyId);
                entity.HasIndex(e => e.StartedAt);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Strategy)
                    .WithMany(s => s.Executions)
                    .HasForeignKey(e => e.StrategyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StrategyVersion configuration
            modelBuilder.Entity<StrategyVersion>(entity =>
            {
                entity.HasIndex(e => e.StrategyId);
                entity.HasIndex(e => e.VersionNumber);
                entity.HasIndex(e => new { e.StrategyId, e.VersionNumber }).IsUnique();

                entity.HasOne(e => e.Strategy)
                    .WithMany()
                    .HasForeignKey(e => e.StrategyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.BrokerAccountId);
                entity.HasIndex(e => e.BrokerOrderId);
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.BrokerAccount)
                    .WithMany(b => b.Orders)
                    .HasForeignKey(e => e.BrokerAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Position configuration
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasIndex(e => e.BrokerAccountId);
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => new { e.BrokerAccountId, e.Symbol }).IsUnique();

                entity.HasOne(e => e.BrokerAccount)
                    .WithMany(b => b.Positions)
                    .HasForeignKey(e => e.BrokerAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityType);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RecurringPlan configuration
            modelBuilder.Entity<RecurringPlan>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BrokerAccountId);
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.NextExecutionDate);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.BrokerAccount)
                    .WithMany()
                    .HasForeignKey(e => e.BrokerAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Backtest configuration
            modelBuilder.Entity<Backtest>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StrategyId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.StrategyId });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Strategy)
                    .WithMany()
                    .HasForeignKey(e => e.StrategyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ParameterOptimization configuration
            modelBuilder.Entity<ParameterOptimization>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StrategyId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Strategy)
                    .WithMany()
                    .HasForeignKey(e => e.StrategyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.BestBacktest)
                    .WithMany()
                    .HasForeignKey(e => e.BestBacktestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
