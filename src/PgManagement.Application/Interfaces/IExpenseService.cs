using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Guid> CreateAsync(Expense expense);
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid ownerId, Guid? branchId = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid ownerId, Guid? branchId = null, string? month = null);
        Task<bool> DeleteAsync(Guid expenseId);
    }

    public interface IExpenseService
    {
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto);
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid ownerId, Guid? branchId = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid ownerId, Guid? branchId = null, string? month = null);
        Task<bool> DeleteExpenseAsync(Guid expenseId);
    }
}
