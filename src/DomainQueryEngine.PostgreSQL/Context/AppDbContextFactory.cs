using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomainQueryEngine.PostgreSQL
{
    /// <summary>
    /// Design-time factory used by <c>dotnet ef</c>. The connection string is read from the
    /// <c>ConnectionStrings__DefaultConnection</c> environment variable (the same key ASP.NET Core
    /// uses), so no credentials live in source control.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private const string ConnectionStringEnvVar = "ConnectionStrings__DefaultConnection";

        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Set the '{ConnectionStringEnvVar}' environment variable before running EF Core commands. " +
                    "Example: Host=localhost;Port=5432;Database=dqe_db;Username=postgres;Password=<your-password>");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
