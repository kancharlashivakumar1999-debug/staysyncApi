using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepo;

        public RoomService(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<IEnumerable<Room>> GetRoomsByFloorIdAsync(Guid floorId)
        {
            var rooms = (await _roomRepo.GetByFloorIdAsync(floorId)).ToList();
            foreach (var r in rooms) r.Beds = (await _roomRepo.GetBedsByRoomIdAsync(r.RoomId)).ToList();
            return rooms;
        }

        public async Task<Room> CreateRoomAsync(CreateRoomDto dto)
        {
            var room = new Room
            {
                FloorId = dto.FloorId,
                BranchId = dto.BranchId,
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                TotalBeds = dto.TotalBeds,
                RoomRent = dto.RoomRent,
                HasAttachedBathroom = dto.HasAttachedBathroom,
                HasAc = dto.HasAc,
                HasBalcony = dto.HasBalcony,
                IsActive = true
            };

            var id = await _roomRepo.CreateAsync(room);
            var created = await _roomRepo.GetByIdAsync(id);

            if (dto.AutoGenerateBeds && dto.TotalBeds > 0)
            {
                char letter = 'A';
                for (int i = 1; i <= dto.TotalBeds; i++)
                {
                    var bed = new Bed
                    {
                        RoomId = id,
                        FloorId = dto.FloorId,
                        BranchId = dto.BranchId,
                        BedNumber = $"{room.RoomNumber}-{letter++}",
                        BedRent = dto.BedRent > 0 ? dto.BedRent : (dto.RoomRent / dto.TotalBeds),
                        DailyRate = dto.DailyRate,
                        IsOccupied = false,
                        Status = "Available"
                    };
                    await _roomRepo.CreateBedAsync(bed);
                }
            }
            return created ?? room;
        }

        public async Task<Room> UpdateRoomAsync(Guid roomId, UpdateRoomDto dto)
        {
            var existing = await _roomRepo.GetByIdAsync(roomId);
            if (existing == null) throw new InvalidOperationException("Room not found.");

            await _roomRepo.UpdateAsync(roomId, dto);
            await _roomRepo.AdjustBedsForRoomAsync(roomId, existing.FloorId, existing.BranchId, dto.RoomNumber, dto.TotalBeds, dto.BedRent, dto.DailyRate);

            var updated = await _roomRepo.GetByIdAsync(roomId);
            if (updated != null)
            {
                updated.Beds = (await _roomRepo.GetBedsByRoomIdAsync(roomId)).ToList();
            }
            return updated ?? existing;
        }

        public async Task<Bed> AddBedAsync(CreateBedDto dto)
        {
            var bed = new Bed
            {
                RoomId = dto.RoomId,
                FloorId = dto.FloorId,
                BranchId = dto.BranchId,
                BedNumber = dto.BedNumber,
                BedRent = dto.BedRent,
                DailyRate = dto.DailyRate,
                IsOccupied = false,
                Status = "Available"
            };
            var id = await _roomRepo.CreateBedAsync(bed);
            return (await _roomRepo.GetBedByIdAsync(id)) ?? bed;
        }

        public async Task<IEnumerable<Bed>> GetBedsByRoomIdAsync(Guid roomId) => await _roomRepo.GetBedsByRoomIdAsync(roomId);
        public async Task<bool> DeleteRoomAsync(Guid roomId) => await _roomRepo.DeleteAsync(roomId);
    }
}
