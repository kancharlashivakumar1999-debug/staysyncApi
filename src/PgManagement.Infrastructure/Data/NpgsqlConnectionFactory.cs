using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace PgManagement.Infrastructure.Data
{
    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgresConnection") 
                ?? "Host=localhost;Port=5432;Database=pg_management_db;Username=postgres;Password=postgres";
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
