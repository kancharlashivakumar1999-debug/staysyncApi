using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateBranchDto
    {
        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        public string BranchName { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }

        [Required]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;
        public string? Pincode { get; set; }
        public int TotalFloors { get; set; } = 1;
    }
}
