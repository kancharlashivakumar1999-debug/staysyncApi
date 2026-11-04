using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IDbConnectionFactory _factory;

        public ExpenseRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string ExpenseProjectionSql = """
            SELECT e.expense_id AS ExpenseId,
                   e.expense_code AS ExpenseCode,
                   e.owner_id AS OwnerId,
                   e.branch_id AS BranchId,
                   b.branch_name AS BranchName,
                   e.category AS Category,
                   e.title AS Title,
                   e.amount AS Amount,
                   e.expense_date AS ExpenseDate,
                   e.vendor_name AS VendorName,
                   e.payment_mode AS PaymentMode,
                   e.receipt_url AS ReceiptUrl,
                   e.notes AS Notes,
                   e.created_at AS CreatedAt
              FROM property_expenses e
              LEFT JOIN branches b ON e.branch_id = b.branch_id
            """;

        public async Task<Guid> CreateAsync(Expense expense)
        {
            using var conn = _factory.CreateConnection();
            expense.ExpenseId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO property_expenses (expense_id, expense_code, owner_id, branch_id, category, title, amount, expense_date, vendor_name, payment_mode, receipt_url, notes, created_at)
                VALUES (@ExpenseId, fn_next_expense_code(), @OwnerId, @BranchId, @Category, @Title, @Amount, @ExpenseDate, @VendorName, @PaymentMode, @ReceiptUrl, @Notes, @CreatedAt)
                RETURNING expense_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, expense);
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid ownerId, Guid? branchId = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{ExpenseProjectionSql} WHERE e.owner_id = @OwnerId";
            if (branchId.HasValue && branchId.Value != Guid.Empty) sql += " AND e.branch_id = @BranchId";
            if (!string.IsNullOrWhiteSpace(category)) sql += " AND e.category = @Category";
            if (fromDate.HasValue) sql += " AND e.expense_date >= @FromDate";
            if (toDate.HasValue) sql += " AND e.expense_date <= @ToDate";
            sql += " ORDER BY e.expense_date DESC, e.created_at DESC;";

            return await conn.QueryAsync<ExpenseDto>(sql, new { OwnerId = ownerId, BranchId = branchId, Category = category, FromDate = fromDate, ToDate = toDate });
        }

        public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid ownerId, Guid? branchId = null, string? month = null)
        {
            using var conn = _factory.CreateConnection();
            string sql = @"
                SELECT e.category, SUM(e.amount) AS total
                  FROM property_expenses e
                 WHERE e.owner_id = @OwnerId";

            if (branchId.HasValue && branchId.Value != Guid.Empty) sql += " AND e.branch_id = @BranchId";
            if (!string.IsNullOrWhiteSpace(month)) sql += " AND TO_CHAR(e.expense_date, 'YYYY-MM') = @Month";
            sql += " GROUP BY e.category;";

            var rows = await conn.QueryAsync<dynamic>(sql, new { OwnerId = ownerId, BranchId = branchId, Month = month });
            var summary = new ExpenseSummaryDto();
            foreach (var r in rows)
            {
                string cat = (string)r.category;
                decimal amt = (decimal)r.total;
                summary.ByCategory[cat] = amt;
                summary.TotalExpenses += amt;
            }
            return summary;
        }

        public async Task<bool> DeleteAsync(Guid expenseId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM property_expenses WHERE expense_id = @ExpenseId;";
            return await conn.ExecuteAsync(sql, new { ExpenseId = expenseId }) > 0;
        }
    }
}
