using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Owner
    {
        public Guid OwnerId { get; set; } = Uuid7.NewGuid();
        public string OwnerCode { get; set; } = string.Empty; // e.g. PO-0001
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int ExpectedBranches { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
