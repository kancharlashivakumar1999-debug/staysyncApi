namespace PgManagement.Application.DTOs
{
    public class PaymentAnalyticsOverviewDto
    {
        public decimal CurrentMonthTotal { get; set; }
        public int CurrentMonthTransactionCount { get; set; }
        public decimal YearTotal { get; set; }
        public int YearTransactionCount { get; set; }
        public decimal LifetimeTotalCollected { get; set; }
        public List<MonthlyCollectionTrendDto> MonthlyTrends { get; set; } = new();
    }

    public class MonthlyCollectionTrendDto
    {
        public string MonthKey { get; set; } = string.Empty; // e.g. "2026-10"
        public string MonthLabel { get; set; } = string.Empty; // e.g. "Oct 2026"
        public int MonthNumber { get; set; }
        public int YearNumber { get; set; }
        public decimal TotalCollected { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class HierarchyCollectionItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // "Hostel", "Floor", "Room", "Bed"
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int LifetimeGuestsCount { get; set; }
        public decimal TotalAmountCollected { get; set; } // Non-advance rent/dues only
        public decimal CurrentMonthAmountCollected { get; set; }
    }

    public class BedLifetimeHistoryDto
    {
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public decimal BedRent { get; set; }
        public bool IsOccupied { get; set; }
        public string Status { get; set; } = "Available";
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public Guid FloorId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalAmountCollectedTillNow { get; set; } // Only amount collected (not advance)
        public int TotalGuestsStayed { get; set; }
        public List<BedGuestStayHistoryDto> GuestsHistory { get; set; } = new();
        public List<TenantPaymentDto> PaymentLogs { get; set; } = new();
    }

    public class BedGuestStayHistoryDto
    {
        public Guid GuestId { get; set; }
        public string GuestCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public bool IsCurrentGuest { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? VacatingDate { get; set; }
        public decimal MonthlyRentAtStay { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal RentCollectedFromGuest { get; set; } // Rent only (excluding deposit)
    }
}
