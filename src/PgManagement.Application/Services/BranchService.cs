using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepo;
        private readonly IFloorRepository _floorRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IOwnerRepository _ownerRepo;

        public BranchService(IBranchRepository branchRepo, IFloorRepository floorRepo, IRoomRepository roomRepo, IOwnerRepository ownerRepo)
        {
            _branchRepo = branchRepo;
            _floorRepo = floorRepo;
            _roomRepo = roomRepo;
            _ownerRepo = ownerRepo;
        }

        public async Task<IEnumerable<Branch>> GetBranchesHierarchyAsync(Guid ownerId)
        {
            var branches = (await _branchRepo.GetByOwnerIdAsync(ownerId)).ToList();
            foreach (var b in branches)
            {
                var floors = (await _floorRepo.GetByBranchIdAsync(b.BranchId)).ToList();
                foreach (var f in floors)
                {
                    var rooms = (await _roomRepo.GetByFloorIdAsync(f.FloorId)).ToList();
                    foreach (var r in rooms)
                    {
                        r.Beds = (await _roomRepo.GetBedsByRoomIdAsync(r.RoomId)).ToList();
                    }
                    f.Rooms = rooms;
                }
                b.Floors = floors;
            }
            return branches;
        }

        public async Task<Branch?> GetBranchByIdAsync(Guid branchId) => await _branchRepo.GetByIdAsync(branchId);

        public async Task<Branch> CreateBranchAsync(CreateBranchDto dto)
        {
            var owner = await _ownerRepo.GetByIdAsync(dto.OwnerId);
            if (owner != null)
            {
                var existingBranches = await _branchRepo.GetByOwnerIdAsync(dto.OwnerId);
                if (existingBranches.Count() >= owner.ExpectedBranches)
                {
                    throw new InvalidOperationException($"Branch limit exceeded! Your current profile allows a maximum of {owner.ExpectedBranches} property(ies). Please update 'Expected Branches' in your profile to add more.");
                }
            }

            var branch = new Branch
            {
                OwnerId = dto.OwnerId,
                BranchName = dto.BranchName,
                ContactNumber = dto.ContactNumber,
                AddressLine = dto.AddressLine,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,
                TotalFloors = dto.TotalFloors > 0 ? dto.TotalFloors : 1
            };

            // Prepare floor definitions
            var floorsToCreate = new List<Floor>();
            for (int i = 0; i < branch.TotalFloors; i++)
            {
                var floorName = i == 0 ? "Ground Floor" : $"{i}st Floor";
                if (i == 2) floorName = "2nd Floor";
                else if (i == 3) floorName = "3rd Floor";
                else if (i > 3) floorName = $"{i}th Floor";

                floorsToCreate.Add(new Floor
                {
                    FloorNumber = i,
                    FloorName = floorName,
                    Description = $"{floorName} layout"
                });
            }

            // Create Branch + all initial Floors atomically under a single database transaction
            var createdBranch = await _branchRepo.CreateBranchWithFloorsAsync(branch, floorsToCreate);
            return createdBranch;
        }

        public async Task<bool> DeleteBranchAsync(Guid branchId) => await _branchRepo.DeleteAsync(branchId);
    }
}
