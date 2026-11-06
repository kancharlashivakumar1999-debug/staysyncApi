using System.Data;
using Dapper;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class FloorRepository : IFloorRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly INoSeriesService _noSeries;
        private readonly IIdentityInformationService _identity;

        public FloorRepository(
            IDbConnectionFactory factory,
            INoSeriesService noSeries,
            IIdentityInformationService identity)
        {
            _factory = factory;
            _noSeries = noSeries;
            _identity = identity;
        }

        private const string FloorProjectionSql = """
            SELECT f.floor_id AS FloorId,
                   f.branch_id AS BranchId,
                   f.floor_code AS FloorCode,
                   f.floor_number AS FloorNumber,
                   f.floor_name AS FloorName,
                   f.description AS Description,
                   f.created_at AS CreatedAt
              FROM floors f
            """;

        public async Task<IEnumerable<Floor>> GetByBranchIdAsync(Guid branchId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{FloorProjectionSql} WHERE f.branch_id = @BranchId ORDER BY f.floor_number ASC;";
            return await conn.QueryAsync<Floor>(sql, new { BranchId = branchId });
        }

        public async Task<Floor?> GetByIdAsync(Guid floorId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{FloorProjectionSql} WHERE f.floor_id = @FloorId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Floor>(sql, new { FloorId = floorId });
        }

        public async Task<Guid> CreateAsync(Floor floor)
        {
            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                floor.FloorId = Uuid7.NewGuid();

                // 1. Fetch next floor code inside transaction SCOPED BY BRANCH ID
                var code = await _noSeries.GetNextCodeWithTransactionAsync(conn, tran, "FL", floor.BranchId);
                floor.FloorCode = code;

                // 2. Insert with the generated code
                const string sql = @"
                    INSERT INTO floors (floor_id, branch_id, floor_code, floor_number, floor_name, description, created_at)
                    VALUES (@FloorId, @BranchId, @FloorCode, @FloorNumber, @FloorName, @Description, @CreatedAt)
                    RETURNING floor_id;";

                var id = await conn.ExecuteScalarAsync<Guid>(sql, floor, transaction: tran);
                tran.Commit();
                return id;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid floorId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM floors WHERE floor_id = @FloorId;";
            return await conn.ExecuteAsync(sql, new { FloorId = floorId }) > 0;
        }
    }
}
