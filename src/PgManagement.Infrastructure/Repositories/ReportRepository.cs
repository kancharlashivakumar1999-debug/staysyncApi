using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _factory;

        public ReportRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string ClosingProjectionSql = """
            SELECT mc.closing_id AS ClosingId,
                   mc.closing_code AS ClosingCode,
                   mc.owner_id AS OwnerId,
                   mc.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   mc.closing_month AS ClosingMonth,
                   mc.total_rent_collected AS TotalRentCollected,
                   mc.total_other_income AS TotalOtherIncome,
                   mc.total_expenses AS TotalExpenses,
                   mc.net_operating_income AS NetOperatingIncome,
                   mc.total_outstanding_dues AS TotalOutstandingDues,
                   mc.active_tenants_count AS ActiveTenantsCount,
                   mc.occupancy_rate AS OccupancyRate,
                   mc.closed_by AS ClosedBy,
                   mc.closed_at AS ClosedAt,
                   mc.notes AS Notes
              FROM monthly_closings mc
              LEFT JOIN branches b ON mc.branch_id = b.branch_id
            """;

        public async Task<Guid> CreateClosingAsync(MonthlyClosing closing)
        {
            using var conn = _factory.CreateConnection();
            closing.ClosingId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO monthly_closings (
                    closing_id, closing_code, owner_id, branch_id, closing_month,
                    total_rent_collected, total_other_income, total_expenses,
                    net_operating_income, total_outstanding_dues, active_tenants_count,
                    occupancy_rate, closed_by, closed_at, notes
                ) VALUES (
                    @ClosingId, fn_next_closing_code(), @OwnerId, @BranchId, @ClosingMonth,
                    @TotalRentCollected, @TotalOtherIncome, @TotalExpenses,
                    @NetOperatingIncome, @TotalOutstandingDues, @ActiveTenantsCount,
                    @OccupancyRate, @ClosedBy, @ClosedAt, @Notes
                ) RETURNING closing_id;";

            return await conn.ExecuteScalarAsync<Guid>(sql, closing);
        }

        public async Task<IEnumerable<MonthlyClosingDto>> GetClosingsAsync(Guid ownerId, Guid? branchId = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ClosingProjectionSql} WHERE mc.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty) sql += " AND mc.branch_id = @BranchId";
            sql += " ORDER BY mc.closing_month DESC, mc.closed_at DESC;";

            return await conn.QueryAsync<MonthlyClosingDto>(sql, new { OwnerId = ownerId, BranchId = branchId });
        }

        public async Task<FinancialProfitLossDto> CalculateMonthlyPLAsync(Guid ownerId, Guid? branchId, string month)
        {
            using var conn = _factory.CreateConnection();

            bool hasBranch = branchId.HasValue && branchId.Value != Guid.Empty;

            string branchCondTp = hasBranch ? "AND tp.branch_id = @BranchId" : "";
            string branchCondPe = hasBranch ? "AND pe.branch_id = @BranchId" : "";
            string branchCondTc = hasBranch ? "AND tc.branch_id = @BranchId" : "";
            string branchCondBr = hasBranch ? "AND br.branch_id = @BranchId" : "";

            string sql = $"""
                SELECT 
                    COALESCE((
                        SELECT SUM(amount_paid) 
                          FROM tenant_payments tp
                         WHERE tp.owner_id = @OwnerId {branchCondTp}
                           AND TO_CHAR(tp.payment_date, 'YYYY-MM') = @Month
                    ), 0) AS TotalIncome,
                    COALESCE((
                        SELECT SUM(tp.amount_paid) 
                          FROM tenant_payments tp
                          LEFT JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                         WHERE tp.owner_id = @OwnerId {branchCondTp}
                           AND (tc.charge_type = 'Rent' OR tc.charge_type IS NULL)
                           AND TO_CHAR(tp.payment_date, 'YYYY-MM') = @Month
                    ), 0) AS RentCollected,
                    COALESCE((
                        SELECT SUM(tp.amount_paid) 
                          FROM tenant_payments tp
                          JOIN tenant_charges tc ON tp.charge_id = tc.charge_id
                         WHERE tp.owner_id = @OwnerId {branchCondTp}
                           AND tc.charge_type <> 'Rent'
                           AND TO_CHAR(tp.payment_date, 'YYYY-MM') = @Month
                    ), 0) AS UtilitiesCollected,
                    COALESCE((
                        SELECT SUM(amount) 
                          FROM property_expenses pe
                         WHERE pe.owner_id = @OwnerId {branchCondPe}
                           AND TO_CHAR(pe.expense_date, 'YYYY-MM') = @Month
                    ), 0) AS TotalExpenses,
                    COALESCE((
                        SELECT SUM(amount)
                          FROM tenant_charges tc
                         WHERE tc.owner_id = @OwnerId {branchCondTc}
                           AND tc.is_paid = false
                    ), 0) AS TotalOutstandingDues,
                    (
                        SELECT COUNT(*)
                          FROM beds b
                          JOIN branches br ON b.branch_id = br.branch_id
                         WHERE br.owner_id = @OwnerId {branchCondBr}
                    ) AS TotalBeds,
                    (
                        SELECT COUNT(*)
                          FROM beds b
                          JOIN branches br ON b.branch_id = br.branch_id
                         WHERE br.owner_id = @OwnerId {branchCondBr}
                           AND b.is_occupied = true
                    ) AS OccupiedBeds;
            """;

            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { OwnerId = ownerId, BranchId = branchId, Month = month });
            decimal totalIncome = (decimal)(row?.TotalIncome ?? 0m);
            decimal rentColl = (decimal)(row?.RentCollected ?? 0m);
            decimal utilColl = (decimal)(row?.UtilitiesCollected ?? 0m);
            decimal expenses = (decimal)(row?.TotalExpenses ?? 0m);
            decimal dues = (decimal)(row?.TotalOutstandingDues ?? 0m);
            long totalBeds = (long)(row?.TotalBeds ?? 0L);
            long occupiedBeds = (long)(row?.OccupiedBeds ?? 0L);
            decimal occupancy = totalBeds > 0 ? Math.Round(((decimal)occupiedBeds / totalBeds) * 100, 2) : 0m;

            return new FinancialProfitLossDto
            {
                Month = month,
                TotalIncome = totalIncome,
                RentCollected = rentColl,
                UtilitiesCollected = utilColl,
                TotalExpenses = expenses,
                NetProfit = totalIncome - expenses,
                TotalOutstandingDues = dues,
                OccupancyPercentage = occupancy
            };
        }
    }
}
