using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<Guid> CreateClosingAsync(MonthlyClosing closing);
        Task<IEnumerable<MonthlyClosingDto>> GetClosingsAsync(Guid ownerId, Guid? branchId = null);
        Task<FinancialProfitLossDto> CalculateMonthlyPLAsync(Guid ownerId, Guid? branchId, string month);
    }

    public interface IReportService
    {
        Task<MonthlyClosingDto> CloseMonthAsync(PerformClosingDto dto);
        Task<IEnumerable<MonthlyClosingDto>> GetClosingHistoryAsync(Guid ownerId, Guid? branchId = null);
        Task<FinancialProfitLossDto> GetMonthlyPLReportAsync(Guid ownerId, Guid? branchId, string month);
    }
}
