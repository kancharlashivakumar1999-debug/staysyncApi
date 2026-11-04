using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Room
    {
        public Guid RoomId { get; set; } = Uuid7.NewGuid();
        public Guid FloorId { get; set; }
        public Guid BranchId { get; set; }
        public string RoomCode { get; set; } = string.Empty; // e.g. R-0001
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = "Shared";
        public int TotalBeds { get; set; } = 2;
        public decimal RoomRent { get; set; } = 0.00m;
        public bool HasAttachedBathroom { get; set; } = true;
        public bool HasAc { get; set; } = false;
        public bool HasBalcony { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Bed> Beds { get; set; } = new();
    }
}
