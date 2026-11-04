using PgManagement.Application.DTOs;

namespace PgManagement.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDto> GetSummaryAsync(Guid ownerId);
    }
}
