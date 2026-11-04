using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbConnectionFactory _factory;

        public DashboardRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid ownerId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM branches WHERE owner_id = @OwnerId) AS TotalBranches,
                    (SELECT COUNT(*) FROM floors f JOIN branches b ON f.branch_id = b.branch_id WHERE b.owner_id = @OwnerId) AS TotalFloors,
                    (SELECT COUNT(*) FROM rooms r JOIN branches b ON r.branch_id = b.branch_id WHERE b.owner_id = @OwnerId) AS TotalRooms,
                    (SELECT COUNT(*) FROM beds b JOIN branches br ON b.branch_id = br.branch_id WHERE br.owner_id = @OwnerId) AS TotalBeds,
                    (SELECT COUNT(*) FROM beds b JOIN branches br ON b.branch_id = br.branch_id WHERE br.owner_id = @OwnerId AND b.is_occupied = true) AS OccupiedBeds,
                    (SELECT COUNT(*) FROM guests WHERE owner_id = @OwnerId AND status = 'Active') AS TotalActiveGuests,
                    (SELECT COUNT(*) FROM guests WHERE owner_id = @OwnerId AND status = 'Notice_Period') AS VacatingSoonCount,
                    COALESCE((SELECT SUM(bed_rent) FROM beds b JOIN branches br ON b.branch_id = br.branch_id WHERE br.owner_id = @OwnerId), 0) AS TotalMonthlyRevenuePotential,
                    COALESCE((SELECT SUM(advance_amount) FROM guests WHERE owner_id = @OwnerId AND status = 'Active'), 0) AS AdvanceCollectedTotal;
            ";
            var summary = await conn.QueryFirstOrDefaultAsync<DashboardSummaryDto>(sql, new { OwnerId = ownerId });
            if (summary != null)
            {
                summary.AvailableBeds = summary.TotalBeds - summary.OccupiedBeds;
                return summary;
            }
            return new DashboardSummaryDto();
        }
    }
}
