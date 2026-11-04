using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepo;

        public StaffService(IStaffRepository staffRepo)
        {
            _staffRepo = staffRepo;
        }

        public async Task<IEnumerable<StaffDto>> GetStaffAsync(Guid ownerId, Guid? branchId = null) =>
            await _staffRepo.GetByOwnerIdAsync(ownerId, branchId);

        public async Task<StaffDto> CreateStaffAsync(CreateStaffDto dto)
        {
            var staff = new StaffMember
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                FullName = dto.FullName,
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                Salary = dto.Salary,
                JoiningDate = dto.JoiningDate,
                IsActive = true
            };

            var id = await _staffRepo.CreateAsync(staff);
            var created = await _staffRepo.GetByIdAsync(id);
            return new StaffDto
            {
                StaffId = created?.StaffId ?? staff.StaffId,
                StaffCode = created?.StaffCode ?? staff.StaffCode,
                OwnerId = staff.OwnerId,
                BranchId = staff.BranchId,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                Role = staff.Role,
                Salary = staff.Salary,
                JoiningDate = staff.JoiningDate,
                IsActive = staff.IsActive
            };
        }

        public async Task<bool> ToggleStaffStatusAsync(Guid staffId, bool isActive) =>
            await _staffRepo.UpdateStatusAsync(staffId, isActive);
    }
}
