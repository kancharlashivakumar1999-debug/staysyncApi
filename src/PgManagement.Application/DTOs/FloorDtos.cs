using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateFloorDto
    {
        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public int FloorNumber { get; set; }

        [Required]
        public string FloorName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
