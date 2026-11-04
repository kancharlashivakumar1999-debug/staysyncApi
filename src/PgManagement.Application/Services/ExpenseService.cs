using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepo;

        public ExpenseService(IExpenseRepository expenseRepo)
        {
            _expenseRepo = expenseRepo;
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto)
        {
            var exp = new Expense
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                Category = dto.Category,
                Title = dto.Title,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                VendorName = dto.VendorName,
                PaymentMode = dto.PaymentMode,
                ReceiptUrl = dto.ReceiptUrl,
                Notes = dto.Notes
            };

            var id = await _expenseRepo.CreateAsync(exp);
            return new ExpenseDto
            {
                ExpenseId = id,
                ExpenseCode = exp.ExpenseCode,
                OwnerId = exp.OwnerId,
                BranchId = exp.BranchId,
                Category = exp.Category,
                Title = exp.Title,
                Amount = exp.Amount,
                ExpenseDate = exp.ExpenseDate,
                VendorName = exp.VendorName,
                PaymentMode = exp.PaymentMode,
                ReceiptUrl = exp.ReceiptUrl,
                Notes = exp.Notes,
                CreatedAt = exp.CreatedAt
            };
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid ownerId, Guid? branchId = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null) =>
            await _expenseRepo.GetExpensesAsync(ownerId, branchId, category, fromDate, toDate);

        public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid ownerId, Guid? branchId = null, string? month = null) =>
            await _expenseRepo.GetExpenseSummaryAsync(ownerId, branchId, month);

        public async Task<bool> DeleteExpenseAsync(Guid expenseId) =>
            await _expenseRepo.DeleteAsync(expenseId);
    }
}
