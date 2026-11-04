using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Bed
    {
        public Guid BedId { get; set; } = Uuid7.NewGuid();
        public Guid RoomId { get; set; }
        public Guid FloorId { get; set; }
        public Guid BranchId { get; set; }
        public string BedCode { get; set; } = string.Empty; // e.g. BED-0001
        public string BedNumber { get; set; } = string.Empty;
        public decimal BedRent { get; set; } = 0.00m;
        public decimal DailyRate { get; set; } = 0.00m;
        public bool IsOccupied { get; set; } = false;
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
