using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Visitor
    {
        public Guid VisitorId { get; set; } = Uuid7.NewGuid();
        public string VisitorCode { get; set; } = string.Empty; // VIS-0001
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public Guid GuestId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string? RelationToGuest { get; set; }
        public string PurposeOfVisit { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public DateTime? CheckOutTime { get; set; }
        public string? IdProofType { get; set; }
        public string? IdProofNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
