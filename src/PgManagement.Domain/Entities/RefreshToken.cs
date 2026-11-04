using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class RefreshToken
    {
        public Guid TokenId { get; set; } = Uuid7.NewGuid();
        public Guid OwnerId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
    }
}
