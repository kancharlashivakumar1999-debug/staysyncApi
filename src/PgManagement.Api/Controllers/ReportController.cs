using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _report;

        public ReportController(IReportService report)
        {
            _report = report;
        }

        [HttpGet("pl/{ownerId:guid}")]
        public async Task<ActionResult> GetMonthlyPL(
            Guid ownerId,
            [FromQuery] Guid? branchId,
            [FromQuery] string month)
        {
            if (string.IsNullOrWhiteSpace(month))
            {
                month = DateTime.UtcNow.ToString("yyyy-MM");
            }
            return Ok(await _report.GetMonthlyPLReportAsync(ownerId, branchId, month));
        }

        [HttpGet("closing-history/{ownerId:guid}")]
        public async Task<ActionResult> GetClosingHistory(
            Guid ownerId,
            [FromQuery] Guid? branchId)
        {
            return Ok(await _report.GetClosingHistoryAsync(ownerId, branchId));
        }

        [HttpPost("close-month")]
        public async Task<ActionResult> CloseMonth([FromBody] PerformClosingDto dto)
        {
            return Ok(await _report.CloseMonthAsync(dto));
        }
    }
}
