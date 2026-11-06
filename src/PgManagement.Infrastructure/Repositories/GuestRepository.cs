using System.Data;
using Dapper;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly IDbConnectionFactory _factory;

        public GuestRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string GuestProjectionSql = """
            SELECT g.guest_id AS GuestId,
                   g.guest_code AS GuestCode,
                   g.owner_id AS OwnerId,
                   g.branch_id AS BranchId,
                   g.floor_id AS FloorId,
                   g.room_id AS RoomId,
                   g.bed_id AS BedId,
                   g.full_name AS FullName,
                   g.email AS Email,
                   g.phone_number AS PhoneNumber,
                   g.emergency_contact AS EmergencyContact,
                   g.age AS Age,
                   g.gender AS Gender,
                   g.occupation AS Occupation,
                   g.workplace_or_college AS WorkplaceOrCollege,
                   g.tenant_type AS TenantType,
                   g.advance_amount AS AdvanceAmount,
                   g.monthly_rent AS MonthlyRent,
                   g.daily_rent AS DailyRent,
                   g.joining_date AS JoiningDate,
                   g.expected_vacating_date AS ExpectedVacatingDate,
                   g.actual_vacating_date AS ActualVacatingDate,
                   g.days_before_vacate_reminder AS DaysBeforeVacateReminder,
                   g.vacate_notice_served AS VacateNoticeServed,
                   g.vacating_returned_amount AS VacatingReturnedAmount,
                   g.deductions_amount AS DeductionsAmount,
                   g.settlement_notes AS SettlementNotes,
                   g.proof_document_type AS ProofDocumentType,
                   g.proof_document_number AS ProofDocumentNumber,
                   g.proof_document_url AS ProofDocumentUrl,
                   g.status AS Status,
                   g.created_at AS CreatedAt,
                   g.updated_at AS UpdatedAt
              FROM guests g
            """;

        public async Task<IEnumerable<Guest>> GetByOwnerIdAsync(Guid ownerId, Guid? branchId = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{GuestProjectionSql} WHERE g.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                sql += " AND g.branch_id = @BranchId";
            }
            sql += " ORDER BY g.guest_code DESC;";
            return await conn.QueryAsync<Guest>(sql, new { OwnerId = ownerId, BranchId = branchId });
        }

        public async Task<Guest?> GetByIdAsync(Guid guestId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{GuestProjectionSql} WHERE g.guest_id = @GuestId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Guest>(sql, new { GuestId = guestId });
        }

        public async Task<Guid> CreateAsync(Guest guest)
        {
            using var conn = _factory.CreateConnection();
            guest.GuestId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO guests (
                    guest_id, guest_code, owner_id, branch_id, floor_id, room_id, bed_id, full_name, email, phone_number,
                    emergency_contact, age, gender, occupation, workplace_or_college, tenant_type,
                    advance_amount, monthly_rent, daily_rent, joining_date, expected_vacating_date,
                    days_before_vacate_reminder, vacate_notice_served, proof_document_type,
                    proof_document_number, proof_document_url, status, created_at, updated_at
                ) VALUES (
                    @GuestId, fn_next_guest_code(), @OwnerId, @BranchId, @FloorId, @RoomId, @BedId, @FullName, @Email, @PhoneNumber,
                    @EmergencyContact, @Age, @Gender, @Occupation, @WorkplaceOrCollege, @TenantType,
                    @AdvanceAmount, @MonthlyRent, @DailyRent, @JoiningDate, @ExpectedVacatingDate,
                    @DaysBeforeVacateReminder, @VacateNoticeServed, @ProofDocumentType,
                    @ProofDocumentNumber, @ProofDocumentUrl, @Status, @CreatedAt, @UpdatedAt
                ) RETURNING guest_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, guest);
        }

        public async Task<Guid> CheckInGuestWithOccupancyAsync(Guest guest)
        {
            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                // Verify bed is still vacant inside transaction with row lock
                const string checkBedSql = "SELECT is_occupied FROM beds WHERE bed_id = @BedId FOR UPDATE;";
                var isOccupied = await conn.ExecuteScalarAsync<bool?>(checkBedSql, new { guest.BedId }, transaction: tran);
                if (isOccupied == true)
                {
                    throw new InvalidOperationException("This bed has already been occupied by another resident.");
                }

                guest.GuestId = Uuid7.NewGuid();
                // Generate guest code
                var code = await conn.ExecuteScalarAsync<string>(
                    "SELECT config.next_doc_no('G', @BranchId);",
                    new { guest.BranchId },
                    transaction: tran
                ) ?? ("G-" + Guid.NewGuid().ToString().Substring(0, 4));
                guest.GuestCode = code;

                const string guestSql = @"
                    INSERT INTO guests (
                        guest_id, guest_code, owner_id, branch_id, floor_id, room_id, bed_id, full_name, email, phone_number,
                        emergency_contact, age, gender, occupation, workplace_or_college, tenant_type,
                        advance_amount, monthly_rent, daily_rent, joining_date, expected_vacating_date,
                        days_before_vacate_reminder, vacate_notice_served, proof_document_type,
                        proof_document_number, proof_document_url, status, created_at, updated_at
                    ) VALUES (
                        @GuestId, @GuestCode, @OwnerId, @BranchId, @FloorId, @RoomId, @BedId, @FullName, @Email, @PhoneNumber,
                        @EmergencyContact, @Age, @Gender, @Occupation, @WorkplaceOrCollege, @TenantType,
                        @AdvanceAmount, @MonthlyRent, @DailyRent, @JoiningDate, @ExpectedVacatingDate,
                        @DaysBeforeVacateReminder, @VacateNoticeServed, @ProofDocumentType,
                        @ProofDocumentNumber, @ProofDocumentUrl, @Status, @CreatedAt, @UpdatedAt
                    ) RETURNING guest_id;";

                var guestId = await conn.ExecuteScalarAsync<Guid>(guestSql, guest, transaction: tran);

                // Update bed status in the exact same transaction
                const string updateBedSql = "UPDATE beds SET is_occupied = TRUE, status = 'Occupied' WHERE bed_id = @BedId;";
                await conn.ExecuteAsync(updateBedSql, new { guest.BedId }, transaction: tran);

                tran.Commit();
                return guestId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task SettleAndVacateAsync(Guid guestId, decimal returnedAmount, decimal deductionsAmount, string? notes, DateTime vacateDate)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE guests
                SET status = 'Vacated',
                    actual_vacating_date = @VacateDate,
                    vacating_returned_amount = @ReturnedAmount,
                    deductions_amount = @DeductionsAmount,
                    settlement_notes = @Notes,
                    updated_at = CURRENT_TIMESTAMP
                WHERE guest_id = @GuestId;";
            await conn.ExecuteAsync(sql, new { GuestId = guestId, VacateDate = vacateDate, ReturnedAmount = returnedAmount, DeductionsAmount = deductionsAmount, Notes = notes });
        }
    }
}
