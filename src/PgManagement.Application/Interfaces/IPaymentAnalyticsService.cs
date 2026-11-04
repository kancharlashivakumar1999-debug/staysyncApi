using PgManagement.Application.DTOs;

namespace PgManagement.Application.Interfaces
{
    public interface IPaymentAnalyticsRepository
    {
        Task<PaymentAnalyticsOverviewDto> GetOverviewAnalyticsAsync(Guid ownerId, Guid? branchId, int year);
        Task<IEnumerable<HierarchyCollectionItemDto>> GetHostelCollectionsAsync(Guid ownerId);
        Task<IEnumerable<HierarchyCollectionItemDto>> GetFloorCollectionsAsync(Guid ownerId, Guid? branchId);
        Task<IEnumerable<HierarchyCollectionItemDto>> GetRoomCollectionsAsync(Guid ownerId, Guid? floorId, Guid? branchId);
        Task<IEnumerable<HierarchyCollectionItemDto>> GetBedCollectionsAsync(Guid ownerId, Guid? roomId, Guid? branchId);
        Task<BedLifetimeHistoryDto?> GetBedLifetimeHistoryAsync(Guid bedId);
    }

    public interface IPaymentAnalyticsService
    {
        Task<PaymentAnalyticsOverviewDto> GetOverviewAnalyticsAsync(Guid ownerId, Guid? branchId, int year);
        Task<IEnumerable<HierarchyCollectionItemDto>> GetHierarchyCollectionsAsync(Guid ownerId, string level, Guid? branchId, Guid? parentId);
        Task<BedLifetimeHistoryDto?> GetBedLifetimeHistoryAsync(Guid bedId);
    }
}
