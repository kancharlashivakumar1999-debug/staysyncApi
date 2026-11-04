using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetRoomsByFloorIdAsync(Guid floorId);
        Task<Room> CreateRoomAsync(CreateRoomDto dto);
        Task<Room> UpdateRoomAsync(Guid roomId, UpdateRoomDto dto);
        Task<Bed> AddBedAsync(CreateBedDto dto);
        Task<IEnumerable<Bed>> GetBedsByRoomIdAsync(Guid roomId);
        Task<bool> DeleteRoomAsync(Guid roomId);
    }
}
