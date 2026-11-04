using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet("summary/{ownerId:guid}")]
        public async Task<ActionResult> GetSummary(Guid ownerId)
        {
            return Ok(await _dashboard.GetSummaryAsync(ownerId));
        }
    }
}
