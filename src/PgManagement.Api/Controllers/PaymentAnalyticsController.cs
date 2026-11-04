using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentAnalyticsController : ControllerBase
    {
        private readonly IPaymentAnalyticsService _service;

        public PaymentAnalyticsController(IPaymentAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("overview/{ownerId:guid}")]
        public async Task<ActionResult> GetOverview(Guid ownerId, [FromQuery] Guid? branchId, [FromQuery] int? year)
        {
            int queryYear = year ?? DateTime.UtcNow.Year;
            return Ok(await _service.GetOverviewAnalyticsAsync(ownerId, branchId, queryYear));
        }

        [HttpGet("hierarchy/{ownerId:guid}")]
        public async Task<ActionResult> GetHierarchy(Guid ownerId, [FromQuery] string? level, [FromQuery] Guid? branchId, [FromQuery] Guid? parentId)
        {
            return Ok(await _service.GetHierarchyCollectionsAsync(ownerId, level ?? "hostel", branchId, parentId));
        }

        [HttpGet("bed-history/{bedId:guid}")]
        public async Task<ActionResult> GetBedHistory(Guid bedId)
        {
            var history = await _service.GetBedLifetimeHistoryAsync(bedId);
            if (history == null) return NotFound(new { message = "Bed not found." });
            return Ok(history);
        }
    }
}
