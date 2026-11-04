using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateComplaintDto
    {
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        public Guid? GuestId { get; set; }
        public Guid? RoomId { get; set; }
        [Required]
        public string Category { get; set; } = "Plumbing";
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
    }

    public class UpdateComplaintStatusDto
    {
        [Required]
        public string Status { get; set; } = "In Progress"; // New, Assigned, In Progress, Resolved, Closed
        public string? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class ComplaintDto
    {
        public Guid ComplaintId { get; set; }
        public string ComplaintCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? GuestId { get; set; }
        public string? GuestName { get; set; }
        public Guid? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
