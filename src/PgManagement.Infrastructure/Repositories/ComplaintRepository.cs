using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly IDbConnectionFactory _factory;

        public ComplaintRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string ComplaintProjectionSql = """
            SELECT c.complaint_id AS ComplaintId,
                   c.complaint_code AS ComplaintCode,
                   c.owner_id AS OwnerId,
                   c.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   c.guest_id AS GuestId,
                   g.full_name AS GuestName,
                   r.room_number AS RoomNumber,
                   c.category AS Category,
                   c.title AS Title,
                   c.description AS Description,
                   c.priority AS Priority,
                   c.status AS Status,
                   c.assigned_to AS AssignedTo,
                   c.resolution_notes AS ResolutionNotes,
                   c.resolved_at AS ResolvedAt,
                   c.created_at AS CreatedAt,
                   c.updated_at AS UpdatedAt
              FROM complaints c
              LEFT JOIN branches b ON c.branch_id = b.branch_id
              LEFT JOIN guests g ON c.guest_id = g.guest_id
              LEFT JOIN rooms r ON c.room_id = r.room_id
            """;

        public async Task<Guid> CreateAsync(Complaint complaint)
        {
            using var conn = _factory.CreateConnection();
            complaint.ComplaintId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO complaints (complaint_id, complaint_code, owner_id, branch_id, guest_id, room_id, category, title, description, priority, status, created_at, updated_at)
                VALUES (@ComplaintId, fn_next_complaint_code(), @OwnerId, @BranchId, @GuestId, @RoomId, @Category, @Title, @Description, @Priority, @Status, @CreatedAt, @UpdatedAt)
                RETURNING complaint_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, complaint);
        }

        public async Task<IEnumerable<ComplaintDto>> GetComplaintsAsync(Guid ownerId, Guid? branchId = null, string? status = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ComplaintProjectionSql} WHERE c.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty) sql += " AND c.branch_id = @BranchId";
            if (!string.IsNullOrWhiteSpace(status)) sql += " AND c.status = @Status";
            sql += " ORDER BY CASE c.priority WHEN 'Emergency' THEN 1 WHEN 'High' THEN 2 WHEN 'Medium' THEN 3 ELSE 4 END, c.created_at DESC;";

            return await conn.QueryAsync<ComplaintDto>(sql, new { OwnerId = ownerId, BranchId = branchId, Status = status });
        }

        public async Task<ComplaintDto?> GetByIdAsync(Guid complaintId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ComplaintProjectionSql} WHERE c.complaint_id = @ComplaintId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<ComplaintDto>(sql, new { ComplaintId = complaintId });
        }

        public async Task<bool> UpdateStatusAsync(Guid complaintId, string status, string? assignedTo, string? resolutionNotes, DateTime? resolvedAt)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE complaints
                SET status = @Status,
                    assigned_to = COALESCE(@AssignedTo, assigned_to),
                    resolution_notes = COALESCE(@ResolutionNotes, resolution_notes),
                    resolved_at = @ResolvedAt,
                    updated_at = CURRENT_TIMESTAMP
                WHERE complaint_id = @ComplaintId;";

            return await conn.ExecuteAsync(sql, new { ComplaintId = complaintId, Status = status, AssignedTo = assignedTo, ResolutionNotes = resolutionNotes, ResolvedAt = resolvedAt }) > 0;
        }
    }
}
