using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Expense
    {
        public Guid ExpenseId { get; set; } = Uuid7.NewGuid();
        public string ExpenseCode { get; set; } = string.Empty; // EXP-0001
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string Category { get; set; } = "Utilities"; // Utilities, Maintenance, Food Supplies, Staff Salary, Internet, Cleaning, Other
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.Date;
        public string? VendorName { get; set; }
        public string PaymentMode { get; set; } = "UPI";
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
