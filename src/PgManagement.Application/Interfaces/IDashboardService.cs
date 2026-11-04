using PgManagement.Application.DTOs;

namespace PgManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(Guid ownerId);
    }
}
