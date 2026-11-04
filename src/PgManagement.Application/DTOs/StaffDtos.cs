using System.ComponentModel.DataAnnotations;

namespace PgManagement.Application.DTOs
{
    public class CreateStaffDto
    {
        [Required]
        public Guid OwnerId { get; set; }
        public Guid? BranchId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Reception";
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;
    }

    public class StaffDto
    {
        public Guid StaffId { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; }
    }
}
