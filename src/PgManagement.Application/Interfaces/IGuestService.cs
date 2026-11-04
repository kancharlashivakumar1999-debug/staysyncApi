using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IGuestService
    {
        Task<IEnumerable<GuestDto>> GetGuestsAsync(Guid ownerId, Guid? branchId = null);
        Task<Guest> CheckInGuestAsync(GuestDto dto);
        Task<bool> VacateGuestAsync(Guid guestId, VacateSettlementDto dto);
    }
}
