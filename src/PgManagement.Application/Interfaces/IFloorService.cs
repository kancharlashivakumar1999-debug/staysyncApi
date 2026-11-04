using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IFloorService
    {
        Task<IEnumerable<Floor>> GetFloorsByBranchIdAsync(Guid branchId);
        Task<Floor> CreateFloorAsync(CreateFloorDto dto);
        Task<bool> DeleteFloorAsync(Guid floorId);
    }
}
