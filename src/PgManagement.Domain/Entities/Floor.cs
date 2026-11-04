using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Floor
    {
        public Guid FloorId { get; set; } = Uuid7.NewGuid();
        public Guid BranchId { get; set; }
        public string FloorCode { get; set; } = string.Empty; // e.g. FL-0001
        public int FloorNumber { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Room> Rooms { get; set; } = new();
    }
}
