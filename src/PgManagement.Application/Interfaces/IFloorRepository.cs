using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IFloorRepository
    {
        Task<IEnumerable<Floor>> GetByBranchIdAsync(Guid branchId);
        Task<Floor?> GetByIdAsync(Guid floorId);
        Task<Guid> CreateAsync(Floor floor);
        Task<bool> DeleteAsync(Guid floorId);
    }
}
