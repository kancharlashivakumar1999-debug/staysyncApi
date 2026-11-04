using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class VisitorRepository : IVisitorRepository
    {
        private readonly IDbConnectionFactory _factory;

        public VisitorRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string VisitorProjectionSql = """
            SELECT v.visitor_id AS VisitorId,
                   v.visitor_code AS VisitorCode,
                   v.owner_id AS OwnerId,
                   v.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   v.guest_id AS GuestId,
                   g.full_name AS GuestName,
                   r.room_number AS RoomNumber,
                   v.visitor_name AS VisitorName,
                   v.visitor_phone AS VisitorPhone,
                   v.relation_to_guest AS RelationToGuest,
                   v.purpose_of_visit AS PurposeOfVisit,
                   v.vehicle_number AS VehicleNumber,
                   v.check_in_time AS CheckInTime,
                   v.check_out_time AS CheckOutTime,
                   v.id_proof_type AS IdProofType,
                   v.id_proof_number AS IdProofNumber
              FROM visitors v
              LEFT JOIN branches b ON v.branch_id = b.branch_id
              LEFT JOIN guests g ON v.guest_id = g.guest_id
              LEFT JOIN rooms r ON g.room_id = r.room_id
            """;

        public async Task<Guid> CreateAsync(Visitor visitor)
        {
            using var conn = _factory.CreateConnection();
            visitor.VisitorId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO visitors (visitor_id, visitor_code, owner_id, branch_id, guest_id, visitor_name, visitor_phone, relation_to_guest, purpose_of_visit, vehicle_number, check_in_time, id_proof_type, id_proof_number, created_at)
                VALUES (@VisitorId, fn_next_visitor_code(), @OwnerId, @BranchId, @GuestId, @VisitorName, @VisitorPhone, @RelationToGuest, @PurposeOfVisit, @VehicleNumber, @CheckInTime, @IdProofType, @IdProofNumber, @CreatedAt)
                RETURNING visitor_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, visitor);
        }

        public async Task<IEnumerable<VisitorDto>> GetVisitorsAsync(Guid ownerId, Guid? branchId = null, Guid? guestId = null, DateTime? date = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{VisitorProjectionSql} WHERE v.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty) sql += " AND v.branch_id = @BranchId";
            if (guestId.HasValue && guestId.Value != Guid.Empty) sql += " AND v.guest_id = @GuestId";
            if (date.HasValue) sql += " AND DATE(v.check_in_time) = DATE(@Date)";
            sql += " ORDER BY v.check_in_time DESC;";

            return await conn.QueryAsync<VisitorDto>(sql, new { OwnerId = ownerId, BranchId = branchId, GuestId = guestId, Date = date });
        }

        public async Task<bool> CheckOutVisitorAsync(Guid visitorId, DateTime checkOutTime)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE visitors SET check_out_time = @CheckOutTime WHERE visitor_id = @VisitorId;";
            return await conn.ExecuteAsync(sql, new { VisitorId = visitorId, CheckOutTime = checkOutTime }) > 0;
        }
    }
}
