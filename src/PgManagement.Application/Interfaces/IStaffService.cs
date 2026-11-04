using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<StaffDto>> GetByOwnerIdAsync(Guid ownerId, Guid? branchId = null);
        Task<StaffMember?> GetByIdAsync(Guid staffId);
        Task<Guid> CreateAsync(StaffMember staff);
        Task<bool> UpdateStatusAsync(Guid staffId, bool isActive);
    }

    public interface IStaffService
    {
        Task<IEnumerable<StaffDto>> GetStaffAsync(Guid ownerId, Guid? branchId = null);
        Task<StaffDto> CreateStaffAsync(CreateStaffDto dto);
        Task<bool> ToggleStaffStatusAsync(Guid staffId, bool isActive);
    }
}
