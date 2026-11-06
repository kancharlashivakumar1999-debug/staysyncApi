using System.Data;
using Dapper;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly INoSeriesService _noSeries;
        private readonly IIdentityInformationService _identity;

        public BranchRepository(
            IDbConnectionFactory factory,
            INoSeriesService noSeries,
            IIdentityInformationService identity)
        {
            _factory = factory;
            _noSeries = noSeries;
            _identity = identity;
        }

        private const string BranchProjectionSql = """
            SELECT b.branch_id AS BranchId,
                   b.owner_id AS OwnerId,
                   b.branch_code AS BranchCode,
                   b.branch_name AS BranchName,
                   b.contact_number AS ContactNumber,
                   b.address_line AS AddressLine,
                   b.city AS City,
                   b.state AS State,
                   b.pincode AS Pincode,
                   b.total_floors AS TotalFloors,
                   b.created_at AS CreatedAt,
                   b.updated_at AS UpdatedAt
              FROM branches b
            """;

        public async Task<IEnumerable<Branch>> GetByOwnerIdAsync(Guid ownerId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{BranchProjectionSql} WHERE b.owner_id = @OwnerId ORDER BY b.branch_code ASC;";
            return await conn.QueryAsync<Branch>(sql, new { OwnerId = ownerId });
        }

        public async Task<Branch?> GetByIdAsync(Guid branchId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{BranchProjectionSql} WHERE b.branch_id = @BranchId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Branch>(sql, new { BranchId = branchId });
        }

        public async Task<Guid> CreateAsync(Branch branch)
        {
            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                var effectiveOwnerId = branch.OwnerId != Guid.Empty ? branch.OwnerId : (_identity.UserId ?? Guid.Empty);
                branch.BranchId = Uuid7.NewGuid();
                branch.OwnerId = effectiveOwnerId;

                // 1. First fetch next code inside transaction
                var code = await _noSeries.GetNextCodeWithTransactionAsync(conn, tran, "BR", effectiveOwnerId);
                branch.BranchCode = code;

                // 2. Insert with the generated code
                const string sql = @"
                    INSERT INTO branches (branch_id, owner_id, branch_code, branch_name, contact_number, address_line, city, state, pincode, total_floors, created_at, updated_at)
                    VALUES (@BranchId, @OwnerId, @BranchCode, @BranchName, @ContactNumber, @AddressLine, @City, @State, @Pincode, @TotalFloors, @CreatedAt, @UpdatedAt)
                    RETURNING branch_id;";

                var id = await conn.ExecuteScalarAsync<Guid>(sql, branch, transaction: tran);
                tran.Commit();
                return id;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<Branch> CreateBranchWithFloorsAsync(Branch branch, List<Floor> floors)
        {
            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                var effectiveOwnerId = branch.OwnerId != Guid.Empty ? branch.OwnerId : (_identity.UserId ?? Guid.Empty);
                branch.BranchId = Uuid7.NewGuid();
                branch.OwnerId = effectiveOwnerId;

                // 1. Fetch next branch code inside transaction (scoped to ownerId)
                var branchCode = await _noSeries.GetNextCodeWithTransactionAsync(conn, tran, "BR", effectiveOwnerId);
                branch.BranchCode = branchCode;

                // 2. Insert branch
                const string branchSql = @"
                    INSERT INTO branches (branch_id, owner_id, branch_code, branch_name, contact_number, address_line, city, state, pincode, total_floors, created_at, updated_at)
                    VALUES (@BranchId, @OwnerId, @BranchCode, @BranchName, @ContactNumber, @AddressLine, @City, @State, @Pincode, @TotalFloors, @CreatedAt, @UpdatedAt)
                    RETURNING branch_id;";

                var branchId = await conn.ExecuteScalarAsync<Guid>(branchSql, branch, transaction: tran);

                // 3. Insert all floors inside the same transaction (scoped to branchId)
                const string floorSql = @"
                    INSERT INTO floors (floor_id, branch_id, floor_code, floor_number, floor_name, description, created_at)
                    VALUES (@FloorId, @BranchId, @FloorCode, @FloorNumber, @FloorName, @Description, @CreatedAt);";

                foreach (var floor in floors)
                {
                    floor.FloorId = Uuid7.NewGuid();
                    floor.BranchId = branchId;
                    var floorCode = await _noSeries.GetNextCodeWithTransactionAsync(conn, tran, "FL", branchId);
                    floor.FloorCode = floorCode;

                    await conn.ExecuteAsync(floorSql, floor, transaction: tran);
                }

                // If any floor fails or throws, transaction will roll back everything!
                tran.Commit();

                branch.Floors = floors;
                return branch;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid branchId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM branches WHERE branch_id = @BranchId;";
            return await conn.ExecuteAsync(sql, new { BranchId = branchId }) > 0;
        }
    }
}
