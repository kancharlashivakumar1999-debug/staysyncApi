using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepo;

        public ReportService(IReportRepository reportRepo)
        {
            _reportRepo = reportRepo;
        }

        public async Task<MonthlyClosingDto> CloseMonthAsync(PerformClosingDto dto)
        {
            var pl = await _reportRepo.CalculateMonthlyPLAsync(dto.OwnerId, dto.BranchId, dto.ClosingMonth);

            var closing = new MonthlyClosing
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                ClosingMonth = dto.ClosingMonth,
                TotalRentCollected = pl.RentCollected,
                TotalOtherIncome = pl.UtilitiesCollected,
                TotalExpenses = pl.TotalExpenses,
                NetOperatingIncome = pl.NetProfit,
                TotalOutstandingDues = pl.TotalOutstandingDues,
                OccupancyRate = pl.OccupancyPercentage,
                ClosedBy = dto.ClosedBy ?? "Admin",
                ClosedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            var id = await _reportRepo.CreateClosingAsync(closing);
            return new MonthlyClosingDto
            {
                ClosingId = id,
                ClosingCode = closing.ClosingCode,
                OwnerId = closing.OwnerId,
                BranchId = closing.BranchId,
                ClosingMonth = closing.ClosingMonth,
                TotalRentCollected = closing.TotalRentCollected,
                TotalOtherIncome = closing.TotalOtherIncome,
                TotalExpenses = closing.TotalExpenses,
                NetOperatingIncome = closing.NetOperatingIncome,
                TotalOutstandingDues = closing.TotalOutstandingDues,
                OccupancyRate = closing.OccupancyRate,
                ClosedBy = closing.ClosedBy,
                ClosedAt = closing.ClosedAt,
                Notes = closing.Notes
            };
        }

        public async Task<IEnumerable<MonthlyClosingDto>> GetClosingHistoryAsync(Guid ownerId, Guid? branchId = null) =>
            await _reportRepo.GetClosingsAsync(ownerId, branchId);

        public async Task<FinancialProfitLossDto> GetMonthlyPLReportAsync(Guid ownerId, Guid? branchId, string month) =>
            await _reportRepo.CalculateMonthlyPLAsync(ownerId, branchId, month);
    }
}
