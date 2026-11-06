using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IBranchRepository
    {
        Task<IEnumerable<Branch>> GetByOwnerIdAsync(Guid ownerId);
        Task<Branch?> GetByIdAsync(Guid branchId);
        Task<Guid> CreateAsync(Branch branch);
        Task<Branch> CreateBranchWithFloorsAsync(Branch branch, List<Floor> floors);
        Task<bool> DeleteAsync(Guid branchId);
    }
}
