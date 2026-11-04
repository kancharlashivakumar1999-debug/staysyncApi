using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface ILedgerRepository
    {
        Task<Guid> CreateChargeAsync(TenantCharge charge);
        Task<Guid> RecordPaymentAsync(TenantPayment payment);
        Task<IEnumerable<TenantChargeDto>> GetChargesByGuestIdAsync(Guid guestId);
        Task<IEnumerable<TenantPaymentDto>> GetPaymentsByGuestIdAsync(Guid guestId);
        Task<IEnumerable<TenantChargeDto>> GetPendingChargesByOwnerIdAsync(Guid ownerId, Guid? branchId = null);
        Task<IEnumerable<TenantPaymentDto>> GetRecentPaymentsByOwnerIdAsync(Guid ownerId, Guid? branchId = null, int limit = 50);
        Task MarkChargeAsPaidAsync(Guid chargeId);
        Task<TenantLedgerSummaryDto> GetGuestLedgerSummaryAsync(Guid guestId);
        Task<TenantLedgerSummaryDto?> GetBedLedgerSummaryAsync(Guid bedId);
    }

    public interface ILedgerService
    {
        Task<TenantChargeDto> AddChargeAsync(CreateChargeDto dto);
        Task<TenantPaymentDto> RecordPaymentAsync(RecordPaymentDto dto);
        Task<TenantLedgerSummaryDto> GetGuestLedgerAsync(Guid guestId);
        Task<TenantLedgerSummaryDto?> GetBedLedgerAsync(Guid bedId);
        Task<IEnumerable<TenantChargeDto>> GetPendingDuesAsync(Guid ownerId, Guid? branchId = null);
        Task<IEnumerable<TenantPaymentDto>> GetRecentPaymentsAsync(Guid ownerId, Guid? branchId = null);
    }
}
