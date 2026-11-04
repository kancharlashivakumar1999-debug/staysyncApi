namespace PgManagement.Application.DTOs
{
    public class GuestDto
    {
        public Guid GuestId { get; set; }
        public string GuestCode { get; set; } = string.Empty;
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
        public decimal AdvanceAmount { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DailyRent { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ExpectedVacatingDate { get; set; }
        public int DaysBeforeVacateReminder { get; set; } = 15;
        public bool VacateNoticeServed { get; set; }
        public decimal? VacatingReturnedAmount { get; set; }
        public decimal? DeductionsAmount { get; set; }
        public string? SettlementNotes { get; set; }
        public string? ProofDocumentType { get; set; }
        public string? ProofDocumentNumber { get; set; }
        public string? ProofDocumentUrl { get; set; }
        public string Status { get; set; } = "Active";

        // Display mappings
        public string? RoomNumber { get; set; }
        public string? BedNumber { get; set; }
        public string? BranchName { get; set; }
        public string? FloorName { get; set; }
    }

    public class VacateSettlementDto
    {
        public decimal ReturnedAmount { get; set; }
        public decimal DeductionsAmount { get; set; }
        public string? SettlementNotes { get; set; }
        public DateTime ActualVacatingDate { get; set; } = DateTime.UtcNow;
    }
}
