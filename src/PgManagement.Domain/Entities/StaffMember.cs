using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class StaffMember
    {
        public Guid StaffId { get; set; } = Uuid7.NewGuid();
        public string StaffCode { get; set; } = string.Empty; // ST-0001
        public Guid OwnerId { get; set; }
        public Guid? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Reception"; // Manager, Reception, Staff, Warden
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
