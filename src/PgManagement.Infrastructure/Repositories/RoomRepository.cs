using Dapper;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly IDbConnectionFactory _factory;

        public RoomRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string RoomProjectionSql = """
            SELECT r.room_id AS RoomId,
                   r.floor_id AS FloorId,
                   r.branch_id AS BranchId,
                   r.room_code AS RoomCode,
                   r.room_number AS RoomNumber,
                   r.room_type AS RoomType,
                   r.total_beds AS TotalBeds,
                   r.room_rent AS RoomRent,
                   r.has_attached_bathroom AS HasAttachedBathroom,
                   r.has_ac AS HasAc,
                   r.has_balcony AS HasBalcony,
                   r.is_active AS IsActive,
                   r.created_at AS CreatedAt
              FROM rooms r
            """;

        private const string BedProjectionSql = """
            SELECT b.bed_id AS BedId,
                   b.room_id AS RoomId,
                   b.floor_id AS FloorId,
                   b.branch_id AS BranchId,
                   b.bed_code AS BedCode,
                   b.bed_number AS BedNumber,
                   b.bed_rent AS BedRent,
                   b.daily_rate AS DailyRate,
                   b.is_occupied AS IsOccupied,
                   b.status AS Status,
                   b.created_at AS CreatedAt
              FROM beds b
            """;

        public async Task<IEnumerable<Room>> GetByFloorIdAsync(Guid floorId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{RoomProjectionSql} WHERE r.floor_id = @FloorId ORDER BY r.room_code ASC;";
            return await conn.QueryAsync<Room>(sql, new { FloorId = floorId });
        }

        public async Task<Room?> GetByIdAsync(Guid roomId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{RoomProjectionSql} WHERE r.room_id = @RoomId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Room>(sql, new { RoomId = roomId });
        }

        public async Task<Guid> CreateAsync(Room room)
        {
            using var conn = _factory.CreateConnection();
            room.RoomId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO rooms (room_id, floor_id, branch_id, room_code, room_number, room_type, total_beds, room_rent, has_attached_bathroom, has_ac, has_balcony, is_active, created_at)
                VALUES (@RoomId, @FloorId, @BranchId, fn_next_room_code(), @RoomNumber, @RoomType, @TotalBeds, @RoomRent, @HasAttachedBathroom, @HasAc, @HasBalcony, @IsActive, @CreatedAt)
                RETURNING room_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, room);
        }

        public async Task<bool> UpdateAsync(Guid roomId, UpdateRoomDto dto)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE rooms
                SET room_number = @RoomNumber,
                    room_type = @RoomType,
                    total_beds = @TotalBeds,
                    room_rent = @RoomRent,
                    has_attached_bathroom = @HasAttachedBathroom,
                    has_ac = @HasAc,
                    has_balcony = @HasBalcony,
                    is_active = @IsActive
                WHERE room_id = @RoomId;";

            return await conn.ExecuteAsync(sql, new
            {
                RoomId = roomId,
                dto.RoomNumber,
                dto.RoomType,
                dto.TotalBeds,
                dto.RoomRent,
                dto.HasAttachedBathroom,
                dto.HasAc,
                dto.HasBalcony,
                dto.IsActive
            }) > 0;
        }

        public async Task<bool> DeleteAsync(Guid roomId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM rooms WHERE room_id = @RoomId;";
            return await conn.ExecuteAsync(sql, new { RoomId = roomId }) > 0;
        }

        public async Task<IEnumerable<Bed>> GetBedsByRoomIdAsync(Guid roomId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{BedProjectionSql} WHERE b.room_id = @RoomId ORDER BY b.bed_code ASC;";
            return await conn.QueryAsync<Bed>(sql, new { RoomId = roomId });
        }

        public async Task<Bed?> GetBedByIdAsync(Guid bedId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{BedProjectionSql} WHERE b.bed_id = @BedId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Bed>(sql, new { BedId = bedId });
        }

        public async Task<Guid> CreateBedAsync(Bed bed)
        {
            using var conn = _factory.CreateConnection();
            bed.BedId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO beds (bed_id, room_id, floor_id, branch_id, bed_code, bed_number, bed_rent, daily_rate, is_occupied, status, created_at)
                VALUES (@BedId, @RoomId, @FloorId, @BranchId, fn_next_bed_code(), @BedNumber, @BedRent, @DailyRate, @IsOccupied, @Status, @CreatedAt)
                RETURNING bed_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, bed);
        }

        public async Task UpdateBedOccupancyAsync(Guid bedId, bool isOccupied, string status)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE beds SET is_occupied = @IsOccupied, status = @Status WHERE bed_id = @BedId;";
            await conn.ExecuteAsync(sql, new { BedId = bedId, IsOccupied = isOccupied, Status = status });
        }

        public async Task AdjustBedsForRoomAsync(Guid roomId, Guid floorId, Guid branchId, string roomNumber, int targetBeds, decimal bedRent, decimal dailyRate)
        {
            using var conn = _factory.CreateConnection();
            var existingBeds = (await GetBedsByRoomIdAsync(roomId)).ToList();

            // Update existing beds rent
            const string updateRentSql = "UPDATE beds SET bed_rent = @BedRent, daily_rate = @DailyRate WHERE room_id = @RoomId;";
            await conn.ExecuteAsync(updateRentSql, new { RoomId = roomId, BedRent = bedRent, DailyRate = dailyRate });

            if (existingBeds.Count < targetBeds)
            {
                // Add missing beds
                char letter = (char)('A' + existingBeds.Count);
                for (int i = existingBeds.Count + 1; i <= targetBeds; i++)
                {
                    var bed = new Bed
                    {
                        RoomId = roomId,
                        FloorId = floorId,
                        BranchId = branchId,
                        BedNumber = $"{roomNumber}-{letter++}",
                        BedRent = bedRent,
                        DailyRate = dailyRate,
                        IsOccupied = false,
                        Status = "Available"
                    };
                    await CreateBedAsync(bed);
                }
            }
            else if (existingBeds.Count > targetBeds)
            {
                // Remove excess unoccupied beds
                int bedsToRemove = existingBeds.Count - targetBeds;
                var unoccupied = existingBeds.Where(b => !b.IsOccupied).Take(bedsToRemove).ToList();
                foreach (var b in unoccupied)
                {
                    await conn.ExecuteAsync("DELETE FROM beds WHERE bed_id = @BedId;", new { BedId = b.BedId });
                }
            }
        }
    }
}
