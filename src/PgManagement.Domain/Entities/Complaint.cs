using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Complaint
    {
        public Guid ComplaintId { get; set; } = Uuid7.NewGuid();
        public string ComplaintCode { get; set; } = string.Empty; // CMP-0001
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public Guid? GuestId { get; set; }
        public Guid? RoomId { get; set; }
        public string Category { get; set; } = "Plumbing"; // Plumbing, Electrical, WiFi / Internet, Food / Mess, Cleaning, Other
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Emergency
        public string Status { get; set; } = "New"; // New, Assigned, In Progress, Resolved, Closed
        public string? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
