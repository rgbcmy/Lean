using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebUI.Data
{
    /// <summary>
    /// Design-time factory for EF Core migrations
    /// </summary>
    public class WebUIDbContextFactory : IDesignTimeDbContextFactory<WebUIDbContext>
    {
        public WebUIDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebUIDbContext>();
            
            // Use SQLite for migrations by default
            optionsBuilder.UseSqlite("Data Source=webui-design.db");

            return new WebUIDbContext(optionsBuilder.Options);
        }
    }
}
