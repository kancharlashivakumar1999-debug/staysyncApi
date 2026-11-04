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

        public BranchRepository(IDbConnectionFactory factory) => _factory = factory;

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
            branch.BranchId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO branches (branch_id, owner_id, branch_code, branch_name, contact_number, address_line, city, state, pincode, total_floors, created_at, updated_at)
                VALUES (@BranchId, @OwnerId, fn_next_branch_code(), @BranchName, @ContactNumber, @AddressLine, @City, @State, @Pincode, @TotalFloors, @CreatedAt, @UpdatedAt)
                RETURNING branch_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, branch);
        }

        public async Task<bool> DeleteAsync(Guid branchId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM branches WHERE branch_id = @BranchId;";
            return await conn.ExecuteAsync(sql, new { BranchId = branchId }) > 0;
        }
    }
}
