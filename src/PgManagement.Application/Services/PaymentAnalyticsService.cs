using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Application.Services
{
    public class PaymentAnalyticsService : IPaymentAnalyticsService
    {
        private readonly IPaymentAnalyticsRepository _repo;

        public PaymentAnalyticsService(IPaymentAnalyticsRepository repo)
        {
            _repo = repo;
        }

        public async Task<PaymentAnalyticsOverviewDto> GetOverviewAnalyticsAsync(Guid ownerId, Guid? branchId, int year)
        {
            if (year <= 2000 || year > 2100) year = DateTime.UtcNow.Year;
            return await _repo.GetOverviewAnalyticsAsync(ownerId, branchId, year);
        }

        public async Task<IEnumerable<HierarchyCollectionItemDto>> GetHierarchyCollectionsAsync(Guid ownerId, string level, Guid? branchId, Guid? parentId)
        {
            level = (level ?? "Hostel").ToLowerInvariant();
            return level switch
            {
                "floor" => await _repo.GetFloorCollectionsAsync(ownerId, branchId),
                "room" => await _repo.GetRoomCollectionsAsync(ownerId, parentId, branchId),
                "bed" => await _repo.GetBedCollectionsAsync(ownerId, parentId, branchId),
                _ => await _repo.GetHostelCollectionsAsync(ownerId)
            };
        }

        public async Task<BedLifetimeHistoryDto?> GetBedLifetimeHistoryAsync(Guid bedId)
        {
            return await _repo.GetBedLifetimeHistoryAsync(bedId);
        }
    }
}
