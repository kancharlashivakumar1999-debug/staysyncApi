using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class MonthlyClosing
    {
        public Guid ClosingId { get; set; } = Uuid7.NewGuid();
        public string ClosingCode { get; set; } = string.Empty; // MCL-0001
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string ClosingMonth { get; set; } = string.Empty; // 2026-10
        public decimal TotalRentCollected { get; set; }
        public decimal TotalOtherIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetOperatingIncome { get; set; }
        public decimal TotalOutstandingDues { get; set; }
        public int ActiveTenantsCount { get; set; }
        public decimal OccupancyRate { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime ClosedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
