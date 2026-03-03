using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace xFood.Infrastructure.Persistence
{
    // Fábrica usada pelo EF Core durante o tempo de design (migrações).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<xFoodDbContext>
    {
        // Cria o DbContext usando a mesma conexão do appsettings.
        public xFoodDbContext CreateDbContext(string[] args)
        {
            // String de conexão usada para gerar migrações.
            var conn = "Host=ep-nameless-smoke-ac0cn8zh-pooler.sa-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_shjgkE6GD4Jf;Port=5432;SSL Mode=Require;Trust Server Certificate=true;";

            var opts = new DbContextOptionsBuilder<xFoodDbContext>()
                .UseNpgsql(conn)
                .Options;

            return new xFoodDbContext(opts);
        }
    }
}
