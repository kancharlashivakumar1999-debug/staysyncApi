using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;

        public DashboardService(IDashboardRepository dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid ownerId) => await _dashboardRepo.GetSummaryAsync(ownerId);
    }
}
