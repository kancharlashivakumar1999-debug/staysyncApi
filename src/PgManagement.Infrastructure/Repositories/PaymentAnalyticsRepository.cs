using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class PaymentAnalyticsRepository : IPaymentAnalyticsRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PaymentAnalyticsRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<PaymentAnalyticsOverviewDto> GetOverviewAnalyticsAsync(Guid ownerId, Guid? branchId, int year)
        {
            using var conn = _factory.CreateConnection();

            // 1. Current Month Totals
            const string currentMonthSql = """
                SELECT COALESCE(SUM(tp.amount_paid), 0) AS CurrentMonthTotal,
                       COUNT(tp.payment_id) AS CurrentMonthTransactionCount
                  FROM tenant_payments tp
                  LEFT JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                 WHERE tp.owner_id = @OwnerId
                   AND (@BranchId IS NULL OR tp.branch_id = @BranchId)
                   AND DATE_TRUNC('month', tp.payment_date) = DATE_TRUNC('month', CURRENT_DATE)
                   AND (tc.charge_type IS NULL OR tc.charge_type != 'Deposit');
            """;
            var currentMonth = await conn.QueryFirstOrDefaultAsync<dynamic>(currentMonthSql, new { OwnerId = ownerId, BranchId = branchId });

            // 2. Year Totals
            const string yearSql = """
                SELECT COALESCE(SUM(tp.amount_paid), 0) AS YearTotal,
                       COUNT(tp.payment_id) AS YearTransactionCount
                  FROM tenant_payments tp
                  LEFT JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                 WHERE tp.owner_id = @OwnerId
                   AND (@BranchId IS NULL OR tp.branch_id = @BranchId)
                   AND EXTRACT(YEAR FROM tp.payment_date) = @Year
                   AND (tc.charge_type IS NULL OR tc.charge_type != 'Deposit');
            """;
            var yearData = await conn.QueryFirstOrDefaultAsync<dynamic>(yearSql, new { OwnerId = ownerId, BranchId = branchId, Year = year });

            // 3. Lifetime Total
            const string lifetimeSql = """
                SELECT COALESCE(SUM(tp.amount_paid), 0) AS LifetimeTotal
                  FROM tenant_payments tp
                  LEFT JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                 WHERE tp.owner_id = @OwnerId
                   AND (@BranchId IS NULL OR tp.branch_id = @BranchId)
                   AND (tc.charge_type IS NULL OR tc.charge_type != 'Deposit');
            """;
            decimal lifetimeTotal = await conn.ExecuteScalarAsync<decimal>(lifetimeSql, new { OwnerId = ownerId, BranchId = branchId });

            // 4. Monthly Trend for Year
            const string trendSql = """
                SELECT TO_CHAR(tp.payment_date, 'YYYY-MM') AS MonthKey,
                       TO_CHAR(tp.payment_date, 'Mon YYYY') AS MonthLabel,
                       EXTRACT(MONTH FROM tp.payment_date)::INT AS MonthNumber,
                       EXTRACT(YEAR FROM tp.payment_date)::INT AS YearNumber,
                       COALESCE(SUM(tp.amount_paid), 0) AS TotalCollected,
                       COUNT(tp.payment_id) AS TotalTransactions
                  FROM tenant_payments tp
                  LEFT JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                 WHERE tp.owner_id = @OwnerId
                   AND (@BranchId IS NULL OR tp.branch_id = @BranchId)
                   AND EXTRACT(YEAR FROM tp.payment_date) = @Year
                   AND (tc.charge_type IS NULL OR tc.charge_type != 'Deposit')
                 GROUP BY MonthKey, MonthLabel, MonthNumber, YearNumber
                 ORDER BY MonthNumber ASC;
            """;
            var trends = (await conn.QueryAsync<MonthlyCollectionTrendDto>(trendSql, new { OwnerId = ownerId, BranchId = branchId, Year = year })).ToList();

            return new PaymentAnalyticsOverviewDto
            {
                CurrentMonthTotal = currentMonth?.currentmonthtotal ?? 0m,
                CurrentMonthTransactionCount = (int)(currentMonth?.currentmonthtransactioncount ?? 0),
                YearTotal = yearData?.yeartotal ?? 0m,
                YearTransactionCount = (int)(yearData?.yeartransactioncount ?? 0),
                LifetimeTotalCollected = lifetimeTotal,
                MonthlyTrends = trends
            };
        }

        public async Task<IEnumerable<HierarchyCollectionItemDto>> GetHostelCollectionsAsync(Guid ownerId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = """
                SELECT b.branch_id AS Id,
                       b.branch_code AS Code,
                       b.branch_name AS Name,
                       'Hostel' AS Level,
                       NULL::UUID AS ParentId,
                       NULL::VARCHAR AS ParentName,
                       (SELECT COUNT(*) FROM beds WHERE branch_id = b.branch_id) AS TotalBeds,
                       (SELECT COUNT(*) FROM beds WHERE branch_id = b.branch_id AND is_occupied = true) AS OccupiedBeds,
                       (SELECT COUNT(*) FROM guests WHERE branch_id = b.branch_id) AS LifetimeGuestsCount,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE branch_id = b.branch_id), 0) AS TotalAmountCollected,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE branch_id = b.branch_id AND DATE_TRUNC('month', payment_date) = DATE_TRUNC('month', CURRENT_DATE)), 0) AS CurrentMonthAmountCollected
                  FROM branches b
                 WHERE b.owner_id = @OwnerId
                 ORDER BY TotalAmountCollected DESC;
            """;
            return await conn.QueryAsync<HierarchyCollectionItemDto>(sql, new { OwnerId = ownerId });
        }

        public async Task<IEnumerable<HierarchyCollectionItemDto>> GetFloorCollectionsAsync(Guid ownerId, Guid? branchId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = """
                SELECT f.floor_id AS Id,
                       f.floor_code AS Code,
                       f.floor_name AS Name,
                       'Floor' AS Level,
                       f.branch_id AS ParentId,
                       b.branch_name AS ParentName,
                       (SELECT COUNT(*) FROM beds WHERE floor_id = f.floor_id) AS TotalBeds,
                       (SELECT COUNT(*) FROM beds WHERE floor_id = f.floor_id AND is_occupied = true) AS OccupiedBeds,
                       (SELECT COUNT(*) FROM guests WHERE floor_id = f.floor_id) AS LifetimeGuestsCount,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE floor_id = f.floor_id), 0) AS TotalAmountCollected,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE floor_id = f.floor_id AND DATE_TRUNC('month', payment_date) = DATE_TRUNC('month', CURRENT_DATE)), 0) AS CurrentMonthAmountCollected
                  FROM floors f
                  JOIN branches b ON f.branch_id = b.branch_id
                 WHERE b.owner_id = @OwnerId
                   AND (@BranchId IS NULL OR f.branch_id = @BranchId)
                 ORDER BY f.floor_number ASC;
            """;
            return await conn.QueryAsync<HierarchyCollectionItemDto>(sql, new { OwnerId = ownerId, BranchId = branchId });
        }

        public async Task<IEnumerable<HierarchyCollectionItemDto>> GetRoomCollectionsAsync(Guid ownerId, Guid? floorId, Guid? branchId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = """
                SELECT r.room_id AS Id,
                       r.room_code AS Code,
                       CONCAT('Room ', r.room_number, ' (', r.room_type, ')') AS Name,
                       'Room' AS Level,
                       r.floor_id AS ParentId,
                       f.floor_name AS ParentName,
                       r.total_beds AS TotalBeds,
                       (SELECT COUNT(*) FROM beds WHERE room_id = r.room_id AND is_occupied = true) AS OccupiedBeds,
                       (SELECT COUNT(*) FROM guests WHERE room_id = r.room_id) AS LifetimeGuestsCount,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE room_id = r.room_id), 0) AS TotalAmountCollected,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE room_id = r.room_id AND DATE_TRUNC('month', payment_date) = DATE_TRUNC('month', CURRENT_DATE)), 0) AS CurrentMonthAmountCollected
                  FROM rooms r
                  JOIN floors f ON r.floor_id = f.floor_id
                  JOIN branches b ON r.branch_id = b.branch_id
                 WHERE b.owner_id = @OwnerId
                   AND (@FloorId IS NULL OR r.floor_id = @FloorId)
                   AND (@BranchId IS NULL OR r.branch_id = @BranchId)
                 ORDER BY r.room_number ASC;
            """;
            return await conn.QueryAsync<HierarchyCollectionItemDto>(sql, new { OwnerId = ownerId, FloorId = floorId, BranchId = branchId });
        }

        public async Task<IEnumerable<HierarchyCollectionItemDto>> GetBedCollectionsAsync(Guid ownerId, Guid? roomId, Guid? branchId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = """
                SELECT bd.bed_id AS Id,
                       bd.bed_code AS Code,
                       CONCAT('Bed ', bd.bed_number, ' (', bd.status, ')') AS Name,
                       'Bed' AS Level,
                       bd.room_id AS ParentId,
                       r.room_number AS ParentName,
                       1 AS TotalBeds,
                       CASE WHEN bd.is_occupied THEN 1 ELSE 0 END AS OccupiedBeds,
                       (SELECT COUNT(*) FROM guests WHERE bed_id = bd.bed_id) AS LifetimeGuestsCount,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE bed_id = bd.bed_id), 0) AS TotalAmountCollected,
                       COALESCE((SELECT SUM(amount_paid) FROM view_rent_and_fee_payments WHERE bed_id = bd.bed_id AND DATE_TRUNC('month', payment_date) = DATE_TRUNC('month', CURRENT_DATE)), 0) AS CurrentMonthAmountCollected
                  FROM beds bd
                  JOIN rooms r ON bd.room_id = r.room_id
                  JOIN branches b ON bd.branch_id = b.branch_id
                 WHERE b.owner_id = @OwnerId
                   AND (@RoomId IS NULL OR bd.room_id = @RoomId)
                   AND (@BranchId IS NULL OR bd.branch_id = @BranchId)
                 ORDER BY bd.bed_number ASC;
            """;
            return await conn.QueryAsync<HierarchyCollectionItemDto>(sql, new { OwnerId = ownerId, RoomId = roomId, BranchId = branchId });
        }

        public async Task<BedLifetimeHistoryDto?> GetBedLifetimeHistoryAsync(Guid bedId)
        {
            using var conn = _factory.CreateConnection();

            // 1. Bed Header Details
            const string bedSql = """
                SELECT bd.bed_id AS BedId,
                       bd.bed_code AS BedCode,
                       bd.bed_number AS BedNumber,
                       bd.bed_rent AS BedRent,
                       bd.is_occupied AS IsOccupied,
                       bd.status AS Status,
                       r.room_id AS RoomId,
                       r.room_number AS RoomNumber,
                       r.room_code AS RoomCode,
                       f.floor_id AS FloorId,
                       f.floor_name AS FloorName,
                       b.branch_id AS BranchId,
                       b.branch_name AS BranchName,
                       COALESCE(SUM(v.amount_paid), 0) AS TotalAmountCollectedTillNow
                  FROM beds bd
                  JOIN rooms r ON bd.room_id = r.room_id
                  JOIN floors f ON bd.floor_id = f.floor_id
                  JOIN branches b ON bd.branch_id = b.branch_id
                  LEFT JOIN view_rent_and_fee_payments v ON bd.bed_id = v.bed_id
                 WHERE bd.bed_id = @BedId
                 GROUP BY bd.bed_id, bd.bed_code, bd.bed_number, bd.bed_rent, bd.is_occupied, bd.status,
                          r.room_id, r.room_number, r.room_code, f.floor_id, f.floor_name, b.branch_id, b.branch_name
                 LIMIT 1;
            """;
            var bedHeader = await conn.QueryFirstOrDefaultAsync<BedLifetimeHistoryDto>(bedSql, new { BedId = bedId });
            if (bedHeader == null) return null;

            // 2. Guest Stays on this Bed (Active at top, then descending order by joining_date)
            const string guestsSql = """
                SELECT g.guest_id AS GuestId,
                       g.guest_code AS GuestCode,
                       g.full_name AS FullName,
                       g.phone_number AS PhoneNumber,
                       g.status AS Status,
                       (g.status = 'Active') AS IsCurrentGuest,
                       g.joining_date AS JoiningDate,
                       g.actual_vacating_date AS VacatingDate,
                       g.monthly_rent AS MonthlyRentAtStay,
                       g.advance_amount AS AdvanceAmount,
                       COALESCE(SUM(v.amount_paid), 0) AS RentCollectedFromGuest
                  FROM guests g
                  LEFT JOIN view_rent_and_fee_payments v ON g.guest_id = v.guest_id
                 WHERE g.bed_id = @BedId
                 GROUP BY g.guest_id, g.guest_code, g.full_name, g.phone_number, g.status, g.joining_date, g.actual_vacating_date, g.monthly_rent, g.advance_amount
                 ORDER BY (CASE WHEN g.status = 'Active' THEN 0 ELSE 1 END) ASC, g.joining_date DESC;
            """;
            var guests = (await conn.QueryAsync<BedGuestStayHistoryDto>(guestsSql, new { BedId = bedId })).ToList();
            bedHeader.GuestsHistory = guests;
            bedHeader.TotalGuestsStayed = guests.Count;

            // 3. Detailed Payment Logs for this bed
            const string paymentsSql = """
                SELECT v.payment_id AS PaymentId,
                       v.payment_code AS PaymentCode,
                       v.charge_id AS ChargeId,
                       v.guest_id AS GuestId,
                       g.full_name AS GuestName,
                       v.branch_id AS BranchId,
                       v.amount_paid AS AmountPaid,
                       v.payment_mode AS PaymentMode,
                       v.transaction_reference AS TransactionReference,
                       v.payment_date AS PaymentDate,
                       v.received_by AS ReceivedBy,
                       v.receipt_notes AS ReceiptNotes
                  FROM view_rent_and_fee_payments v
                  JOIN guests g ON v.guest_id = g.guest_id
                 WHERE v.bed_id = @BedId
                 ORDER BY v.payment_date DESC
                 LIMIT 100;
            """;
            bedHeader.PaymentLogs = (await conn.QueryAsync<TenantPaymentDto>(paymentsSql, new { BedId = bedId })).ToList();

            return bedHeader;
        }
    }
}
