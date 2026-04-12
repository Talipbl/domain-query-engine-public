using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomainQueryEngine.PostgreSQL
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=dqe_db;Username=postgres;Password=sa1453");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
