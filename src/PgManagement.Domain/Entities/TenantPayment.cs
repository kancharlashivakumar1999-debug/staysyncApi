using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class TenantPayment
    {
        public Guid PaymentId { get; set; } = Uuid7.NewGuid();
        public string PaymentCode { get; set; } = string.Empty; // PAY-0001
        public Guid? ChargeId { get; set; }
        public Guid GuestId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMode { get; set; } = "UPI"; // UPI, Cash, Bank Transfer, Online
        public string? TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReceivedBy { get; set; }
        public string? ReceiptNotes { get; set; }
        public string? PaymentMonth { get; set; } // '2026-10' or '2026-11'
        public int DaysPaid { get; set; } = 30; // user entered or defaulted days
        public decimal AdvanceAllocated { get; set; } = 0.00m;
        public decimal RentAllocated { get; set; } = 0.00m;
        public decimal PendingBalanceAfter { get; set; } = 0.00m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
