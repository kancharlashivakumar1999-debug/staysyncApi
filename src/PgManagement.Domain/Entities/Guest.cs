using PgManagement.Domain.Common;

namespace PgManagement.Domain.Entities
{
    public class Guest
    {
        public Guid GuestId { get; set; } = Uuid7.NewGuid();
        public string GuestCode { get; set; } = string.Empty; // e.g. G-0001
        public Guid OwnerId { get; set; }
        public Guid BranchId { get; set; }
        public Guid FloorId { get; set; }
        public Guid RoomId { get; set; }
        public Guid BedId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? EmergencyContact { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? WorkplaceOrCollege { get; set; }
        public string TenantType { get; set; } = "Monthly";
        public decimal AdvanceAmount { get; set; } = 0.00m;
        public decimal MonthlyRent { get; set; } = 0.00m;
        public decimal DailyRent { get; set; } = 0.00m;
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ExpectedVacatingDate { get; set; }
        public DateTime? ActualVacatingDate { get; set; }
        public int DaysBeforeVacateReminder { get; set; } = 15;
        public bool VacateNoticeServed { get; set; } = false;
        public decimal? VacatingReturnedAmount { get; set; } = 0.00m;
        public decimal? DeductionsAmount { get; set; } = 0.00m;
        public string? SettlementNotes { get; set; }
        public string? ProofDocumentType { get; set; }
        public string? ProofDocumentNumber { get; set; }
        public string? ProofDocumentUrl { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
