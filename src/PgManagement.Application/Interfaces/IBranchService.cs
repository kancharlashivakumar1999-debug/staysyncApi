using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IBranchService
    {
        Task<IEnumerable<Branch>> GetBranchesHierarchyAsync(Guid ownerId);
        Task<Branch?> GetBranchByIdAsync(Guid branchId);
        Task<Branch> CreateBranchAsync(CreateBranchDto dto);
        Task<bool> DeleteBranchAsync(Guid branchId);
    }
}
