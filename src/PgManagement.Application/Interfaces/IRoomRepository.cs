using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetByFloorIdAsync(Guid floorId);
        Task<Room?> GetByIdAsync(Guid roomId);
        Task<Guid> CreateAsync(Room room);
        Task<bool> UpdateAsync(Guid roomId, UpdateRoomDto dto);
        Task<bool> DeleteAsync(Guid roomId);

        Task<IEnumerable<Bed>> GetBedsByRoomIdAsync(Guid roomId);
        Task<Bed?> GetBedByIdAsync(Guid bedId);
        Task<Guid> CreateBedAsync(Bed bed);
        Task UpdateBedOccupancyAsync(Guid bedId, bool isOccupied, string status);
        Task AdjustBedsForRoomAsync(Guid roomId, Guid floorId, Guid branchId, string roomNumber, int targetBeds, decimal bedRent, decimal dailyRate);
    }
}
