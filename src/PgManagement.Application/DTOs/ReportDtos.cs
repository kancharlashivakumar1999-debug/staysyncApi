using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class PerformClosingDto
    {
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public string ClosingMonth { get; set; } = string.Empty; // e.g. 2026-10
        public string? ClosedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class MonthlyClosingDto
    {
        public Guid ClosingId { get; set; }
        public string ClosingCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public string ClosingMonth { get; set; } = string.Empty;
        public decimal TotalRentCollected { get; set; }
        public decimal TotalOtherIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetOperatingIncome { get; set; }
        public decimal TotalOutstandingDues { get; set; }
        public int ActiveTenantsCount { get; set; }
        public decimal OccupancyRate { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime ClosedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class FinancialProfitLossDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal RentCollected { get; set; }
        public decimal UtilitiesCollected { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal TotalOutstandingDues { get; set; }
        public decimal OccupancyPercentage { get; set; }
    }
}
