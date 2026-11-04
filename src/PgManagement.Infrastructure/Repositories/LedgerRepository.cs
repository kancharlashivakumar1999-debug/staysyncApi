using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly IDbConnectionFactory _factory;

        public LedgerRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string ChargeProjectionSql = """
            SELECT tc.charge_id AS ChargeId,
                   tc.charge_code AS ChargeCode,
                   tc.guest_id AS GuestId,
                   g.full_name AS GuestName,
                   r.room_number AS RoomNumber,
                   bd.bed_number AS BedNumber,
                   tc.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   tc.charge_type AS ChargeType,
                   tc.billing_month AS BillingMonth,
                   tc.amount AS Amount,
                   tc.due_date AS DueDate,
                   tc.is_paid AS IsPaid,
                   tc.description AS Description,
                   tc.created_at AS CreatedAt
              FROM tenant_charges tc
              JOIN guests g ON tc.guest_id = g.guest_id
              LEFT JOIN rooms r ON g.room_id = r.room_id
              LEFT JOIN beds bd ON g.bed_id = bd.bed_id
              LEFT JOIN branches b ON tc.branch_id = b.branch_id
            """;

        private const string PaymentProjectionSql = """
            SELECT tp.payment_id AS PaymentId,
                   tp.payment_code AS PaymentCode,
                   tp.charge_id AS ChargeId,
                   tp.guest_id AS GuestId,
                   g.full_name AS GuestName,
                   tp.branch_id AS BranchId,
                   tp.amount_paid AS AmountPaid,
                   tp.payment_mode AS PaymentMode,
                   tp.transaction_reference AS TransactionReference,
                   tp.payment_date AS PaymentDate,
                   tp.received_by AS ReceivedBy,
                   tp.receipt_notes AS ReceiptNotes,
                   tp.payment_month AS PaymentMonth,
                   tp.days_paid AS DaysPaid,
                   tp.advance_allocated AS AdvanceAllocated,
                   tp.rent_allocated AS RentAllocated,
                   tp.pending_balance_after AS PendingBalanceAfter
              FROM tenant_payments tp
              JOIN guests g ON tp.guest_id = g.guest_id
            """;

        public async Task<Guid> CreateChargeAsync(TenantCharge charge)
        {
            using var conn = _factory.CreateConnection();
            charge.ChargeId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO tenant_charges (charge_id, charge_code, guest_id, owner_id, branch_id, charge_type, billing_month, amount, due_date, is_paid, description, created_at)
                VALUES (@ChargeId, fn_next_charge_code(), @GuestId, @OwnerId, @BranchId, @ChargeType, @BillingMonth, @Amount, @DueDate, @IsPaid, @Description, @CreatedAt)
                RETURNING charge_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, charge);
        }

        public async Task<Guid> RecordPaymentAsync(TenantPayment payment)
        {
            using var conn = _factory.CreateConnection();
            payment.PaymentId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO tenant_payments (
                    payment_id, payment_code, charge_id, guest_id, owner_id, branch_id,
                    amount_paid, payment_mode, transaction_reference, payment_date,
                    received_by, receipt_notes, payment_month, days_paid,
                    advance_allocated, rent_allocated, pending_balance_after, created_at
                )
                VALUES (
                    @PaymentId, fn_next_payment_code(), @ChargeId, @GuestId, @OwnerId, @BranchId,
                    @AmountPaid, @PaymentMode, @TransactionReference, @PaymentDate,
                    @ReceivedBy, @ReceiptNotes, @PaymentMonth, @DaysPaid,
                    @AdvanceAllocated, @RentAllocated, @PendingBalanceAfter, @CreatedAt
                )
                RETURNING payment_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, payment);
        }

        public async Task<IEnumerable<TenantChargeDto>> GetChargesByGuestIdAsync(Guid guestId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ChargeProjectionSql} WHERE tc.guest_id = @GuestId ORDER BY tc.due_date DESC;";
            return await conn.QueryAsync<TenantChargeDto>(sql, new { GuestId = guestId });
        }

        public async Task<IEnumerable<TenantPaymentDto>> GetPaymentsByGuestIdAsync(Guid guestId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{PaymentProjectionSql} WHERE tp.guest_id = @GuestId ORDER BY tp.payment_date DESC;";
            return await conn.QueryAsync<TenantPaymentDto>(sql, new { GuestId = guestId });
        }

        public async Task<IEnumerable<TenantChargeDto>> GetPendingChargesByOwnerIdAsync(Guid ownerId, Guid? branchId = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ChargeProjectionSql} WHERE tc.owner_id = @OwnerId AND tc.is_paid = false";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                sql += " AND tc.branch_id = @BranchId";
            }
            sql += " ORDER BY tc.due_date ASC;";
            return await conn.QueryAsync<TenantChargeDto>(sql, new { OwnerId = ownerId, BranchId = branchId });
        }

        public async Task<IEnumerable<TenantPaymentDto>> GetRecentPaymentsByOwnerIdAsync(Guid ownerId, Guid? branchId = null, int limit = 50)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{PaymentProjectionSql} WHERE tp.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                sql += " AND tp.branch_id = @BranchId";
            }
            sql += " ORDER BY tp.payment_date DESC LIMIT @Limit;";
            return await conn.QueryAsync<TenantPaymentDto>(sql, new { OwnerId = ownerId, BranchId = branchId, Limit = limit });
        }

        public async Task MarkChargeAsPaidAsync(Guid chargeId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE tenant_charges SET is_paid = true WHERE charge_id = @ChargeId;";
            await conn.ExecuteAsync(sql, new { ChargeId = chargeId });
        }

        public async Task<TenantLedgerSummaryDto> GetGuestLedgerSummaryAsync(Guid guestId)
        {
            using var conn = _factory.CreateConnection();
            var charges = (await GetChargesByGuestIdAsync(guestId)).ToList();
            var payments = (await GetPaymentsByGuestIdAsync(guestId)).ToList();

            const string guestSql = """
                SELECT g.guest_id AS GuestId,
                       g.full_name AS FullName,
                       g.guest_code AS GuestCode,
                       g.phone_number AS PhoneNumber,
                       g.branch_id AS BranchId,
                       g.room_id AS RoomId,
                       g.bed_id AS BedId,
                       g.joining_date AS JoiningDate,
                       g.monthly_rent AS MonthlyRent,
                       g.advance_amount AS AdvanceAmount,
                       r.room_number AS RoomNumber,
                       b.bed_number AS BedNumber
                  FROM guests g
                  LEFT JOIN rooms r ON g.room_id = r.room_id
                  LEFT JOIN beds b ON g.bed_id = b.bed_id
                 WHERE g.guest_id = @GuestId
                 LIMIT 1;
            """;
            var row = (IDictionary<string, object>?)await conn.QueryFirstOrDefaultAsync(guestSql, new { GuestId = guestId });

            string guestName = row != null && row.TryGetValue("fullname", out var fn) && fn != null ? fn.ToString()! : "Unknown Guest";
            if (row != null && guestName == "Unknown Guest" && row.TryGetValue("FullName", out var fnCase) && fnCase != null) guestName = fnCase.ToString()!;

            string guestCode = row != null && (row.TryGetValue("guestcode", out var gc) || row.TryGetValue("GuestCode", out gc)) && gc != null ? gc.ToString()! : string.Empty;
            string phone = row != null && (row.TryGetValue("phonenumber", out var pn) || row.TryGetValue("PhoneNumber", out pn)) && pn != null ? pn.ToString()! : string.Empty;
            string roomNum = row != null && (row.TryGetValue("roomnumber", out var rn) || row.TryGetValue("RoomNumber", out rn)) && rn != null ? rn.ToString()! : string.Empty;
            string bedNum = row != null && (row.TryGetValue("bednumber", out var bn) || row.TryGetValue("BedNumber", out bn)) && bn != null ? bn.ToString()! : string.Empty;

            DateTime joiningDate = DateTime.UtcNow.Date;
            if (row != null && (row.TryGetValue("joiningdate", out var jd) || row.TryGetValue("JoiningDate", out jd)) && jd != null)
            {
                DateTime.TryParse(jd.ToString(), out joiningDate);
            }

            decimal monthlyRent = 0m;
            if (row != null && (row.TryGetValue("monthlyrent", out var mr) || row.TryGetValue("MonthlyRent", out mr)) && mr != null)
            {
                decimal.TryParse(mr.ToString(), out monthlyRent);
            }

            decimal advanceAmount = 0m;
            if (row != null && (row.TryGetValue("advanceamount", out var adv) || row.TryGetValue("AdvanceAmount", out adv)) && adv != null)
            {
                decimal.TryParse(adv.ToString(), out advanceAmount);
            }

            Guid branchId = Guid.Empty;
            if (row != null && (row.TryGetValue("branchid", out var brid) || row.TryGetValue("BranchId", out brid)) && brid != null)
            {
                Guid.TryParse(brid.ToString(), out branchId);
            }

            Guid roomId = Guid.Empty;
            if (row != null && (row.TryGetValue("roomid", out var rmid) || row.TryGetValue("RoomId", out rmid)) && rmid != null)
            {
                Guid.TryParse(rmid.ToString(), out roomId);
            }

            Guid bedId = Guid.Empty;
            if (row != null && (row.TryGetValue("bedid", out var bdid) || row.TryGetValue("BedId", out bdid)) && bdid != null)
            {
                Guid.TryParse(bdid.ToString(), out bedId);
            }

            // --- 30-Day Rate & Current Month Rent Allocation ---
            // Requirement: Daily rate = Monthly Rent / 30.
            // When user joins in a month (e.g. 10th of Nov), remaining days in month = Days in month - Joining Day + 1
            // Amount = (remaining days * daily rate) + Advance Required.
            // If already paid for this month, pending is (rent - rent_paid).
            decimal dailyRate = monthlyRent > 0 ? Math.Round(monthlyRent / 30m, 2) : 0m;
            int daysInCurrentMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            string currentMonthKey = DateTime.UtcNow.ToString("yyyy-MM");

            // Check if resident joined in the current month or earlier
            bool joinedThisMonth = joiningDate.ToString("yyyy-MM") == currentMonthKey;
            int defaultDaysInMonth = joinedThisMonth ? Math.Max(1, daysInCurrentMonth - joiningDate.Day + 1) : 30;
            decimal proratedJoiningRent = Math.Round(dailyRate * defaultDaysInMonth, 2);

            // Calculate total lifetime paid and advance vs rent split
            decimal totalPaid = payments.Sum(p => p.AmountPaid);

            // Compute advance paid & pending
            decimal advancePaid = Math.Min(advanceAmount, totalPaid);
            decimal advancePending = Math.Max(0, advanceAmount - advancePaid);

            // Current Month Payments: filtered by payment_month = currentMonthKey or payment_date in current month
            var currentMonthPayments = payments.Where(p => 
                p.PaymentMonth == currentMonthKey || 
                p.PaymentDate.ToString("yyyy-MM") == currentMonthKey).ToList();

            decimal currentMonthPaid = currentMonthPayments.Sum(p => p.AmountPaid);

            // Current Month Rent Charge: if joined this month -> proratedJoiningRent, else full monthlyRent
            decimal currentMonthRentCharge = joinedThisMonth ? proratedJoiningRent : monthlyRent;

            // Compute charges: if no charges explicitly registered in DB yet, initial charges = Advance + Prorated Rent
            decimal totalCharges = charges.Sum(c => c.Amount);
            if (charges.Count == 0 && (advanceAmount > 0 || monthlyRent > 0))
            {
                totalCharges = advanceAmount + proratedJoiningRent;
            }

            // Total outstanding dues lifetime
            decimal outstandingBalance = Math.Max(0, totalCharges - totalPaid);

            // Waterfall payment breakdown for rent
            decimal rentCharge = totalCharges - advanceAmount;
            decimal remainingPaidForRent = Math.Max(0, totalPaid - advanceAmount);
            decimal rentPaid = Math.Min(rentCharge, remainingPaidForRent);
            decimal rentPending = Math.Max(0, rentCharge - rentPaid);

            // Current Month Rent Pending after advance is fully covered
            // For example: Rent 9000 (300/day * 16 = 4800), Adv 5000 -> Total 9800.
            // Customer pays 9000 -> 5000 to advance, 4000 to rent, leaving pending in November as 800.
            decimal currentMonthPending = (advancePending > 0)
                ? (advancePending + rentPending)
                : rentPending;

            return new TenantLedgerSummaryDto
            {
                GuestId = guestId,
                GuestName = guestName,
                GuestCode = guestCode,
                PhoneNumber = phone,
                BranchId = branchId,
                RoomId = roomId,
                BedId = bedId,
                JoiningDate = joiningDate,
                MonthlyRent = monthlyRent,
                AdvanceAmount = advanceAmount,
                DailyRate = dailyRate,
                CurrentBillingMonth = currentMonthKey,
                CurrentMonthRentCharge = currentMonthRentCharge,
                CurrentMonthPaid = currentMonthPaid,
                CurrentMonthPending = currentMonthPending,
                ProratedJoiningRent = proratedJoiningRent,
                ProratedDaysInMonth = defaultDaysInMonth,
                DaysInJoiningMonth = daysInCurrentMonth,
                TotalCharges = totalCharges,
                TotalPaid = totalPaid,
                OutstandingBalance = outstandingBalance,
                AdvancePaid = advancePaid,
                AdvancePending = advancePending,
                RentPaid = rentPaid,
                RentPending = rentPending,
                RoomBed = $"Room {roomNum} - Bed {bedNum}",
                Charges = charges,
                Payments = payments,
                CurrentMonthPayments = currentMonthPayments
            };
        }
        public async Task<TenantLedgerSummaryDto?> GetBedLedgerSummaryAsync(Guid bedId)
        {
            using var conn = _factory.CreateConnection();
            const string guestSql = """
                SELECT g.guest_id AS GuestId
                  FROM guests g
                 WHERE g.bed_id = @BedId AND g.status = 'Active'
                 ORDER BY g.created_at DESC
                 LIMIT 1;
            """;
            var activeGuestId = await conn.QueryFirstOrDefaultAsync<Guid?>(guestSql, new { BedId = bedId });
            if (!activeGuestId.HasValue || activeGuestId.Value == Guid.Empty)
            {
                return null;
            }

            return await GetGuestLedgerSummaryAsync(activeGuestId.Value);
        }
    }
}
