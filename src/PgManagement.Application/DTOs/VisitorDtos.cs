using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class LogVisitorDto
    {
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public Guid GuestId { get; set; }
        [Required]
        public string VisitorName { get; set; } = string.Empty;
        [Required]
        public string VisitorPhone { get; set; } = string.Empty;
        public string? RelationToGuest { get; set; }
        [Required]
        public string PurposeOfVisit { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public string? IdProofType { get; set; }
        public string? IdProofNumber { get; set; }
    }

    public class VisitorDto
    {
        public Guid VisitorId { get; set; }
        public string VisitorCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid GuestId { get; set; }
        public string? GuestName { get; set; }
        public string? RoomNumber { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string? RelationToGuest { get; set; }
        public string PurposeOfVisit { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? IdProofType { get; set; }
        public string? IdProofNumber { get; set; }
    }
}
