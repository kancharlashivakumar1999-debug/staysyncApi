using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateChargeDto
    {
        [Required]
        public Guid GuestId { get; set; }
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public string ChargeType { get; set; } = "Rent";
        [Required]
        public string BillingMonth { get; set; } = string.Empty; // e.g. 2026-10
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
    }

    public class RecordPaymentDto
    {
        public Guid? ChargeId { get; set; }
        [Required]
        public Guid GuestId { get; set; }
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public decimal AmountPaid { get; set; }
        public string PaymentMode { get; set; } = "UPI";
        public string? TransactionReference { get; set; }
        public string? ReceivedBy { get; set; }
        public string? ReceiptNotes { get; set; }
        public string? PaymentMonth { get; set; }
        public int DaysPaid { get; set; } = 30;
        public decimal AdvanceAllocated { get; set; }
        public decimal RentAllocated { get; set; }
        public decimal PendingBalanceAfter { get; set; }
    }

    public class TenantChargeDto
    {
        public Guid ChargeId { get; set; }
        public string ChargeCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid GuestId { get; set; }
        public string? GuestName { get; set; }
        public string? RoomNumber { get; set; }
        public string? BedNumber { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public string ChargeType { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantPaymentDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public Guid? ChargeId { get; set; }
        public Guid GuestId { get; set; }
        public string? GuestName { get; set; }
        public Guid BranchId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? ReceivedBy { get; set; }
        public string? ReceiptNotes { get; set; }
        public string? PaymentMonth { get; set; }
        public int DaysPaid { get; set; } = 30;
        public decimal AdvanceAllocated { get; set; }
        public decimal RentAllocated { get; set; }
        public decimal PendingBalanceAfter { get; set; }
    }

    public class TenantLedgerSummaryDto
    {
        public Guid GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoomBed { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public Guid RoomId { get; set; }
        public Guid BedId { get; set; }
        public DateTime JoiningDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ProratedJoiningRent { get; set; }
        public int ProratedDaysInMonth { get; set; }
        public int DaysInJoiningMonth { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal AdvancePending { get; set; }
        public decimal RentPaid { get; set; }
        public decimal RentPending { get; set; }
        public decimal DailyRate { get; set; }
        public string CurrentBillingMonth { get; set; } = string.Empty;
        public decimal CurrentMonthRentCharge { get; set; }
        public decimal CurrentMonthPaid { get; set; }
        public decimal CurrentMonthPending { get; set; }
        public List<TenantPaymentDto> CurrentMonthPayments { get; set; } = new();
        public List<TenantChargeDto> Charges { get; set; } = new();
        public List<TenantPaymentDto> Payments { get; set; } = new();
    }
}
