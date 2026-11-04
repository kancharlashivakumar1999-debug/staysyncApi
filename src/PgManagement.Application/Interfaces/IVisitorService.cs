using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IVisitorRepository
    {
        Task<Guid> CreateAsync(Visitor visitor);
        Task<IEnumerable<VisitorDto>> GetVisitorsAsync(Guid ownerId, Guid? branchId = null, Guid? guestId = null, DateTime? date = null);
        Task<bool> CheckOutVisitorAsync(Guid visitorId, DateTime checkOutTime);
    }

    public interface IVisitorService
    {
        Task<VisitorDto> LogVisitorAsync(LogVisitorDto dto);
        Task<IEnumerable<VisitorDto>> GetVisitorsAsync(Guid ownerId, Guid? branchId = null, Guid? guestId = null, DateTime? date = null);
        Task<bool> CheckOutVisitorAsync(Guid visitorId);
    }
}
