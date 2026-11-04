using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateExpenseDto
    {
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public string Category { get; set; } = "Utilities";
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.Date;
        public string? VendorName { get; set; }
        public string PaymentMode { get; set; } = "UPI";
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class ExpenseDto
    {
        public Guid ExpenseId { get; set; }
        public string ExpenseCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? VendorName { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ExpenseSummaryDto
    {
        public decimal TotalExpenses { get; set; }
        public Dictionary<string, decimal> ByCategory { get; set; } = new();
    }
}
