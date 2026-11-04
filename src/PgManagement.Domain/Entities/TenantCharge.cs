using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class TenantCharge
    {
        public Guid ChargeId { get; set; } = Uuid7.NewGuid();
        public string ChargeCode { get; set; } = string.Empty; // CHG-0001
        public Guid GuestId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string ChargeType { get; set; } = "Rent"; // Rent, Electricity, Food, Laundry, Deposit, Maintenance, Other
        public string BillingMonth { get; set; } = string.Empty; // 2026-10
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
