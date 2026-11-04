using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetByEmailAsync(string email);
        Task<Owner?> GetByIdAsync(Guid ownerId);
        Task<Guid> CreateAsync(Owner owner);
        Task<bool> UpdateProfileAsync(Guid ownerId, string fullName, string phoneNumber, int expectedBranches);
        Task SaveRefreshTokenAsync(Guid ownerId, string token, DateTime expiresAt);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
    }
}
