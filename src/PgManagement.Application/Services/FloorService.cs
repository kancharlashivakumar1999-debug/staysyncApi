using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class FloorService : IFloorService
    {
        private readonly IFloorRepository _floorRepo;

        public FloorService(IFloorRepository floorRepo)
        {
            _floorRepo = floorRepo;
        }

        public async Task<IEnumerable<Floor>> GetFloorsByBranchIdAsync(Guid branchId) => await _floorRepo.GetByBranchIdAsync(branchId);

        public async Task<Floor> CreateFloorAsync(CreateFloorDto dto)
        {
            var floor = new Floor
            {
                BranchId = dto.BranchId,
                FloorNumber = dto.FloorNumber,
                FloorName = dto.FloorName,
                Description = dto.Description
            };
            var id = await _floorRepo.CreateAsync(floor);
            return (await _floorRepo.GetByIdAsync(id)) ?? floor;
        }

        public async Task<bool> DeleteFloorAsync(Guid floorId) => await _floorRepo.DeleteAsync(floorId);
    }
}
