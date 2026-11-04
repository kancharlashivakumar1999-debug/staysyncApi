using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly IDbConnectionFactory _factory;

        public StaffRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string StaffProjectionSql = """
            SELECT s.staff_id AS StaffId,
                   s.staff_code AS StaffCode,
                   s.owner_id AS OwnerId,
                   s.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   s.full_name AS FullName,
                   s.email AS Email,
                   s.phone_number AS PhoneNumber,
                   s.role AS Role,
                   s.salary AS Salary,
                   s.joining_date AS JoiningDate,
                   s.is_active AS IsActive
              FROM staff_members s
              LEFT JOIN branches b ON s.branch_id = b.branch_id
            """;

        public async Task<IEnumerable<StaffDto>> GetByOwnerIdAsync(Guid ownerId, Guid? branchId = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{StaffProjectionSql} WHERE s.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                sql += " AND s.branch_id = @BranchId";
            }
            sql += " ORDER BY s.staff_code ASC;";
            return await conn.QueryAsync<StaffDto>(sql, new { OwnerId = ownerId, BranchId = branchId });
        }

        public async Task<StaffMember?> GetByIdAsync(Guid staffId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = """
                SELECT s.staff_id AS StaffId,
                       s.staff_code AS StaffCode,
                       s.owner_id AS OwnerId,
                       s.branch_id AS BranchId,
                       s.full_name AS FullName,
                       s.email AS Email,
                       s.phone_number AS PhoneNumber,
                       s.role AS Role,
                       s.salary AS Salary,
                       s.joining_date AS JoiningDate,
                       s.is_active AS IsActive,
                       s.created_at AS CreatedAt,
                       s.updated_at AS UpdatedAt
                  FROM staff_members s
                 WHERE s.staff_id = @StaffId
                 LIMIT 1;
            """;
            return await conn.QueryFirstOrDefaultAsync<StaffMember>(sql, new { StaffId = staffId });
        }

        public async Task<Guid> CreateAsync(StaffMember staff)
        {
            using var conn = _factory.CreateConnection();
            staff.StaffId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO staff_members (staff_id, staff_code, owner_id, branch_id, full_name, email, phone_number, role, salary, joining_date, is_active, created_at, updated_at)
                VALUES (@StaffId, fn_next_staff_code(), @OwnerId, @BranchId, @FullName, @Email, @PhoneNumber, @Role, @Salary, @JoiningDate, @IsActive, @CreatedAt, @UpdatedAt)
                RETURNING staff_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, staff);
        }

        public async Task<bool> UpdateStatusAsync(Guid staffId, bool isActive)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE staff_members SET is_active = @IsActive, updated_at = CURRENT_TIMESTAMP WHERE staff_id = @StaffId;";
            return await conn.ExecuteAsync(sql, new { StaffId = staffId, IsActive = isActive }) > 0;
        }
    }
}
