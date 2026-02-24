using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebUI.Core.Security;

namespace WebUI.Data
{
    /// <summary>
    /// Database configuration and provider abstraction
    /// </summary>
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddWebUIDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var provider = configuration["WebUI:DatabaseProvider"] ?? "SQLite";
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured.");
            }

            switch (provider.ToUpperInvariant())
            {
                case "POSTGRESQL":
                    services.AddDbContext<WebUIDbContext>(options =>
                        options.UseNpgsql(connectionString,
                            npgsqlOptions => npgsqlOptions.MigrationsAssembly("WebUI.Data")));
                    break;

                case "SQLITE":
                    services.AddDbContext<WebUIDbContext>(options =>
                        options.UseSqlite(connectionString,
                            sqliteOptions => sqliteOptions.MigrationsAssembly("WebUI.Data")));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported database provider: {provider}. Use 'PostgreSQL' or 'SQLite'.");
            }

            return services;
        }

        public static async Task InitializeDatabaseAsync(
            IServiceProvider serviceProvider,
            bool runMigrations = true)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WebUIDbContext>();

            if (runMigrations)
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            await SeedDataAsync(context);
        }

        private static async Task SeedDataAsync(WebUIDbContext context)
        {
            Console.WriteLine("[DatabaseConfiguration] Checking if users exist...");
            var userCount = await context.Users.CountAsync();
            Console.WriteLine($"[DatabaseConfiguration] Found {userCount} existing users.");
            
            // Seed default admin user if no users exist
            if (!await context.Users.AnyAsync())
            {
                Console.WriteLine("[DatabaseConfiguration] Creating default admin user...");
                
                // Use the same PasswordHasher that the authentication service uses
                var passwordHasher = new PasswordHasher();
                var hashedPassword = passwordHasher.HashPassword("admin123");
                
                var adminUser = new Entities.User
                {
                    Username = "admin",
                    PasswordHash = hashedPassword,
                    Email = "admin@leanwebui.local",
                    FullName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                Console.WriteLine("[DatabaseConfiguration] Admin user created successfully!");
            }
            else
            {
                Console.WriteLine("[DatabaseConfiguration] Users already exist, skipping seed.");
            }
        }

        /// <summary>
        /// Seed default data (can be called separately)
        /// </summary>
        public static async Task SeedDefaultDataAsync(WebUIDbContext context)
        {
            Console.WriteLine("[DatabaseConfiguration] Starting to seed default data...");
            await SeedDataAsync(context);
            Console.WriteLine("[DatabaseConfiguration] Finished seeding default data.");
        }
    }
}
