using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        public Guid FloorId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = "Double Sharing";
        public int TotalBeds { get; set; } = 2;
        public decimal RoomRent { get; set; }
        public decimal BedRent { get; set; }
        public decimal DailyRate { get; set; } = 500;
        public bool HasAttachedBathroom { get; set; } = true;
        public bool HasAc { get; set; } = false;
        public bool HasBalcony { get; set; } = false;
        public bool AutoGenerateBeds { get; set; } = true;
    }

    public class UpdateRoomDto
    {
        [Required]
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = "Double Sharing";
        public int TotalBeds { get; set; } = 2;
        public decimal RoomRent { get; set; }
        public decimal BedRent { get; set; }
        public decimal DailyRate { get; set; } = 500;
        public bool HasAttachedBathroom { get; set; } = true;
        public bool HasAc { get; set; } = false;
        public bool HasBalcony { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class CreateBedDto
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid FloorId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public string BedNumber { get; set; } = string.Empty;
        public decimal BedRent { get; set; }
        public decimal DailyRate { get; set; } = 500;
    }
}
