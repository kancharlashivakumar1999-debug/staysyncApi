using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Branch
    {
        public Guid BranchId { get; set; } = Uuid7.NewGuid();
        public Guid OwnerId { get; set; }
        public string BranchCode { get; set; } = string.Empty; // e.g. BR-0001
        public string BranchName { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Pincode { get; set; }
        public int TotalFloors { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Floor> Floors { get; set; } = new();
    }
}
