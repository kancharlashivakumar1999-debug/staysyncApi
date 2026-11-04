using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expense;

        public ExpenseController(IExpenseService expense)
        {
            _expense = expense;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetExpenses(
            Guid ownerId,
            [FromQuery] Guid? branchId,
            [FromQuery] string? category,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            return Ok(await _expense.GetExpensesAsync(ownerId, branchId, category, fromDate, toDate));
        }

        [HttpGet("summary/{ownerId:guid}")]
        public async Task<ActionResult> GetExpenseSummary(
            Guid ownerId,
            [FromQuery] Guid? branchId,
            [FromQuery] string? month)
        {
            return Ok(await _expense.GetExpenseSummaryAsync(ownerId, branchId, month));
        }

        [HttpPost]
        public async Task<ActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
        {
            return Ok(await _expense.CreateExpenseAsync(dto));
        }

        [HttpDelete("{expenseId:guid}")]
        public async Task<ActionResult> DeleteExpense(Guid expenseId)
        {
            return Ok(await _expense.DeleteExpenseAsync(expenseId));
        }
    }
}
