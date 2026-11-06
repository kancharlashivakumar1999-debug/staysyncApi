using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IGuestRepository
    {
        Task<IEnumerable<Guest>> GetByOwnerIdAsync(Guid ownerId, Guid? branchId = null);
        Task<Guest?> GetByIdAsync(Guid guestId);
        Task<Guid> CreateAsync(Guest guest);
        Task<Guid> CheckInGuestWithOccupancyAsync(Guest guest);
        Task SettleAndVacateAsync(Guid guestId, decimal returnedAmount, decimal deductionsAmount, string? notes, DateTime vacateDate);
    }
}
