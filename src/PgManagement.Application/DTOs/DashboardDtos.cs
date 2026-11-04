namespace PgManagement.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalBranches { get; set; }
        public int TotalFloors { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalActiveGuests { get; set; }
        public int VacatingSoonCount { get; set; }
        public decimal TotalMonthlyRevenuePotential { get; set; }
        public decimal AdvanceCollectedTotal { get; set; }
    }
}
